using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Command
{
    public class MoveCommand : ICommand
    {
        private WorldController controller;
        private int id;
        private Point3D before;
        private Point3D after;

        public MoveCommand(WorldController controller, int id, Point3D before, Point3D after)
        {
            this.controller = controller;
            this.id = id;
            this.before = before;
            this.after = after;
        }

        public void Undo()
        {
            var obj = controller.GetObject(id);
            if (obj == null) return;

            obj.Position = before;
            controller.UpdateObject(obj);
        }

        public void Redo()
        {
            var obj = controller.GetObject(id);
            if (obj == null) return;

            obj.Position = after;
            controller.UpdateObject(obj);
        }
    }


}
