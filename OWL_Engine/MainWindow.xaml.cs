using DOESUE.Core;
using DOESUE.Math;
using OWL_Engine.Camera;
using OWL_Engine.Managers;
using OWL_Engine.Objects;
using OWL_Engine.Raycaster;
using OWL_Engine.Render;
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
        private bool _isInternalSelectionChange = false;
        private Point _dragStartPoint;
        public SceneManager? SceneManager { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            this.WindowState = WindowState.Maximized;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            renderer = new _3DRenderer();
            renderer.Initialize(View3D);   // ← ここで sceneVisual が追加される
            SceneManager = new SceneManager(renderer);
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
            RefreshHierarchy();
            
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
                if (e.Key == Key.Up || e.Key == Key.W) dir = new IntVector3(0, 0, 1);
                if (e.Key == Key.Down|| e.Key == Key.S) dir = new IntVector3(0, 0, -1);
                if (e.Key == Key.Left || e.Key == Key.A) dir = new IntVector3(-1, 0, 0);
                if (e.Key == Key.Right || e.Key == Key.D) dir = new IntVector3(1, 0, 0);
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

            // 矢印キー以外なら、移動処理には進まない
            bool isArrow =
                e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.Left || e.Key == Key.Right|| e.Key == Key.W || e.Key == Key.S|| e.Key == Key.A || e.Key == Key.D;

            // Delete / + / - / Q / E はこのあと個別処理するのでここでは return しない
            if (!isArrow && e.Key != Key.Delete &&
                e.Key != Key.OemPlus && e.Key != Key.OemMinus &&
                e.Key != Key.Q && e.Key != Key.E)
            {
                return;
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
            Keyboard.ClearFocus(); // TreeView にフォーカスを残さない

            this.Focus();

            if (renderer == null || selection == null)
                return;

            Point pos = e.GetPosition(View3D);
            var model = ObjectRaycaster.Raycast(pos, View3D);

            if (model == null)
            {
                // 空白クリック → 完全選択解除
                SelectObject(null);
                return;
            }

            int? id = renderer.GetObjectIdFromModel(model);
            if (id == null) return;

            // ここだけでOK
            SelectObject(id.Value);

        }
        private void SelectObject(int? id)
        {
            _isInternalSelectionChange = true;

            if (selection == null) return;

            // 1. 以前の選択を解除（ハイライト）
            if (selection.SelectedId is int prevId)
                renderer?.UnhighlightObject(prevId);

            // 2. SelectionManager の更新
            if (id is int newId)
                selection.Select(newId);
            else
                selection.Clear();

            // 3. 新しい選択にハイライトを付ける ← これが抜けていた
            if (id is int highlightId)
                renderer?.HighlightObject(highlightId);

            // 4. Hierarchy の選択を同期
            ClearTreeSelection();
            if (id is int selectId)
                SelectInHierarchy(selectId);
            
            _isInternalSelectionChange = false;

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
            renderer.UpdateInfiniteGrid((PerspectiveCamera)View3D.Camera);

            var world = controller.GetWorld();
            if (world == null)
                return;

            //  カメラの target を center に反映
            if (cameraMove == null) return;
            var camPos = cameraMove.Target;
            center = new IntVector3(
                (int)Math.Round(camPos.X),
                (int)Math.Round(camPos.Y),
                (int)Math.Round(camPos.Z)
            );

            if (center != lastcenter)
            {
                renderer.UpdateVisibleArea(world.GridMap, center, world);
                lastcenter = center;
            }

            renderer.UpdateObjectTransform(controller);
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

            RefreshHierarchy();
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

            RefreshHierarchy();
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
            RefreshHierarchy();
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
        private void SelectInHierarchy(int id)
        {
            foreach (var item in HierarchyTree.Items)
            {

                if (FindAndSelect(item as TreeViewItem, id))
                    break;
            }
        }

        private bool FindAndSelect(TreeViewItem? item, int id)
        {
            if (item == null) return false;

            if (item.Tag is WorldObject obj && obj.Id == id)
            {
                item.IsSelected = true;
                item.BringIntoView();
                return true;
            }

            foreach (TreeViewItem child in item.Items)
            {
                if (FindAndSelect(child, id))
                    return true;
            }

            return false;
        }
        private void RefreshHierarchy()
        {
            if (controller == null) return;

            HierarchyTree.Items.Clear();

            var objects = controller.GetAllWorldObjects().ToList();

            var roots = objects.Where(o => o.ParentId == -1);

            foreach (var root in roots)
            {
                HierarchyTree.Items.Add(CreateTreeItem(root, objects));
            }
        }
        private void ClearTreeSelection()
        {
            ClearSelectionRecursive(HierarchyTree);
        }

        private void ClearSelectionRecursive(ItemsControl parent)
        {
            foreach (var obj in parent.Items)
            {
                if (obj is TreeViewItem item)
                {
                    item.IsSelected = false;
                    ClearSelectionRecursive(item);
                }
            }
        }
        private TreeViewItem CreateTreeItem(WorldObject obj, List<WorldObject> all)
        {
            var item = new TreeViewItem
            {
                Header = obj.Name,
                Tag = obj
            };
            item.PreviewMouseLeftButtonDown += TreeItem_MouseDown;
            item.Drop += TreeItem_Drop;
            item.DragOver += TreeItem_DragOver;
            item.MouseMove += TreeItem_MouseMove;
            // 子を探す
            var children = all.Where(o => o.ParentId == obj.Id);

            foreach (var child in children)
            {
                item.Items.Add(CreateTreeItem(child, all));
            }

            return item;
        }
        private void TreeItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }
        private void TreeItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStartPoint.X) < 5 &&
                Math.Abs(pos.Y - _dragStartPoint.Y) < 5)
                return;

            if (sender is TreeViewItem item)
            {
                DragDrop.DoDragDrop(item, item, DragDropEffects.Move);
            }
        }

        private void TreeItem_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TreeViewItem)))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
        private void TreeItem_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TreeViewItem)))
                return;

            var draggedItem = (TreeViewItem)e.Data.GetData(typeof(TreeViewItem));
            var targetItem = (TreeViewItem)sender;

            if (draggedItem == targetItem)
                return;

            if (draggedItem.Tag is WorldObject draggedObj &&
                targetItem.Tag is WorldObject targetObj)
            {
                // 親を変更
                draggedObj.ParentId = targetObj.Id;

                // WorldController にも通知
                controller?.SetParent(draggedObj.Id, targetObj.Id);

                // Hierarchy を再構築
                RefreshHierarchy();

                // 選択を維持
                SelectInHierarchy(draggedObj.Id);
            }
        }

        private void HierarchyTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_isInternalSelectionChange) return;

            // TreeViewItem が選択された直後にフォーカスを Window に戻す
            Keyboard.ClearFocus();
            this.Focus();
            if(selection == null) return;
            if (HierarchyTree.SelectedItem is TreeViewItem item &&
                item.Tag is WorldObject obj)
            {
                selection.Select(obj.Id);
            }

        }
    }
}