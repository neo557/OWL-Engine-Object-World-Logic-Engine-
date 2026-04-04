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

        // メッシュ
        public MeshGeometry3D? Mesh { get; set; }

        public Point3D Position { get; set; }

        public Vector3D Scale { get; set; } = new Vector3D(1,1,1);
        public Vector3D Rotation { get; set; } = new Vector3D(0, 0, 0);

        public GeometryModel3D? Model { get; set; }
        // 色
        public Color Color { get; set; }
        public int ParentId { get; set; } = -1;
        // 保存用：種類（Cube / Rectangle / Triangle / OBJ）
        public string Type { get; set; } = "Cube";
        // 保存用：OBJ の場合だけパスを保持
        public string? ObjPath { get; set; } = null;
        public void SetMesh(MeshGeometry3D mesh)
        {
            Mesh = mesh;

            if (Model == null)
                Model = new GeometryModel3D();

            Model.Geometry = mesh;

            var mat = new DiffuseMaterial(new SolidColorBrush(Color));
            Model.Material = mat;
            Model.BackMaterial = mat;
        }

        public void UpdateMaterial()
        {
            if (Model == null)
                Model = new GeometryModel3D();

            var mat = new DiffuseMaterial(new SolidColorBrush(Color));
            Model.Material = mat;
            Model.BackMaterial = mat;
        }
        public void EnsureMaterial()
        {
            if (Model == null)
                return; // ここでは新規生成しない。SetMesh や Factory に任せる

            if (Model.Material == null)
            {
                var c = Color;

                // 何も設定されていない Color のときだけデフォルト色を与える
                if (c.A == 0 && c.R == 0 && c.G == 0 && c.B == 0)
                    c = Colors.Gray;

                var mat = new DiffuseMaterial(new SolidColorBrush(c));
                Model.Material = mat;
                Model.BackMaterial = mat;
            }
        }

    }
}
