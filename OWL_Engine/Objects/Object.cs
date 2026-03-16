using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace OWL_Engine
{
    public class Object
    {
        public int X;
        public int Y;

        public Object(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(DrawingContext dc, int cellSize)
        {
            Rect rect = new Rect(X * cellSize + 4, Y * cellSize + 4, cellSize - 8, cellSize - 8);

            dc.DrawRectangle(Brushes.Blue,new Pen(Brushes.Black, 1), rect);
        }
    }
}
