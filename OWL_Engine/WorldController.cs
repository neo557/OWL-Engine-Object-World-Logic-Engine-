using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Worlds;
using OWL_Engine.Managers;
using System;

namespace OWL_Engine
{
    public class WorldController
    {
        private TransFormWorld world = null!;
        private SelectionManager selection;

        public void Initialize()
        {
            world = new TransFormWorld();

        }

        public WorldController(SelectionManager selection)
        {
            this.selection = selection;
        }


        public void Move(int id, IntVector3 dir)
        {
            var node = world.GetObject(id);
            if (node == null) return;

            var newPos = node.GetWorldPosition() + dir;

            world.TryMoveObject(id, newPos);
        }

        public TransFormWorld GetWorld()
        {
            return world;
        }

        public void MoveSelectedObject(IntVector3 dir)
        {
            if(!selection.HasSelection) return;

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