using OWL_Engine.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace OWL_Engine.Command
{
    public class CreateCommand : ICommand
    {
        private WorldController controller;
        private WorldObject created;

        public CreateCommand(WorldController controller, WorldObject obj)
        {
            this.controller = controller;
            this.created = obj;
        }

        public void Undo()
        {
            controller.RemoveObject(created.Id);
        }

        public void Redo()
        {
            controller.AddObject(created);
        }
    }


}
