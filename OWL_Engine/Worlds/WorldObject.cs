using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Worlds
{
    public abstract class WorldObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public MeshGeometry3D? Mesh { get; protected set; }

        public Point3D Position { get; set; }

        public Vector3D Scale { get; set; } = new Vector3D(1,1,1);
        public GeometryModel3D? Model { get; set; }
        public Color Color { get; set; } = Colors.LightGray;
    }
}
