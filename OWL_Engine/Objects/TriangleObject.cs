using OWL_Engine.Worlds;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace OWL_Engine.Objects
{
    public class TriangleObject : WorldObject
    {
        public TriangleObject()
        {
            Name = "Triangle";
            Mesh = CreateTrianglePrismMesh();
        }

        private MeshGeometry3D CreateTrianglePrismMesh()
        {
            double thickness = 0.2;

            MeshGeometry3D mesh = new MeshGeometry3D();

            // 上面（三角形）
            Point3D A = new Point3D(0, 0, 0);
            Point3D B = new Point3D(1, 0, 0);
            Point3D C = new Point3D(0.5, 1, 0);

            // 下面（三角形）Z方向に厚みを持たせる
            Point3D A2 = new Point3D(0, 0, thickness);
            Point3D B2 = new Point3D(1, 0, thickness);
            Point3D C2 = new Point3D(0.5, 1, thickness);

            // 頂点追加
            mesh.Positions.Add(A);  //0
            mesh.Positions.Add(B);  //1
            mesh.Positions.Add(C);  //2
            mesh.Positions.Add(A2); //3
            mesh.Positions.Add(B2); //4
            mesh.Positions.Add(C2); //5

            // 上面
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            // 下面
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);

            // 側面1 (A-B)
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(4);

            // 側面2 (B-C)
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);

            // 側面3 (C-A)
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(0);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(3);

            return mesh;
        }
    }
}