using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Camera;
using OWL_Engine.Managers;
using OWL_Engine.Raycaster;
using OWL_Engine.Render;
using OWL_Engine.VisualTree;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace OWL_Engine
{
    public partial class MainWindow : Window
    {
        private WorldController controller;
        private _3DRenderer renderer;
        private CameraMove cameraMove;
        private Hittests hittests;
        private TransFormWorld world;
        private SelectionManager selection;
        private IntVector3 center;
        private IntVector3 lastcenter;

        public MainWindow()
        {
            InitializeComponent();

            
            renderer = new _3DRenderer();
            cameraMove = new CameraMove(View3D);
            hittests = new Hittests(View3D);
            selection = new SelectionManager();
            controller = new WorldController(selection);
            center = new IntVector3(0, 0, 0);
            renderer.Initialize(View3D);
            renderer.BuildGrid(10);
            controller.Initialize();
            world = controller.GetWorld();

            // 原点にオブジェクトを1つ作る
            world.TryCreateObject(1, new IntVector3(0, 0, 0), out _);

            // 可視範囲を更新して描画
            renderer.UpdateVisibleArea(world.GridMap, center, world);

            lastcenter = center;
            HierarchyList.ItemsSource = world.GetAllObjects().ToList();
            CompositionTarget.Rendering += OnRendering; 

        }

        void OnRendering(object? sender, EventArgs e)
        {
            UpdateLoop(sender, e);
        }

        public void Window_KeyDown(object sender, KeyEventArgs e)
        {
            IntVector3 dir = new IntVector3(0, 0, 0);

            if (e.Key == Key.Up) dir = new IntVector3(0, 0, 1);
            if (e.Key == Key.Down) dir = new IntVector3(0, 0, -1);
            if (e.Key == Key.Left) dir = new IntVector3(-1, 0, 0);
            if (e.Key == Key.Right) dir = new IntVector3(1, 0, 0);

            if (e.Key == Key.Delete && selection.HasSelection)
            {
                if (selection.SelectedId is int id)
                {
                    if (world.GetObject(id) != null)
                        world.RemoveObject(id);

                    renderer.RemoveObject(id);
                    selection.Clear();
                }
            }

            if (selection.HasSelection)
            {
                if (selection.SelectedId is int id)
                {

                    var before = world.GetObject(id)?.GetWorldPosition();

                    controller.Move(id, dir);

                    var after = world.GetObject(id)?.GetWorldPosition();

                }
            }

        }


        void View3D_MouseDown(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(View3D);

            var model = ObjectRaycaster.Raycast(pos, View3D);
            if (model == null) return;

            int? id = renderer.GetObjectIdFromModel(model);
            if (id == null) return;

            // 以前の選択を解除
            if (selection.SelectedId is int prevId)
                renderer.UnhighlightObject(prevId);

            // 新しい選択
            selection.Select(id.Value);
            renderer.HighlightObject(id.Value);
        }

        void UpdateLoop(object? sender, EventArgs e)
        {
            Point pos = Mouse.GetPosition(View3D);

            var cell = MouseGridRaycaster.GetGridPosition(
                pos,
                View3D,
                (PerspectiveCamera)View3D.Camera,
                renderer.CurrentLayerY);

            renderer.HoverCell = cell;
            renderer.UpdateCursor();

            var world = controller.GetWorld();

            if (center != lastcenter)
            {
                renderer.UpdateVisibleArea(world.GridMap, center, world);
                lastcenter = center;
            }

            renderer.UpdateObjectTransform(world);
        }

        private void HierarchyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HierarchyList.SelectedItem is int id)
            {
                // 以前の選択を解除
                if (selection.SelectedId is int prevId)
                    renderer.UnhighlightObject(prevId);

                // 新しい選択
                selection.Select(id);
                renderer.HighlightObject(id);
            }
        }

    }
}