using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using VisualTreeHelper = System.Windows.Media.VisualTreeHelper;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using DOESUE.Math;
using OWL_Engine.Worlds;
using OWL_Engine.Camera;
using Viewport3D = System.Windows.Controls.Viewport3D;

namespace OWL_Engine.VisualTree
{
    public class Hittests
    {
        Viewport3D viewport;

        public Hittests(Viewport3D viewport)
        {
            this.viewport = viewport;
            this.viewport.MouseDown += ViewPort_MouseDown;
        }
        void ViewPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(viewport);

            PointHitTestParameters hitParam = new PointHitTestParameters(pos);

            VisualTreeHelper.HitTest(viewport, null, HitTestResultCallback, hitParam);
        }

        HitTestResultBehavior HitTestResultCallback(HitTestResult result)
        {
            RayHitTestResult? rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                Point3D hitPoint = rayResult.PointHit;

                int gridX = (int)Math.Floor(hitPoint.X);
                int gridY = (int)Math.Floor(hitPoint.Y);
                int gridZ = (int)Math.Floor(hitPoint.Z);

                var gridPos = new IntVector3(gridX, 0, gridZ);

                //オブジェクトを配置する処理
            }

            return HitTestResultBehavior.Stop;
        }

        
    }
}
