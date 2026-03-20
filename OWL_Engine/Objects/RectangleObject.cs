using OWL_Engine.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Objects
{
    public class RectangleObject : WorldObject
    {
        public RectangleObject()
        {
            Name = "Rectangle";
            Mesh = CreateBoxMesh();
        }

        private MeshGeometry3D CreateBoxMesh()
        {
            double width = 2;
            double height = 1;
            double depth = 0.2;

            MeshGeometry3D mesh = new MeshGeometry3D();

            // 8頂点
            Point3D p0 = new Point3D(0, 0, 0);
            Point3D p1 = new Point3D(width, 0, 0);
            Point3D p2 = new Point3D(width, height, 0);
            Point3D p3 = new Point3D(0, height, 0);

            Point3D p4 = new Point3D(0, 0, depth);
            Point3D p5 = new Point3D(width, 0, depth);
            Point3D p6 = new Point3D(width, height, depth);
            Point3D p7 = new Point3D(0, height, depth);

            mesh.Positions = new Point3DCollection { p0, p1, p2, p3, p4, p5, p6, p7 };

            mesh.TriangleIndices = new Int32Collection
        {
            // 前面
            0,1,2, 0,2,3,
            // 背面
            4,6,5, 4,7,6,
            // 底面
            0,4,5, 0,5,1,
            // 上面
            3,2,6, 3,6,7,
            // 右面
            1,5,6, 1,6,2,
            // 左面
            0,3,7, 0,7,4
        };

            return mesh;
        }
    }
}
