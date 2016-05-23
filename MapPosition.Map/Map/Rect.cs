using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapPosition.Map
{
    public class Rect
    {
        private double _top = double.MinValue;
        public double Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
            }
        }

        private double _right = double.MinValue;
        public double Right
        {
            get
            {
                return _right;
            }
            set
            {
                _right = value;
            }
        }

        private double _bottom = double.MinValue;
        public double Bottom
        {
            get
            {
                return _bottom;
            }
            set
            {
                _bottom = value;
            }
        }

        private double _left = double.MinValue;
        public double Left
        {
            get
            {
                return _left;
            }
            set
            {
                _left = value;
            }
        }

        public Rect()
        {

        }

        public Rect(double top, double right, double bottom, double left)
        {
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Left = left;
        }

        public void Expand(Rect otherRect)
        {
            if (otherRect == null)
                return;

            if (otherRect.Top > Top)
            {
                Top = otherRect.Top;
            }

            if (otherRect.Right > Right)
            {
                Right = otherRect.Right;
            }

            if (this.Bottom == double.MinValue || otherRect.Bottom < this.Bottom)
            {
                this.Bottom = otherRect.Bottom;
            }

            if (this.Left == double.MinValue || otherRect.Left < this.Left)
            {
                this.Left = otherRect.Left;
            }
        }

        public bool IsInclude(MapPoint point)
        {
            if (point == null)
                return false;

            return point.X >= Left && point.X <= Right && point.Y <= Top && point.Y >= Bottom;
        }
    }
}
