using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Objects;
using OWL_Engine.Worlds;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace OWL_Engine.Render
{
    public class _3DRenderer
    {
        Model3DGroup sceneRoot = new();
        Model3DGroup objectRoot = new();
        Model3DGroup gridRoot = new();
        Model3DGroup cursorRoot = new();

        ModelVisual3D sceneVisual = new();
        CubeObject cubeObject = new CubeObject();
        HashSet<IntVector3> visibleCells = new();

        public int RenderDistance = 20;
        public int CurrentLayerY = 0;
        public IntVector3? HoverCell = null;
        private double gridSize = 1.0; // グリッド間隔（後で可変にする）
        public double GridSize => gridSize;
        // グリッド描画用
        private Model3DGroup infiniteGrid = new Model3DGroup();
        private Model3DGroup gridLines = new Model3DGroup();
        public int? SelectedObjectID = null;

        GeometryModel3D? cursorModel;
        TranslateTransform3D cursorTransform = new();

        Dictionary<int, GeometryModel3D> objectModels = new();
        Dictionary<int, TranslateTransform3D> objectTransforms = new();
        private Dictionary<int, RotateTransform3D> objectRotations = new();

        private Dictionary<int, Material> originalMaterials = new();
        private Dictionary<int, double> originalScale = new();

        private Model3DGroup? _worldModels; 
        private GeometryModel3D? fineGridPlane;
        private DiffuseMaterial? fineGridMaterial;

        Dictionary<IntVector3, List<int>> cellObjects = new();

        static MeshGeometry3D cubeMesh = CreateCubeMesh();
        private Transform3D? rotateTransform;
        private Transform3D? scaleTransform;

        public void Initialize(Viewport3D viewport)
        {
            // ここで自前のシーンツリーを全部作る

            // ワールド用の Model3DGroup を作る
            _worldModels = new Model3DGroup();

            // ルートの Model3DGroup（ライトやグリッドも含める）
            sceneRoot = new Model3DGroup();
            objectRoot = _worldModels;      // オブジェクトはここにぶら下げる
            gridRoot = new Model3DGroup();
            cursorRoot = new Model3DGroup();

            // ライト
            var light = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-1, -1, -1)
            };
            sceneRoot.Children.Add(light);
            sceneRoot.Children.Add(objectRoot);
            sceneRoot.Children.Add(gridRoot);
            sceneRoot.Children.Add(cursorRoot);

            // ルートをぶら下げる ModelVisual3D を作る
            sceneVisual = new ModelVisual3D
            {
                Content = sceneRoot
            };

            // Viewport3D に追加
            viewport.Children.Add(sceneVisual);

            // カーソルモデル
            cursorModel = CreateCubeModel(Colors.LightBlue);
            cursorModel.Transform = cursorTransform;
            cursorRoot.Children.Add(cursorModel);
        }
        public void InitializeGrid(Viewport3D viewport)
        {
            var brush = CreateFineGridBrush();

            fineGridMaterial = new DiffuseMaterial(brush);
            fineGridPlane = CreateGridPlane(fineGridMaterial);

            // 細線Plane
            gridRoot.Children.Add(fineGridPlane);

            // 太線グリッド（これを忘れると何も見えない）
            gridRoot.Children.Add(gridLines);
        }
        private GeometryModel3D CreateGridPlane(Material mat)
        {
            double size = 200; // 200m × 200m の地面

            var mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(-size, -0.001, -size));
            mesh.Positions.Add(new Point3D(size, -0.001, -size));
            mesh.Positions.Add(new Point3D(size, -0.001, size));
            mesh.Positions.Add(new Point3D(-size, -0.001, size));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);

            // UV は 1m = 1.0 として貼る
            mesh.TextureCoordinates.Add(new Point(-size, -size));
            mesh.TextureCoordinates.Add(new Point(size, -size));
            mesh.TextureCoordinates.Add(new Point(size, size));
            mesh.TextureCoordinates.Add(new Point(-size, size));

            return new GeometryModel3D(mesh, mat);
        }
        public void UpdateInfiniteGrid(PerspectiveCamera camera)
        {
            gridLines.Children.Clear();

            double camX = camera.Position.X;
            double camZ = camera.Position.Z;

            double centerX = Math.Round(camX);
            double centerZ = Math.Round(camZ);

            var thickBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            var thickMat = new DiffuseMaterial(thickBrush);

            double worldSpacing = 1.0;
            int range = 50;

            for (int i = -range; i <= range; i++)
            {
                double x = centerX + i * worldSpacing;
                double z = centerZ + i * worldSpacing;

                gridLines.Children.Add(CreateLine(
                    new Point3D(x, 0, centerZ - range),
                    new Point3D(x, 0, centerZ + range),
                    thickMat
                ));

                gridLines.Children.Add(CreateLine(
                    new Point3D(centerX - range, 0, z),
                    new Point3D(centerX + range, 0, z),
                    thickMat
                ));
            }

            // 細線は Plane の UV スケールで調整
            UpdateFineGridTexture();
        }
        private Brush CreateFineGridBrush()
        {
            int size = 256;

            DrawingGroup dg = new DrawingGroup();

            // 背景：薄いグレー（透明は使わない）
            var bg = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)); // 15% 黒
            dg.Children.Add(new GeometryDrawing(
                bg,
                null,
                new RectangleGeometry(new Rect(0, 0, size, size))
            ));

            // 細線の色（濃いグレー）
            Brush lineBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));

            // 左端の縦線
            dg.Children.Add(new GeometryDrawing(
                lineBrush,
                new Pen(lineBrush, 2),
                new LineGeometry(new Point(0, 0), new Point(0, size))
            ));

            // 上端の横線
            dg.Children.Add(new GeometryDrawing(
                lineBrush,
                new Pen(lineBrush, 2),
                new LineGeometry(new Point(0, 0), new Point(size, 0))
            ));

            DrawingBrush brush = new DrawingBrush(dg)
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute, // ここを Absolute に
                Viewport = new Rect(0, 0, gridSize, gridSize) // 初期値
            };

            return brush;
        }
        private void UpdateFineGridTexture()
        {
            if (fineGridMaterial?.Brush is DrawingBrush brush)
            {
                // 1 タイル = gridSize [m]
                brush.Viewport = new Rect(0, 0, gridSize, gridSize);
            }
        }
        private GeometryModel3D CreateLine(Point3D p1, Point3D p2, Material mat)
        {
            double width = 0.02;

            Vector3D dir = p2 - p1;

            // 上方向との外積で線の太さ方向を作る
            Vector3D normal = Vector3D.CrossProduct(dir, new Vector3D(0, 1, 0));
            normal.Normalize();
            normal *= width;

            Point3D v0 = p1 + normal;
            Point3D v1 = p1 - normal;
            Point3D v2 = p2 + normal;
            Point3D v3 = p2 - normal;

            var mesh = new MeshGeometry3D();
            int index = 0;

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

            return new GeometryModel3D(mesh, mat);
        }

        static MeshGeometry3D CreateCubeMesh() //メッシュ製作
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

        public GeometryModel3D CreateCubeModel(Color color) //オブジェクト色変更
        {
            var material = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(cubeMesh, material);
        }

        public void UpdateVisibleArea(GridMap grid, IntVector3 center, TransFormWorld world) //グリッド視点情報
        {
            HashSet<IntVector3> needed = new();

            for (int x = -RenderDistance; x <= RenderDistance; x++)
                for (int z = -RenderDistance; z <= RenderDistance; z++)
                {
                    var cell = new IntVector3(center.X + x, 0, center.Z + z);

                    needed.Add(cell);

                    if (!visibleCells.Contains(cell))
                    {
                        cubeObject.CreateCubeMesh();
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


        void RemoveCell(IntVector3 cell) //Cell削除
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

        public void UpdateObjectTransform(WorldController controller) //オブジェクト移動
        {
            foreach (var kv in objectModels)
            {
                int id = kv.Key;

                var obj = controller.GetObject(id); //  WorldObject（double）
                if (obj == null) continue;

                var pos = obj.Position; //  double の位置

                if (objectTransforms.TryGetValue(id, out var transform))
                {
                    transform.OffsetX = pos.X;
                    transform.OffsetY = pos.Y;
                    transform.OffsetZ = pos.Z;
                }
            }
        }

        public void UpdateCursor()
        {
            if (HoverCell is IntVector3 hover)
            {
                double size = GridSize; // renderer.GridSize を参照するなら引数で渡す

                cursorTransform.OffsetX = Math.Round(hover.X / size) * size;
                cursorTransform.OffsetY = Math.Round(hover.Y / size) * size;
                cursorTransform.OffsetZ = Math.Round(hover.Z / size) * size;
            }
        }

        public int? GetObjectIdFromModel(GeometryModel3D model) //オブジェクトID取得
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
        public void RemoveObject(int id)　//オブジェクト削除
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

            if (!objectTransforms.TryGetValue(id, out var translate))
                return;

            if (!objectRotations.TryGetValue(id, out var rotate))
                return;

            // 元のマテリアル保存
            if (!originalMaterials.ContainsKey(id))
                originalMaterials[id] = model.Material;

            // 元のスケール保存
            if (!originalScale.ContainsKey(id))
                originalScale[id] = 1.0;

            //  新しい TransformGroup を作る（回転と移動は維持）
            var group = new Transform3DGroup();
            group.Children.Add(new ScaleTransform3D(1.2, 1.2, 1.2)); // ハイライト用スケール
            group.Children.Add(rotate);      // ← 回転を維持
            group.Children.Add(translate);   // ← 位置を維持

            model.Transform = group;

            // 色変更
            model.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
        }

        public void UnhighlightObject(int id)
        {
            if (!objectModels.TryGetValue(id, out var model))
                return;

            if (!objectTransforms.TryGetValue(id, out var translate))
                return;

            if (!objectRotations.TryGetValue(id, out var rotate))
                return;

            // 元のマテリアルに戻す
            if (originalMaterials.TryGetValue(id, out var mat))
                model.Material = mat;

            // 元のスケールに戻す
            double scale = originalScale.ContainsKey(id) ? originalScale[id] : 1.0;

            //  回転と移動を維持したままスケールだけ戻す
            var group = new Transform3DGroup();
            group.Children.Add(new ScaleTransform3D(scale, scale, scale));
            group.Children.Add(rotate);
            group.Children.Add(translate);

            model.Transform = group;
        }

        public void AddObject(WorldObject obj)
        {
            var model = new GeometryModel3D
            {
                Geometry = obj.Mesh,
                Material = new DiffuseMaterial(new SolidColorBrush(obj.Color))
            };

            var translate = new TranslateTransform3D(obj.Position.X, obj.Position.Y, obj.Position.Z);
            var rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0));
            var scale = new ScaleTransform3D(1, 1, 1);

            var group = new Transform3DGroup();
            group.Children.Add(scale);
            group.Children.Add(rotate);
            group.Children.Add(translate);

            model.Transform = group;

            objectModels[obj.Id] = model;
            objectTransforms[obj.Id] = translate;
            objectRotations[obj.Id] = rotate;

            _worldModels?.Children.Add(model);
        }
        public void RotateY(int id, double degrees)
        {
            if (!objectRotations.TryGetValue(id, out var rot))
                return;

            if (rot.Rotation is AxisAngleRotation3D axis)
                axis.Angle += degrees;
        }

        public void SetGridSize(double size)
        {
            gridSize = size;
        }
    }
}
