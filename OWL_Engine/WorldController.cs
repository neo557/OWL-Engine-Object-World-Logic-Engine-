using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Managers;
using OWL_Engine.Objects;
using OWL_Engine.Render;
using OWL_Engine.Worlds;
using System;
using System.Windows.Controls;
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

            //  現在のワールド座標（double）
            var pos = obj.Position;

            //  gridSize 分だけ移動
            double step = _renderer.GridSize;

            pos.X += dir.X * step;
            pos.Y += dir.Y * step;
            pos.Z += dir.Z * step;

            // WorldObject の位置を更新
            obj.Position = pos;

            //  GridMap の更新（整数セルに変換）
            var oldCell = new IntVector3(
                (int)Math.Floor(pos.X - dir.X * step),
                (int)Math.Floor(pos.Y - dir.Y * step),
                (int)Math.Floor(pos.Z - dir.Z * step)
            );

            var newCell = new IntVector3(
                (int)Math.Floor(pos.X),
                (int)Math.Floor(pos.Y),
                (int)Math.Floor(pos.Z)
            ); 

            _grid.MoveObject(oldCell, newCell, id);

            //  TransFormWorld の更新（整数セルでOK）
            world.TryMoveObject(id, newCell);

            //  Renderer に反映
            _renderer.UpdateObjectTransform(this);
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

    }
}