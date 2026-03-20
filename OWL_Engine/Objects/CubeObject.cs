using OWL_Engine.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Objects
{
    public class CubeObject : WorldObject 
    {
        public CubeObject()
        {
            Name = "Cube";
            Mesh = CreateCubeMesh();
            Color = Colors.LightGray;
        }
        public MeshGeometry3D CreateCubeMesh()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            //立方体8頂点
            Point3D[] p =
            {
             new Point3D(0,0,0),
             new Point3D(1,0,0),
             new Point3D(1,1,0),
             new Point3D(0,1,0),
             new Point3D(0,0,1),
             new Point3D(1,0,1),
             new Point3D(1,1,1),
             new Point3D(0,1,1)
            };
            //三角形インデックス
            int[] indices =
            {
                0,1,2, 0,2,3, // front
                1,5,6, 1,6,2, // right
                5,4,7, 5,7,6, // back
                4,0,3, 4,3,7, // left
                3,2,6, 3,6,7, // top
                4,5,1, 4,1,0  // bottom
            };

            foreach (var v in p)
            {
                mesh.Positions.Add(v);
            }

            foreach (var v in indices)
            {
                mesh.TriangleIndices.Add(v);
            }
            return mesh;
        }
    }
}
