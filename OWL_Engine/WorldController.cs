using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.CImporter;
using OWL_Engine.Managers;
using OWL_Engine.Objects;
using OWL_Engine.Render;
using OWL_Engine.Worlds;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine
{
    public class WorldController
    {
        private TransFormWorld world = null!;
        private SelectionManager selection;

        private readonly _3DRenderer _renderer;
        private readonly GridMap _grid;

        private readonly Dictionary<int, WorldObject> _objects = new();
        private int nextId = 1;

        public void Initialize()
        {
            world = new TransFormWorld();
        }

        public WorldController(SelectionManager selection, _3DRenderer renderer,GridMap grid)
        {
            this.selection = selection;
            _renderer = renderer;
            _grid = grid;
            Initialize();
            LoadOBJModel("test.obj");
        }

        public void LoadOBJModel(string path)
        {
            MeshGeometry3D mesh = CreateMeshFromOBJFull(path);

            int id = nextId++;

            WorldObject obj = new ImportedObject();
            obj.Id = id;
            obj.SetMesh(mesh);
            obj.Position = new Point3D(0, 0, 0);
            obj.Color = Colors.White;

            // World に登録
            world.TryCreateObject(id, new IntVector3(0, 0, 0), out _);

            // 辞書に登録
            _objects[id] = obj;

            // Renderer に登録
            _renderer.AddObject(obj);
        }

        public MeshGeometry3D CreateMeshFromOBJFull(string path)
        {
            if (!NativeBridge.LoadOBJFull(
                path,
                out IntPtr vPtr, out int vCount,
                out IntPtr uvPtr, out int uvCount,
                out IntPtr nPtr, out int nCount,
                out IntPtr idxPtr, out int idxCount))
            {
                throw new Exception("LoadOBJFull failed");
            }

            var mesh = new MeshGeometry3D();

            // 頂点
            float[] vArr = new float[vCount * 3];
            Marshal.Copy(vPtr, vArr, 0, vArr.Length);
            for (int i = 0; i < vArr.Length; i += 3)
            {
                mesh.Positions.Add(new Point3D(
                    vArr[i + 0],
                    vArr[i + 1],
                    vArr[i + 2]));
            }

            // インデックス
            int[] idxArr = new int[idxCount];
            Marshal.Copy(idxPtr, idxArr, 0, idxArr.Length);
            foreach (var i in idxArr)
                mesh.TriangleIndices.Add(i);

            // UV（あれば）
            if (uvCount > 0 && uvPtr != IntPtr.Zero)
            {
                float[] uvArr = new float[uvCount * 2];
                Marshal.Copy(uvPtr, uvArr, 0, uvArr.Length);
                for (int i = 0; i < uvArr.Length; i += 2)
                {
                    mesh.TextureCoordinates.Add(new System.Windows.Point(
                        uvArr[i + 0],
                        uvArr[i + 1]));
                }
            }

            if (nCount > 0)
            {
                float[] nArr = new float[nCount * 3];
                Marshal.Copy(nPtr, nArr, 0, nArr.Length);
                for (int i = 0; i < nArr.Length; i += 3)
                {
                    mesh.Normals.Add(new Vector3D(
                        nArr[i + 0],
                        nArr[i + 1],
                        nArr[i + 2]));
                }
            }

            return mesh;
        }

        public bool TryCreateObject(PrimitiveType type, IntVector3 pos, out int id)
        {
            id = nextId++;

            // 1. WorldObject を生成
            var obj = ObjectFactory.Create(type);
            obj.Id = id;
            obj.Position = new Point3D(pos.X, pos.Y, pos.Z);

            // 2. TransFormWorld に位置情報を登録
            world.TryCreateObject(id, pos, out var node);

            // 3. WorldObject をアプリ側辞書に登録
            _objects[id] = obj;

            // 4. 描画
            _renderer.AddObject(obj);

            return true;
        }

        public WorldObject? GetObject(int id)
        {
            _objects.TryGetValue(id, out var obj);
            return obj;
        }

        public void Move(int id, IntVector3 dir)
        {
            var obj = GetObject(id);
            if (obj == null) return;

            // 移動量（double）
            double step = _renderer.GridSize;
            var delta = new Vector3D(dir.X * step, dir.Y * step, dir.Z * step);

            // 親の移動
            obj.Position = new Point3D(
                obj.Position.X + delta.X,
                obj.Position.Y + delta.Y,
                obj.Position.Z + delta.Z
            );

            // 子の移動（再帰）
            MoveChildrenRecursive(id, delta);

            // GridMap 更新
            var newCell = new IntVector3(
                (int)Math.Floor(obj.Position.X),
                (int)Math.Floor(obj.Position.Y),
                (int)Math.Floor(obj.Position.Z)
            );

            world.TryMoveObject(id, newCell);

            // Renderer 更新
            _renderer.UpdateObjectTransform(this);
        }

        private void MoveChildrenRecursive(int parentId, Vector3D delta)
        {
            foreach (var child in _objects.Values)
            {
                if (child.ParentId == parentId)
                {
                    // 子の移動
                    child.Position = new Point3D(
                        child.Position.X + delta.X,
                        child.Position.Y + delta.Y,
                        child.Position.Z + delta.Z
                    );

                    // GridMap / TransFormWorld も更新
                    var newCell = new IntVector3(
                        (int)Math.Floor(child.Position.X),
                        (int)Math.Floor(child.Position.Y),
                        (int)Math.Floor(child.Position.Z)
                    );

                    world.TryMoveObject(child.Id, newCell);

                    // 孫も動かす
                    MoveChildrenRecursive(child.Id, delta);
                }
            }
        }

        public TransFormWorld GetWorld()
        {
            return world;
        }

        public void MoveSelectedObject(IntVector3 dir)
        {
            if (!selection.HasSelection) return;

            if (selection.SelectedId is int id)
            {
                var node = world.GetObject(id);
                if (node == null) return;

                var newPos = node.GetWorldPosition() + dir;
                world.TryMoveObject(id, newPos);
            }
        }
        public void SetParent(int childId, int parentId)
        {
            // WorldObject を取得する
            var child = GetObject(childId);
            if (child == null) return;

            child.ParentId = parentId;
        }
        public IEnumerable<WorldObject> GetAllWorldObjects()
        {
            return _objects.Values;
        }
    }
}