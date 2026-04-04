using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Asset;
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
            MeshGeometry3D mesh = CreateMeshFromOBJFull(path, out Color diffuse);

            int id = nextId++;

            WorldObject obj = new ImportedObject();
            obj.Id = id;

            obj.Color = diffuse;      // OBJ の色
            obj.SetMesh(mesh);        // この色でマテリアル生成

            obj.Position = new Point3D(0, 0, 0);
            obj.Type = "OBJ";
            obj.ObjPath = path;

            world.TryCreateObject(id, new IntVector3(0, 0, 0), out _);
            _objects[id] = obj;
            _renderer.AddObject(obj);
        }

        public static MeshGeometry3D CreateMeshFromOBJFull(string path, out Color diffuse)
        {
            if (!NativeBridge.LoadOBJFull(
                path,
                out IntPtr vPtr, out int vCount,
                out IntPtr uvPtr, out int uvCount,
                out IntPtr nPtr, out int nCount,
                out IntPtr idxPtr, out int idxCount,
                out IntPtr colorPtr))
            {
                diffuse = Colors.Gray;
                throw new Exception("LoadOBJFull failed");
            }

            // 色を取り出す
            if (colorPtr != IntPtr.Zero)
            {
                float[] col = new float[3];
                Marshal.Copy(colorPtr, col, 0, 3);
                diffuse = Color.FromRgb(
                    (byte)(col[0] * 255),
                    (byte)(col[1] * 255),
                    (byte)(col[2] * 255));
            }
            else
            {
                diffuse = Colors.Gray;
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

            // UV
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

            // 法線
            if (nCount > 0 && nPtr != IntPtr.Zero)
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

            obj.Type = type.ToString();
            obj.ObjPath = null;

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

        public void LoadFromSaveData(SaveData data)
        {
            _objects.Clear();
            world = new TransFormWorld();
            //nextId = 1;

            foreach (var saved in data.Objects ?? new List<SaveObject>())
            {
                WorldObject obj;

                if (saved.Type == "OBJ")
                {
                    if (string.IsNullOrEmpty(saved.ObjPath))
                        throw new Exception("OBJ のパスが保存されていません");

                    var mesh = CreateMeshFromOBJFull(saved.ObjPath, out Color diffuse);
                    obj = new ImportedObject();

                    // デフォルト色（影が見える）
                    obj.Color = diffuse;

                    // Mesh をセット（Material も作られる）
                    obj.SetMesh(mesh);

                    obj.Type = "OBJ";
                    obj.ObjPath = saved.ObjPath;

                    // Material が null なら補完
                    obj.EnsureMaterial();
                }
                else
                {
                    obj = ObjectFactory.Create(Enum.Parse<PrimitiveType>(saved.Type));
                    obj.EnsureMaterial();
                }

                obj.Id = saved.Id;
                obj.Name = saved.Name;
                obj.Position = new Point3D(saved.X, saved.Y, saved.Z);
                obj.ParentId = saved.ParentId;

                _objects[obj.Id] = obj;

                var cell = new IntVector3(
                    (int)Math.Floor(saved.X),
                    (int)Math.Floor(saved.Y),
                    (int)Math.Floor(saved.Z)
                );

                world.TryCreateObject(obj.Id, cell, out _);
                // ここで Model をいじらない。AddObject に任せる。
                _renderer.AddObject(obj);

                nextId = Math.Max(nextId, obj.Id + 1);
            }

            _renderer.UpdateObjectTransform(this);
        }
        public void UpdateObject(WorldObject obj)
        {
            _objects[obj.Id] = obj;
        }
        public int GetNextId()
        {
            return nextId++;
        }
        public void RemoveObject(int id)
        {
            if (_objects.TryGetValue(id, out var obj))
            {
                _objects.Remove(id);
                _renderer.RemoveObject(id);
                world.RemoveObject(id);
            }
        }

        public void AddObject(WorldObject obj)
        {
            _objects[obj.Id] = obj;

            var cell = new IntVector3(
                (int)Math.Floor(obj.Position.X),
                (int)Math.Floor(obj.Position.Y),
                (int)Math.Floor(obj.Position.Z)
            );

            world.TryCreateObject(obj.Id, cell, out _);
            _renderer.AddObject(obj);
        }

    }
}