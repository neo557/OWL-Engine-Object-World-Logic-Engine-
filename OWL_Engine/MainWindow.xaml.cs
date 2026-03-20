using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Camera;
using OWL_Engine.Managers;
using OWL_Engine.Objects;
using OWL_Engine.Raycaster;
using OWL_Engine.Render;
using OWL_Engine.VisualTree;
using OWL_Engine.Worlds;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace OWL_Engine
{
    public partial class MainWindow : Window
    {
        private WorldController? controller;
        private _3DRenderer? renderer;
        private CameraMove? cameraMove;
        private TransFormWorld? world;
        private SelectionManager? selection;
        private IntVector3 center = new IntVector3(0, 0, 0);
        private IntVector3 lastcenter = new IntVector3(0, 0, 0);
        private GridMap? grid;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            renderer = new _3DRenderer();
            renderer.Initialize(View3D);   // ← ここで sceneVisual が追加される

            grid = new GridMap();

            cameraMove = new CameraMove(View3D);
            cameraMove.Attach();
            selection = new SelectionManager();
            controller = new WorldController(selection, renderer, grid);

            renderer.InitializeGrid(View3D);
            controller.Initialize();
            world = controller.GetWorld();

            world.TryCreateObject(1, new IntVector3(0, 0, 0), out _);

            renderer.UpdateVisibleArea(world.GridMap, center, world);

            HierarchyList.ItemsSource = world.GetAllObjects().ToList();

            CompositionTarget.Rendering += OnRendering;
            View3D.MouseLeftButtonDown += View3D_MouseDown;
        }

        void OnRendering(object? sender, EventArgs e)
        {
            UpdateLoop(sender, e);
        }

        public void Window_KeyDown(object sender, KeyEventArgs e)
        {
            IntVector3 dir = new IntVector3(0, 0, 0);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            // Shiftなし → XZ平面移動
            if (!shift)
            {
                if (e.Key == Key.Up) dir = new IntVector3(0, 0, 1);
                if (e.Key == Key.Down) dir = new IntVector3(0, 0, -1);
                if (e.Key == Key.Left) dir = new IntVector3(-1, 0, 0);
                if (e.Key == Key.Right) dir = new IntVector3(1, 0, 0);
            }
            else
            {
                // Shiftあり → Y軸移動
                if (e.Key == Key.Up) dir = new IntVector3(0, 1, 0);
                if (e.Key == Key.Down) dir = new IntVector3(0, -1, 0);
            }

            if (e.Key == Key.Delete && selection?.HasSelection == true && world != null && renderer != null)
            {
                if (selection.SelectedId is int id)
                {
                    if (world.GetObject(id) != null)
                        world.RemoveObject(id);

                    renderer.RemoveObject(id);
                    selection.Clear();
                }
            }

            // + キーで細かくする
            if (e.Key == Key.OemPlus && renderer != null)
            {
                renderer.SetGridSize(renderer.GridSize / 2);
            }

            // - キーで粗くする
            if (e.Key == Key.OemMinus && renderer != null)
            {
                renderer.SetGridSize(renderer.GridSize * 2);
            }

            if (selection?.SelectedId is int selectedId)
            {
                if (renderer == null) return;
                if (e.Key == Key.Q)
                    renderer.RotateY(selectedId, -15);

                if (e.Key == Key.E)
                    renderer.RotateY(selectedId, 15);
            }


            if (selection?.HasSelection == true && world != null && renderer != null)
            {
                if (selection.SelectedId is int id)
                {

                    var before = world.GetObject(id)?.GetWorldPosition();

                    controller?.Move(id, dir);

                    var after = world.GetObject(id)?.GetWorldPosition();

                }
            }

        }

        void View3D_MouseDown(object sender, MouseEventArgs e)
        {
            if (renderer == null || selection == null)
                return;

            Point pos = e.GetPosition(View3D);

            //  3Dモデルに対してレイキャスト
            var model = ObjectRaycaster.Raycast(pos, View3D);

            // 1. 何もヒットしなかった → 選択解除
            if (model == null)
            {
                if (selection.SelectedId is int prevId)
                    renderer.UnhighlightObject(prevId);

                selection.Clear();
                renderer.SelectedObjectID = null;
                HierarchyList.SelectedItem = null;
                return;
            }

            // 2. ヒットした → オブジェクトID取得
            int? id = renderer.GetObjectIdFromModel(model);
            if (id == null) return;

            // 3. 以前の選択を解除
            if (selection.SelectedId is int prev)
                renderer.UnhighlightObject(prev);

            // 4. 新しい選択
            selection.Select(id.Value);
            renderer.HighlightObject(id.Value);

            // 5. 左のリストも同期
            HierarchyList.SelectedItem = world?.GetObject(id.Value);
        }

        void UpdateLoop(object? sender, EventArgs e)
        {
            Point pos = Mouse.GetPosition(View3D); 
            if (renderer == null || controller == null)
                return;

            var cell = MouseGridRaycaster.GetGridPosition(
                pos,
                View3D,
                (PerspectiveCamera)View3D.Camera,
                renderer.CurrentLayerY);

            renderer.HoverCell = cell;
            renderer.UpdateCursor();
            renderer.UpdateInfiniteGrid((PerspectiveCamera)View3D.Camera);
            var world = controller.GetWorld();

            if ( world == null)
                return;
            if (center != lastcenter)
            {
                renderer.UpdateVisibleArea(world.GridMap, center, world);
                
                lastcenter = center;
                
            }

            renderer.UpdateObjectTransform(controller);
        }

        private void HierarchyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (renderer == null || selection == null)
                return;

            // 以前の選択解除
            if (selection.SelectedId is int prevId)
                renderer.UnhighlightObject(prevId);

            if (HierarchyList.SelectedItem is WorldObject obj)
            {
                selection.Select(obj.Id);
                renderer.HighlightObject(obj.Id);
            }
            else
            {
                selection.Clear();
            }
        }

        private void CreateCube_Click(object sender, RoutedEventArgs e)
        {
            if (controller == null || world == null || selection == null || renderer == null)
                return;

            controller.TryCreateObject(PrimitiveType.Cube, center, out int id);

            // 以前の選択を解除
            if (selection.SelectedId is int prevId)
                renderer.UnhighlightObject(prevId);

            // 新しい選択
            selection.Select(id);
            renderer.HighlightObject(id);

            HierarchyList.ItemsSource = world.GetAllObjects().ToList();
        }

        private void CreateRectangle_Click(object sender, RoutedEventArgs e)
        {
            if (controller == null || world == null || selection == null || renderer == null)
                return;
            controller.TryCreateObject(PrimitiveType.Rectangle, center, out int id);
            // 以前の選択を解除
            if (selection.SelectedId is int prevId)
                renderer.UnhighlightObject(prevId);

            // 新しい選択
            selection.Select(id);
            renderer.HighlightObject(id);

            HierarchyList.ItemsSource = world.GetAllObjects().ToList();
        }

        private void CreateTriangle_Click(object sender, RoutedEventArgs e)
        {
            if (controller == null || world == null || selection == null || renderer == null)
                return;
            controller.TryCreateObject(PrimitiveType.Triangle, center, out int id);
            // 以前の選択を解除
            if (selection.SelectedId is int prevId)
                renderer.UnhighlightObject(prevId);

            // 新しい選択
            selection.Select(id);
            renderer.HighlightObject(id);

            HierarchyList.ItemsSource = world.GetAllObjects().ToList();
        }
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            // Viewport3D の上にいるときだけメニューを出す
            var pos = e.GetPosition(View3D);
            if (pos.X < 0 || pos.Y < 0 || pos.X > View3D.ActualWidth || pos.Y > View3D.ActualHeight)
                return;

        }
        private void GridSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridSize.SelectedItem is ComboBoxItem item)
            {
                if (renderer == null) return;
                if (double.TryParse(item.Content.ToString(), out double size))
                {
                    renderer.SetGridSize(size);
                }
            }

            this.Focus();
        }
    }
}