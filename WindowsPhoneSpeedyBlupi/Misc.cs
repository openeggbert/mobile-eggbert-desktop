// WindowsPhoneSpeedyBlupi, Version=1.0.0.5, Culture=neutral, PublicKeyToken=6db12cd62dbec439
// WindowsPhoneSpeedyBlupi.Misc
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using WindowsPhoneSpeedyBlupi;
using static WindowsPhoneSpeedyBlupi.Def;


namespace WindowsPhoneSpeedyBlupi
{
    public static class Misc
    {
        public static Rectangle RotateAdjust(Rectangle rect, double angle)
        {
            TinyPoint tinyPoint = default(TinyPoint);
            tinyPoint.X = rect.Width / 2;
            tinyPoint.Y = rect.Height / 2;
            TinyPoint p = tinyPoint;
            TinyPoint tinyPoint2 = RotatePointRad(angle, p);
            int num = tinyPoint2.X - p.X;
            int num2 = tinyPoint2.Y - p.Y;
            return new Rectangle(rect.Left - num, rect.Top - num2, rect.Width, rect.Height);
        }

        public static TinyPoint RotatePointRad(double angle, TinyPoint p)
        {
            return RotatePointRad(default(TinyPoint), angle, p);
        }

        public static TinyPoint RotatePointRad(TinyPoint center, double angle, TinyPoint p)
        {
            TinyPoint tinyPoint = default(TinyPoint);
            TinyPoint result = default(TinyPoint);
            tinyPoint.X = p.X - center.X;
            tinyPoint.Y = p.Y - center.Y;
            double num = Math.Sin(angle);
            double num2 = Math.Cos(angle);
            result.X = (int)((double)tinyPoint.X * num2 - (double)tinyPoint.Y * num);
            result.Y = (int)((double)tinyPoint.X * num + (double)tinyPoint.Y * num2);
            result.X += center.X;
            result.Y += center.Y;
            return result;
        }

        public static double DegToRad(double angle)
        {
            return angle * Math.PI / 180.0;
        }

        public static int Approch(int actual, int final, int step)
        {
            if (actual < final)
            {
                actual = Math.Min(actual + step, final);
            }
            else if (actual > final)
            {
                actual = Math.Max(actual - step, final);
            }
            return actual;
        }

        public static int Speed(double speed, int max)
        {
            if (speed > 0.0)
            {
                return Math.Max((int)(speed * (double)max), 1);
            }
            if (speed < 0.0)
            {
                return Math.Min((int)(speed * (double)max), -1);
            }
            return 0;
        }

        public static TinyRect Inflate(TinyRect rect, int value)
        {
            TinyRect result = default(TinyRect);
            result.LeftX = rect.LeftX - value;
            result.RightX = rect.RightX + value;
            result.TopY = rect.TopY - value;
            result.BottomY = rect.BottomY + value;
            return result;
        }

        public static bool IsInside(TinyRect rect, TinyPoint p)
        {
            return p.X >= rect.LeftX && p.X <= rect.RightX && p.Y >= rect.TopY && p.Y <= rect.BottomY;
        }

        public static bool IntersectRect(out TinyRect dst, TinyRect src1, TinyRect src2)
        {
            dst = default(TinyRect);
            dst.LeftX = Math.Max(src1.LeftX, src2.LeftX);
            dst.RightX = Math.Min(src1.RightX, src2.RightX);
            dst.TopY = Math.Max(src1.TopY, src2.TopY);
            dst.BottomY = Math.Min(src1.BottomY, src2.BottomY);
            return !IsRectEmpty(dst);
        }

        public static bool UnionRect(out TinyRect dst, TinyRect src1, TinyRect src2)
        {
            dst = default(TinyRect);
            dst.LeftX = Math.Min(src1.LeftX, src2.LeftX);
            dst.RightX = Math.Max(src1.RightX, src2.RightX);
            dst.TopY = Math.Min(src1.TopY, src2.TopY);
            dst.BottomY = Math.Max(src1.BottomY, src2.BottomY);
            return !IsRectEmpty(dst);
        }

        private static bool IsRectEmpty(TinyRect rect)
        {
            if (rect.LeftX < rect.RightX)
            {
                return rect.TopY >= rect.BottomY;
            }
            return true;
        }

    }
}