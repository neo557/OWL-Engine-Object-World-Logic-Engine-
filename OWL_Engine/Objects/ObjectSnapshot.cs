using OWL_Engine.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Objects
{
    public class ObjectSnapshot
    {
        public int Id;
        public string? Name;
        public Point3D Position;
        public Vector3D Scale;
        public Vector3D Rotation;
        public int ParentId;
        public Color Color;
        public required string Type;
        public string? ObjPath;

        public static ObjectSnapshot FromObject(WorldObject obj)
        {
            return new ObjectSnapshot
            {
                Id = obj.Id,
                Name = obj.Name,
                Position = obj.Position,
                Scale = obj.Scale,
                Rotation = obj.Rotation,
                ParentId = obj.ParentId,
                Color = obj.Color,
                Type = obj.Type,
                ObjPath = obj.ObjPath
            };
        }

        public void ApplyTo(WorldObject obj)
        {
            obj.Name = Name;
            obj.Position = Position;
            obj.Scale = Scale;
            obj.Rotation = Rotation;
            obj.ParentId = ParentId;
            obj.Color = Color;
            obj.Type = Type;
            obj.ObjPath = ObjPath;
        }
        public WorldObject ToWorldObject()
        {
            WorldObject obj;

            if (Type == "OBJ")
                obj = new ImportedObject();
            else
                obj = ObjectFactory.Create(Enum.Parse<PrimitiveType>(Type));

            obj.Id = Id;
            obj.Name = Name;
            obj.Position = Position;
            obj.Scale = Scale;
            obj.Rotation = Rotation;
            obj.ParentId = ParentId;
            obj.Color = Color;
            obj.Type = Type;
            obj.ObjPath = ObjPath;

            return obj;
        }
    }


}
