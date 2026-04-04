using OWL_Engine.Objects;
using OWL_Engine.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace OWL_Engine.Command
{
    public class DeleteCommand : ICommand
    {
        private WorldController controller;
        private WorldObject snapshot;

        public DeleteCommand(WorldController controller, WorldObject obj)
        {
            this.controller = controller;
            this.snapshot = ObjectSnapshot.FromObject(obj).ToWorldObject();
        }

        public void Undo()
        {
            controller.AddObject(snapshot);
        }

        public void Redo()
        {
            controller.RemoveObject(snapshot.Id);
        }
    }


}
