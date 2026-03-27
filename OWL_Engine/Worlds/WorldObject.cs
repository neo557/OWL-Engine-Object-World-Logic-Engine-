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
        public string? Name { get; set; } = "Object";

        public MeshGeometry3D? Mesh { get; protected set; }

        public Point3D Position { get; set; }

        public Vector3D Scale { get; set; } = new Vector3D(1,1,1);
        public GeometryModel3D? Model { get; set; }
        public Color Color { get; set; } = Colors.LightGray;
        public int ParentId { get; set; } = -1;
        public void SetMesh(MeshGeometry3D mesh)
        {
            Mesh = mesh;

            if (Model == null)
                Model = new GeometryModel3D();

            // MeshGeometry3D をそのまま Geometry にセットするだけでOK
            Model.Geometry = mesh;

            // マテリアル（裏面も同じにする）
            var mat = new DiffuseMaterial(new SolidColorBrush(Color));
            Model.Material = mat;
            Model.BackMaterial = mat;
        }

        
    }
}
