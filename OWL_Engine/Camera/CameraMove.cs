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
        double pitch = -30;
        double distance = 30;

        Point lastMouse;
        Point3D target = new Point3D(0, 0, 0);

        bool rotating = false;

        bool panning = false;

        //bool zooming = false;


        public CameraMove(Viewport3D view)
        {
            viewport = view;
            
            camera = (PerspectiveCamera)view.Camera;

            view.Loaded += OnLoaded;

            UpdateCamera();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
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
                rotating = true;
                lastMouse = e.GetPosition(viewport);

                Mouse.Capture(viewport);
            }

            if(e.MiddleButton == MouseButtonState.Pressed)
            {
                panning = true;
                lastMouse = e.GetPosition(viewport);
                Mouse.Capture(viewport);
            }
        }

        void View_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rotating = false;
            panning = false;

            Mouse.Capture(null);
        }
        private void View_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(viewport);

            double dx = pos.X - lastMouse.X;
            double dy = pos.Y - lastMouse.Y;

            if(rotating)
            {
                yaw += dx * 0.4;
                pitch -= dy * 0.4;
                if (pitch > 89) pitch = 89;
                if (pitch < -89) pitch = -89;
            }

            if(panning)
            {
                double panSpeed = distance * 0.01;

                target.X -= dx * panSpeed;
                target.Y += dy * panSpeed;
            }

            lastMouse = pos;

            UpdateCamera();
        }

        private void View_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            distance -= e.Delta * 0.05;

            if (distance < 1)
                distance = 1;

            if (distance > 500)
                distance = 500;

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
