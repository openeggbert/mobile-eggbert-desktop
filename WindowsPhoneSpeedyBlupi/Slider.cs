// WindowsPhoneSpeedyBlupi, Version=1.0.0.5, Culture=neutral, PublicKeyToken=6db12cd62dbec439
// WindowsPhoneSpeedyBlupi.Slider
using System;

namespace WindowsPhoneSpeedyBlupi
{
    public class Slider
    {
        public Slider(TinyPoint topLeftCorner, double value) { 
            TopLeftCorner = topLeftCorner;
            value = Value;
            
        }
        public TinyPoint TopLeftCorner { get; set; }

        public double Value { get; set; }

        private int PosLeft
        {
            get
            {
                return TopLeftCorner.X + 22;
            }
        }

        private int PosRight
        {
            get
            {
                return TopLeftCorner.X + 248 - 22;
            }
        }

        public void Draw(Pixmap pixmap)
        {
            TinyPoint tinyPoint = default(TinyPoint);
            tinyPoint.X = TopLeftCorner.X - pixmap.Origin.X;
            tinyPoint.Y = TopLeftCorner.Y - pixmap.Origin.Y;
            TinyPoint dest = tinyPoint;
            TinyRect tinyRect = default(TinyRect);
            tinyRect.LeftX = 0;
            tinyRect.RightX = 124;
            tinyRect.TopY = 0;
            tinyRect.BottomY = 22;
            TinyRect rect = tinyRect;
            pixmap.DrawPart(5, dest, rect, 2.0);
            int num = (int)((double)(PosRight - PosLeft) * Value);
            int num2 = TopLeftCorner.Y + 22;
            int num3 = 94;
            TinyRect tinyRect2 = default(TinyRect);
            tinyRect2.LeftX = PosLeft + num - num3 / 2;
            tinyRect2.RightX = PosLeft + num + num3 / 2;
            tinyRect2.TopY = num2 - num3 / 2;
            tinyRect2.BottomY = num2 + num3 / 2;
            rect = tinyRect2;
            pixmap.DrawIcon(14, 1, rect, 1.0, false);
            TinyRect tinyRect3 = default(TinyRect);
            tinyRect3.LeftX = TopLeftCorner.X - 65;
            tinyRect3.RightX = TopLeftCorner.X - 65 + 60;
            tinyRect3.TopY = TopLeftCorner.Y - 10;
            tinyRect3.BottomY = TopLeftCorner.Y - 10 + 60;
            rect = tinyRect3;
            pixmap.DrawIcon(10, 37, rect, 1.0, false);
            TinyRect tinyRect4 = default(TinyRect);
            tinyRect4.LeftX = TopLeftCorner.X + 248 + 5;
            tinyRect4.RightX = TopLeftCorner.X + 248 + 5 + 60;
            tinyRect4.TopY = TopLeftCorner.Y - 10;
            tinyRect4.BottomY = TopLeftCorner.Y - 10 + 60;
            rect = tinyRect4;
            pixmap.DrawIcon(10, 38, rect, 1.0, false);
        }

        public bool Move(TinyPoint pos)
        {
            TinyRect tinyRect = default(TinyRect);
            tinyRect.LeftX = TopLeftCorner.X - 50;
            tinyRect.RightX = TopLeftCorner.X + 248 + 50;
            tinyRect.TopY = TopLeftCorner.Y - 50;
            tinyRect.BottomY = TopLeftCorner.Y + 44 + 50;
            TinyRect rect = tinyRect;
            if (Misc.IsInside(rect, pos))
            {
                double val = ((double)pos.X - (double)PosLeft) / (double)(PosRight - PosLeft);
                val = Math.Max(val, 0.0);
                val = Math.Min(val, 1.0);
                if (Value != val)
                {
                    Value = val;
                    return true;
                }
            }
            return false;
        }
    }

}