// WindowsPhoneSpeedyBlupi, Version=1.0.0.5, Culture=neutral, PublicKeyToken=6db12cd62dbec439
// WindowsPhoneSpeedyBlupi.TinyRect

namespace WindowsPhoneSpeedyBlupi
{
    public struct TinyRect
    {
        public int LeftX;

        public int RightX;

        public int TopY;

        public int BottomY;

        public TinyRect(int leftX, int rightX, int topY, int bottomY)
        {
            LeftX = leftX;
            RightX = rightX;
            TopY = topY;
            BottomY = bottomY;
        }

        public int Width
        {
            get
            {
                return RightX - LeftX;
            }
        }

        public int Height
        {
            get
            {
                return BottomY - TopY;
            }
        }

        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3}", LeftX, TopY, RightX, BottomY);
        }
    }

}