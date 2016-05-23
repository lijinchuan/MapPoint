using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapPosition.Map
{
    public class MapPoint
    {
        /// <summary>
        /// 位置
        /// </summary>
        /// <param name="_x">经度</param>
        /// <param name="_y">纬度</param>
        public MapPoint(double _x, double _y)
        {
            this.X = _x;
            this.Y = _y;
        }

        public MapPoint()
        {

        }

        public string Postion
        {
            get;
            set;
        }

        /// <summary>
        /// 经度
        /// </summary>
        public double X
        {
            get;
            set;
        }

        /// <summary>
        /// 纬度
        /// </summary>
        public double Y
        {
            get;
            set;
        }
    }
}
