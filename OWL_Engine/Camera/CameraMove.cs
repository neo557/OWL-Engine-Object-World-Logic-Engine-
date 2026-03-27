using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;


namespace OWL_Engine.Camera
{
    public class CameraMove
    {
        PerspectiveCamera camera;
        Viewport3D viewport;
        double yaw = 45;
        double pitch = 30;
        double distance = 30;

        Point lastMouse;
        Point3D target = new();
        public Point3D Target => target;

        bool rotating = false;

        bool panning = false;

        //bool zooming = false;
        bool rightDown = false;
        bool moved = false;
        Point downPos;
        double moveThreshold = 4; // 4px 以上動いたら「移動」と判定


        public CameraMove(Viewport3D view)
        {
            viewport = view;
            camera = (PerspectiveCamera)view.Camera;
            UpdateCamera();
        }
        public void FocusOn(Point3D pos)
        {
            target = pos;
            UpdateCamera();
        }

        public void Attach()
        {
            var window = Window.GetWindow(viewport);
            window.MouseMove += View_MouseMove;
            window.MouseDown += View_MouseDown;
            window.MouseWheel += View_MouseWheel;
            window.MouseUp += View_MouseUp;
        }
        void View_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                rightDown = true;
                moved = false;
                downPos = e.GetPosition(viewport);
            }

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                panning = true;
                lastMouse = e.GetPosition(viewport);
            }
        }

        void View_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                rightDown = false;

                if (!moved )
                {
                    // 右クリック短押し → メニュー表示
                    viewport.ContextMenu.IsOpen = true;
                }

                rotating = false;
            }
            if (e.ChangedButton == MouseButton.Middle)
            {
                // stop panning when middle button released
                panning = false;
            }
        }
        void View_MouseMove(object sender, MouseEventArgs e)
        {
            if (rightDown)
            {
                var pos = e.GetPosition(viewport);
                var dx = pos.X - downPos.X;
                var dy = pos.Y - downPos.Y;

                // 一定距離動いたら「カメラ操作モード」
                if (Math.Abs(dx) > moveThreshold || Math.Abs(dy) > moveThreshold)
                {
                    moved = true;

                    if (!rotating) // ← CameraMove の既存機能を使う
                    {
                        rotating = true;
                        lastMouse = pos;
                    }
                }
            }

            if (rotating)
            {
                // 既存のカメラ回転処理
                Point pos = e.GetPosition(viewport);
                double dx = pos.X - lastMouse.X;
                double dy = pos.Y - lastMouse.Y;

                yaw += dx * 0.4;
                pitch -= dy * 0.4;

                pitch = Math.Max(-89, Math.Min(89, pitch)); //  反転防止

                lastMouse = pos;
                UpdateCamera();
            }
            if (panning)
            {
                Point pos = e.GetPosition(viewport);
                double dx = pos.X - lastMouse.X;
                double dy = pos.Y - lastMouse.Y;

                Vector3D right = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);
                right.Normalize();

                Vector3D up = camera.UpDirection;
                up.Normalize();

                // distance に依存しない panSpeed
                double panSpeed = 0.05;

                target += (-right * dx * panSpeed) + (up * dy * panSpeed);

                lastMouse = pos;
                UpdateCamera();
            }
        }


        private void View_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            distance -= e.Delta * 0.05;

            if (distance < 1)
                distance = 1;

            if (distance > 20000)
                distance = 20000;

            UpdateCamera();
        }
        void UpdateCamera()
        {
            double radYaw = yaw * Math.PI / 180;
            double radPitch = pitch * Math.PI / 180;

            double x = distance * Math.Cos(radPitch) * Math.Cos(radYaw);
            double y = distance * Math.Sin(radPitch);
            double z = distance * Math.Cos(radPitch) * Math.Sin(radYaw);

            camera.Position = new Point3D(
        target.X + x,
        target.Y + y,
        target.Z + z
    );

            camera.LookDirection = target - camera.Position;
        }

    }
}
