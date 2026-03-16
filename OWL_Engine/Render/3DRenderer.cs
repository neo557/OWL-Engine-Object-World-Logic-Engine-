using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Render
{
    public interface IRenderer
    {
        void Render(World world);
    }
    public class _3DRenderer
    {
        Model3DGroup sceneRoot = new();
        Model3DGroup objectRoot = new();
        Model3DGroup gridRoot = new();
        Model3DGroup cursorRoot = new();

        ModelVisual3D sceneVisual = new();

        HashSet<IntVector3> visibleCells = new();

        public int RenderDistance = 20;
        public int CurrentLayerY = 0;
        public IntVector3? HoverCell = null;

        public int? SelectedObjectID = null;

        GeometryModel3D? cursorModel;
        TranslateTransform3D cursorTransform = new();

        Dictionary<int, GeometryModel3D> objectModels = new();
        Dictionary<int, TranslateTransform3D> objectTransforms = new();

        private Dictionary<int, Material> originalMaterials = new();
        private Dictionary<int, double> originalScale = new();

        Dictionary<IntVector3, List<int>> cellObjects = new();

        static MeshGeometry3D cubeMesh = CreateCubeMesh();

        public void Initialize(Viewport3D viewport)
        {
            sceneVisual.Content = sceneRoot;
            viewport.Children.Add(sceneVisual);

            var light = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-1, -1, -1)
            };

            sceneRoot.Children.Add(light);
            sceneRoot.Children.Add(objectRoot);
            sceneRoot.Children.Add(gridRoot);
            sceneRoot.Children.Add(cursorRoot);

            cursorModel = CreateCubeModel(Colors.LightBlue);

            cursorModel.Transform = cursorTransform;

            cursorRoot.Children.Add(cursorModel);
        }

        static MeshGeometry3D CreateCubeMesh()
        {
            var mesh = new MeshGeometry3D();

            Point3D p0 = new(0, 0, 0);
            Point3D p1 = new(1, 0, 0);
            Point3D p2 = new(1, 1, 0);
            Point3D p3 = new(0, 1, 0);

            Point3D p4 = new(0, 0, 1);
            Point3D p5 = new(1, 0, 1);
            Point3D p6 = new(1, 1, 1);
            Point3D p7 = new(0, 1, 1);

            mesh.Positions = new Point3DCollection
            {
                p0,p1,p2,p3,p4,p5,p6,p7
            };

            mesh.TriangleIndices = new Int32Collection
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                0,1,5, 0,5,4,
                2,3,7, 2,7,6,
                1,2,6, 1,6,5,
                0,3,7, 0,7,4
            };

            return mesh;
        }

        public GeometryModel3D CreateCubeModel(Color color)
        {
            var material = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(cubeMesh, material);
        }

        public void BuildGrid(int size)
        {
            gridRoot.Children.Clear();
            gridRoot.Children.Add(CreateGrids(size));
        }

        GeometryModel3D CreateGrids(int size)
        {
            var mesh = new MeshGeometry3D();

            for (int i = -size; i <= size; i++)
            {
                AddLine(mesh,
                    new Point3D(i, 0, -size),
                    new Point3D(i, 0, size));

                AddLine(mesh,
                    new Point3D(-size, 0, i),
                    new Point3D(size, 0, i));
            }

            var material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));

            return new GeometryModel3D(mesh, material);
        }

        void AddLine(MeshGeometry3D mesh, Point3D p1, Point3D p2)
        {
            double width = 0.02;

            Vector3D dir = p2 - p1;

            Vector3D normal = Vector3D.CrossProduct(dir, new Vector3D(0, 1, 0));
            normal.Normalize();
            normal *= width;

            Point3D v0 = p1 + normal;
            Point3D v1 = p1 - normal;
            Point3D v2 = p2 + normal;
            Point3D v3 = p2 - normal;

            int index = mesh.Positions.Count;

            mesh.Positions.Add(v0);
            mesh.Positions.Add(v1);
            mesh.Positions.Add(v2);
            mesh.Positions.Add(v3);

            mesh.TriangleIndices.Add(index + 0);
            mesh.TriangleIndices.Add(index + 1);
            mesh.TriangleIndices.Add(index + 2);

            mesh.TriangleIndices.Add(index + 2);
            mesh.TriangleIndices.Add(index + 3);
            mesh.TriangleIndices.Add(index + 0);
        }

        public void UpdateVisibleArea(GridMap grid, IntVector3 center, TransFormWorld world)
        {
            HashSet<IntVector3> needed = new();

            for (int x = -RenderDistance; x <= RenderDistance; x++)
                for (int z = -RenderDistance; z <= RenderDistance; z++)
                {
                    var cell = new IntVector3(center.X + x, 0, center.Z + z);

                    needed.Add(cell);

                    if (!visibleCells.Contains(cell))
                    {
                        CreateCell(world, cell, grid);
                    }
                }

            foreach (var old in visibleCells)
            {
                if (!needed.Contains(old))
                {
                    RemoveCell(old);
                }
            }

            visibleCells.Clear();
            visibleCells.UnionWith(needed);
        }

        void CreateCell(TransFormWorld world, IntVector3 cell, GridMap grid)
        {
            var ids = grid.GetObjects(cell);
            if (ids == null) return;

            var list = new List<int>();

            foreach (var id in ids)
            {
                if (objectModels.ContainsKey(id))
                {
                    list.Add(id);
                    continue;
                }

                var node = world.GetObject(id);
                if (node == null) continue;

                var pos = node.GetWorldPosition();

                var cube = CreateCubeModel(Colors.Red);

                var transform = new TranslateTransform3D(pos.X, pos.Y, pos.Z);
                cube.Transform = transform;

                objectModels[id] = cube;
                objectTransforms[id] = transform;

                objectRoot.Children.Add(cube);

                list.Add(id);
            }

            cellObjects[cell] = list;
        }

        void RemoveCell(IntVector3 cell)
        {
            if (!cellObjects.TryGetValue(cell, out var ids))
                return;

            foreach (var id in ids)
            {
                if (objectModels.TryGetValue(id, out var model))
                {
                    objectRoot.Children.Remove(model);
                    objectModels.Remove(id);
                    objectTransforms.Remove(id);
                }
            }

            cellObjects.Remove(cell);
        }

        public void UpdateObjectTransform(TransFormWorld world)
        {
            foreach (var pair in objectTransforms)
            {
                int id = pair.Key;
                var transform = pair.Value;

                var node = world.GetObject(id);
                if (node == null) continue;

                var pos = node.GetWorldPosition();

                transform.OffsetX = pos.X;
                transform.OffsetY = pos.Y;
                transform.OffsetZ = pos.Z;
            }
        }

        public void UpdateCursor()
        {
            if (HoverCell is IntVector3 hover)
            {
                cursorTransform.OffsetX = hover.X;
                cursorTransform.OffsetY = hover.Y;
                cursorTransform.OffsetZ = hover.Z;
            }
        }

        public int? GetObjectIdFromModel(GeometryModel3D model)
        {
            foreach (var pair in objectModels)
            {
                if (pair.Value == model)
                {
                    return pair.Key;
                }
            }

            return null;
        }
        public void RemoveObject(int id)
        {
            if (objectModels.TryGetValue(id, out var model))
            {
                objectRoot.Children.Remove(model);
                objectModels.Remove(id);
            }

            objectTransforms.Remove(id);

            // cellObjects から削除（安全版）
            List<IntVector3> emptyCells = new();

            foreach (var kv in cellObjects)
            {
                if (kv.Value == null)
                    continue;

                kv.Value.Remove(id);

                if (kv.Value.Count == 0)
                    emptyCells.Add(kv.Key);
            }

            foreach (var cell in emptyCells)
            {
                cellObjects.Remove(cell);
            }
        }

        public void HighlightObject(int id)
        {
            if (!objectModels.TryGetValue(id, out var model))
                return;

            // 元のマテリアルを保存
            if (!originalMaterials.ContainsKey(id))
                originalMaterials[id] = ((GeometryModel3D)model).Material;

            // 色変更
            var highlightMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
            ((GeometryModel3D)model).Material = highlightMaterial;

            // 元のスケールを保存
            if (!originalScale.ContainsKey(id))
                originalScale[id] = model.Transform.Value.M11;

            // スケールアップ（1.2倍）
            var scale = new ScaleTransform3D(1.2, 1.2, 1.2);
            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(scale);
            transformGroup.Children.Add(model.Transform);

            model.Transform = transformGroup;
        }

        public void UnhighlightObject(int id)
        {
            if (!objectModels.TryGetValue(id, out var model))
                return;

            // 元のマテリアルに戻す
            if (originalMaterials.TryGetValue(id, out var mat))
                ((GeometryModel3D)model).Material = mat;

            // 元のスケールに戻す
            if (originalScale.TryGetValue(id, out var scale))
            {
                var scaleTransform = new ScaleTransform3D(scale, scale, scale);
                var transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(scaleTransform);
                model.Transform = transformGroup;
            }
        }
    }
}
