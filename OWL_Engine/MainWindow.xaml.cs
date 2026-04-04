using DOESUE.Core;
using DOESUE.Math;
using Microsoft.Win32;
using OWL_Engine.Asset;
using OWL_Engine.Camera;
using OWL_Engine.Command;
using OWL_Engine.Managers;
using OWL_Engine.Objects;
using OWL_Engine.Raycaster;
using OWL_Engine.Render;
using OWL_Engine.Worlds;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace OWL_Engine
{
    public interface ICommand
    {
        void Undo();
        void Redo();
    }
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
        Stack<ICommand> undoStack = new();
        Stack<ICommand> redoStack = new();
        private bool _isInternalSelectionChange = false;
        private Point _dragStartPoint;
        private WorldObject? SelectedObject;
        private bool _isUpdatingInspector = false;

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
            if (controller == null || renderer == null || selection == null)
                return;

            // ============================
            // Ctrl 系ショートカット
            // ============================

            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            // 保存
            if (ctrl && e.Key == Key.S)
            {
                Save_Click(sender, e);
                return;
            }

            // 読み込み
            if (ctrl && e.Key == Key.O)
            {
                Export_Click(sender, e);
                return;
            }

            // Undo
            if (ctrl && e.Key == Key.Z)
            {
                Undo();
                return;
            }

            // Redo
            if (ctrl && e.Key == Key.Y)
            {
                Redo();
                return;
            }

            // 全選択
            if (ctrl && e.Key == Key.A)
            {
                foreach (var obj in controller.GetAllWorldObjects())
                {
                    selection.Select(obj.Id);
                    renderer.HighlightObject(obj.Id);
                }
                return;
            }

            // 複製（Ctrl + D）
            if (ctrl && e.Key == Key.D)
            {
                if (selection.SelectedId is int selId)
                {
                    var original = controller.GetObject(selId);
                    if (original == null) return;

                    // clone をここで宣言（スコープ外でも使える）
                    WorldObject clone;

                    if (original.Type == "OBJ")
                    {
                        if (string.IsNullOrEmpty(original.ObjPath))
                        {
                            MessageBox.Show("OBJ のパスが存在しないため複製できません。");
                            return;
                        }

                        clone = new ImportedObject();
                        clone.ObjPath = original.ObjPath;

                        var mesh = WorldController.CreateMeshFromOBJFull(original.ObjPath, out Color diffuse);
                        clone.Color = diffuse;
                        clone.SetMesh(mesh);
                    }
                    else
                    {
                        clone = ObjectFactory.Create(Enum.Parse<PrimitiveType>(original.Type));
                    }

                    clone.Id = controller.GetNextId();
                    clone.Position = new Point3D(original.Position.X + 1, original.Position.Y, original.Position.Z);
                    clone.Scale = original.Scale;
                    clone.Rotation = original.Rotation;
                    clone.Color = original.Color;
                    clone.Type = original.Type;

                    controller.AddObject(clone);

                    undoStack.Push(new CreateCommand(controller, clone));
                    redoStack.Clear();

                    RefreshHierarchy();
                    selection.Select(clone.Id);
                    renderer.HighlightObject(clone.Id);
                }
                return;
            }

            // ============================
            // Delete（Ctrl なし）
            // ============================

            if (e.Key == Key.Delete && selection.HasSelection)
            {
                if (selection.SelectedId is int delId)
                {
                    var obj = controller.GetObject(delId);
                    if (obj != null)
                    {
                        undoStack.Push(new DeleteCommand(controller, obj));
                        redoStack.Clear();

                        controller.RemoveObject(delId);
                        renderer.RemoveObject(delId);
                        selection.Clear();
                    }
                }
                return;
            }

            // ============================
            // 移動処理（矢印キー / WASD）
            // ============================

            IntVector3 dir = new IntVector3(0, 0, 0);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (!shift)
            {
                if (e.Key == Key.Up || e.Key == Key.W) dir = new IntVector3(0, 0, 1);
                if (e.Key == Key.Down || e.Key == Key.S) dir = new IntVector3(0, 0, -1);
                if (e.Key == Key.Left || e.Key == Key.A) dir = new IntVector3(-1, 0, 0);
                if (e.Key == Key.Right || e.Key == Key.D) dir = new IntVector3(1, 0, 0);
            }
            else
            {
                if (e.Key == Key.Up|| e.Key == Key.W) dir = new IntVector3(0, 1, 0);
                if (e.Key == Key.Down || e.Key == Key.S) dir = new IntVector3(0, -1, 0);
            }

            if (dir.X != 0 || dir.Y != 0 || dir.Z != 0)
            {
                if (selection.SelectedId is int moveId)
                {
                    var obj = controller.GetObject(moveId);
                    if (obj == null) return;

                    var before = obj.Position;

                    controller.Move(moveId, dir);

                    var after = obj.Position;

                    undoStack.Push(new MoveCommand(controller, moveId, before, after));
                    redoStack.Clear();
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

            // 3. 新しい選択にハイライトを付ける 
            if (id is int highlightId)
                renderer?.HighlightObject(highlightId);

            // 4. Hierarchy の選択を同期
            ClearTreeSelection();
            if (id is int selectId)
                SelectInHierarchy(selectId);

            // 5. Inspector の更新
            if (id is int objId && controller != null)
            {
                var obj = controller.GetObject(objId);
                if (obj != null)
                    SetSelectedObject(obj);
            }

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

        private void Undo()
        {
            if (undoStack.Count == 0 || renderer == null || controller == null) return;

            var cmd = undoStack.Pop();
            cmd.Undo();
            redoStack.Push(cmd);

            RefreshHierarchy();
            renderer.UpdateObjectTransform(controller);
        }

        private void Redo()
        {
            if (redoStack.Count == 0 || renderer == null || controller == null) return;

            var cmd = redoStack.Pop();
            cmd.Redo();
            undoStack.Push(cmd);

            RefreshHierarchy();
            renderer.UpdateObjectTransform(controller);
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

                SetSelectedObject(obj);// Inspector 更新
            }

        }
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "OBJ Files (*.obj)|*.obj|All Files (*.*)|*.*";

            if (dialog.ShowDialog() == true && controller != null)
            {
                controller.LoadOBJModel(dialog.FileName);
                RefreshHierarchy();
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "JSON Files (*.json)|*.json";

            if (dialog.ShowDialog() == true && controller != null)
            {
                var data = new SaveData();

                foreach (var obj in controller.GetAllWorldObjects())
                {
                    data.Objects.Add(new SaveObject
                    {
                        Id = obj.Id,
                        //Name = obj.Name,
                        Type = obj.Type,      // Cube / Rectangle / OBJ
                        ObjPath = obj.ObjPath, // OBJ の場合だけ
                        X = obj.Position.X,
                        Y = obj.Position.Y,
                        Z = obj.Position.Z,
                        ParentId = obj.ParentId
                    });
                }

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dialog.FileName, json);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "JSON Files (*.json)|*.json|OBJ Files (*.obj)|*.obj|All Files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                var json = File.ReadAllText(dialog.FileName);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                if (data != null)
                {
                    controller?.LoadFromSaveData(data);
                    RefreshHierarchy();
                }
            }
        }
        private void SetSelectedObject(WorldObject obj)
        {
            SelectedObject = obj;

            _isUpdatingInspector = true;

            if (SelectedObject != null)
            {
                InspectorName.Text = SelectedObject.Name;

                PosX.Text = SelectedObject.Position.X.ToString("0.###");
                PosY.Text = SelectedObject.Position.Y.ToString("0.###");
                PosZ.Text = SelectedObject.Position.Z.ToString("0.###");

                ScaleX.Text = SelectedObject.Scale.X.ToString("0.###");
                ScaleY.Text = SelectedObject.Scale.Y.ToString("0.###");
                ScaleZ.Text = SelectedObject.Scale.Z.ToString("0.###");

                RotX.Text = SelectedObject.Rotation.X.ToString("0.###");
                RotY.Text = SelectedObject.Rotation.Y.ToString("0.###");
                RotZ.Text = SelectedObject.Rotation.Z.ToString("0.###");
                _isUpdatingInspector = true;
            }

            _isUpdatingInspector = false;
        }
        private void Pos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingInspector || SelectedObject == null) return;

            // 変更前の状態を保存（UndoStack に積む）
            undoStack.Push((ICommand)ObjectSnapshot.FromObject(SelectedObject));
            redoStack.Clear();

            if (float.TryParse(PosX.Text, out float x) &&
                float.TryParse(PosY.Text, out float y) &&
                float.TryParse(PosZ.Text, out float z))
            {
                SelectedObject.Position = new Point3D(x, y, z);
                UpdateObjectTransform(SelectedObject);
            }
        }
        private void UpdateObjectTransform(WorldObject obj)
        {
            if(controller == null || renderer == null)
                return;
            // WorldController に反映
            controller.UpdateObject(obj);

            // Renderer に全体更新を投げる
            renderer.UpdateObjectTransform(controller);
        }

        private void InspectorName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SelectedObject != null)
            {
                SelectedObject.Name = InspectorName.Text;

                if (HierarchyTree.SelectedItem is TreeViewItem item)
                    item.Header = SelectedObject.Name;
            }
        }

    }
}