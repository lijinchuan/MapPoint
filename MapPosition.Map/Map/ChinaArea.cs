﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace MapPosition.Map
{
    [Serializable]
    public abstract class ChinaArea
    {
        private const int Earth_Radii = 6378137;
        private static ConcurrentDictionary<int, List<ChinaArea>> PositionDic = new ConcurrentDictionary<int, List<ChinaArea>>();

        public static void ClearPositionNear()
        {
            List<ChinaArea> list;
            var key = Thread.CurrentThread.ManagedThreadId;
            PositionDic.TryRemove(key, out list);
        }

        static void AddPostion(ChinaArea positon)
        {
            var key = Thread.CurrentThread.ManagedThreadId;
            List<ChinaArea> list = null;
            if (PositionDic.TryGetValue(key, out list))
            {
                list.Add(positon);
            }
            else
            {
                list = new List<ChinaArea>();
                list.Add(positon);
                PositionDic.TryAdd(key, list);
            }
        }

        public static double Rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        public static double CalDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = Rad(lat1), radLat2 = Rad(lat2), radLng1 = Rad(lng1), radLng2 = Rad(lng2);
            var s = CalDistance2(radLat1, radLng1, radLat2, radLng2);
            return s;
        }

        public static double CalDistance2(double radLat1, double radLng1, double radLat2, double radLng2)
        {
            var s = Math.Acos(Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Cos(radLng1 - radLng2) + Math.Sin(radLat1) * Math.Sin(radLat2)) * Earth_Radii;
            return s;
        }

        public static ChinaArea GetNearPosition(MapPoint point)
        {
            var key = Thread.CurrentThread.ManagedThreadId;
            List<ChinaArea> list = null;
            ChinaArea minitem = null;
            var min = double.MaxValue;
            var radlat = Rad(point.Y);
            var radlon = Rad(point.X);
            if (PositionDic.TryGetValue(key, out list))
            {
                list = list.Select(p => p).ToList();
                foreach (var item in list)
                {
                    if (item is Province)
                    {
                        var cities = (item as Province).Cities;
                        if (cities == null)
                        {
                            foreach (var pts in item.MapPointsList)
                            {
                                foreach (var pt in pts)
                                {
                                    var dis = CalDistance2(radlat, radlon, Rad(pt.Y), Rad(pt.X));
                                    if (dis < min)
                                    {
                                        min = dis;
                                        minitem = item;
                                    }
                                }
                            }

                            continue;
                        }
                        foreach (var city in cities)
                        {
                            var ct = city.Position(point);
                            if (ct != null)
                            {
                                return ct;
                            }

                            foreach (var pts in city.MapPointsList)
                            {
                                foreach (var pt in pts)
                                {
                                    var dis = CalDistance2(radlat, radlon, Rad(pt.Y), Rad(pt.X));
                                    if (dis < min)
                                    {
                                        min = dis;
                                        minitem = city;
                                    }
                                }
                            }
                        }
                    }
                }
                if (min < 100000)
                {
                    return minitem;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        [XmlAttribute("ID")]
        public string ID
        {
            get;
            set;
        }

        [XmlAttribute("code")]
        public string Code
        {
            get;
            set;
        }

        [XmlAttribute("name")]
        public string Name
        {
            get;
            set;
        }

        [XmlAttribute("rings")]
        public string Rings
        {
            get;
            set;
        }

        private List<MapPoint[]> _mapPointsList = null;
        [XmlIgnore]
        public List<MapPoint[]> MapPointsList
        {
            get
            {
                if (_mapPointsList == null && Rings != null)
                {
                    lock (this)
                    {
                        if (_mapPointsList == null)
                        {
                            _mapPointsList = new List<MapPoint[]>();
                            var ringsArray = Rings.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                            Rects = new Rect[ringsArray.Length];
                            MaxX = new double[ringsArray.Length];
                            MaxY = new double[ringsArray.Length];

                            for (int i = 0; i < ringsArray.Length; i++)
                            {
                                List<MapPoint> list = new List<MapPoint>();
                                double top = double.MinValue, right = double.MinValue, bottom = double.MinValue, left = double.MinValue;
                                MaxX[i] = double.MinValue;
                                MaxY[i] = double.MinValue;
                                MapPoint lastPt = null;

                                var arr = ringsArray[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var s in arr)
                                {
                                    var arr2 = s.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arr2.Length != 2)
                                    {
                                        throw new InvalidCastException("数据错误，不是正确的经纬度数据:" + s);
                                    }
                                    var pt = new MapPoint(double.Parse(arr2[0]), double.Parse(arr2[1]));


                                    if (pt.Y > top)
                                    {
                                        top = pt.Y;
                                    }

                                    if (pt.X > right)
                                    {
                                        right = pt.X;
                                    }

                                    if (bottom == double.MinValue || pt.Y < bottom)
                                    {
                                        bottom = pt.Y;
                                    }

                                    if (left == double.MinValue || pt.X < left)
                                    {
                                        left = pt.X;
                                    }

                                    if (lastPt != null)
                                    {
                                        MaxX[i] = Math.Max(MaxX[i], Math.Abs(lastPt.X - pt.X));
                                        MaxY[i] = Math.Max(MaxY[i], Math.Abs(lastPt.Y - pt.Y));
                                    }

                                    lastPt = pt;

                                    list.Add(pt);
                                }
                                Rects[i] = new Rect(top, right, bottom, left);
                                _mapPointsList.Add(list.ToArray());
                            }
                        }
                    }
                }

                return _mapPointsList ?? new List<MapPoint[]>();
            }
        }

        #region 优化的东西
        private Rect[] _rects = null;
        [XmlIgnore]
        public Rect[] Rects
        {
            get
            {
                if (_rects == null)
                {
                    var mp = MapPointsList;
                }
                return _rects;
            }
            protected set
            {
                _rects = value;
            }
        }

        private double[] _maxX = null;
        [XmlIgnore]
        public double[] MaxX
        {
            get
            {
                if (_maxX == null)
                {
                    var mp = MapPointsList;
                }
                return _maxX;
            }
            set
            {
                _maxX = value;
            }
        }

        private double[] _maxY = null;
        [XmlIgnore]
        public double[] MaxY
        {
            get
            {
                if (_maxY == null)
                {
                    var mp = MapPointsList;
                }
                return _maxY;
            }
            set
            {
                _maxY = value;
            }
        }
        #endregion

        /// <summary>
        /// 判断一个点是否在多边形区域内
        /// </summary>
        /// <param name="points"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsInArea(MapPoint[] points, double minxscral, double minyscral, MapPoint point)
        {
            if (points == null || points.Length < 2)
                return false;

            List<double> ylist = new List<double>();
            List<double> xlist = new List<double>();

            MapPoint p1 = null, p2 = null;
            double crossX = double.MinValue, crossY = double.MinValue;

            for (int i = 0; i < points.Length - 1; i++)
            {
                p1 = points[i];
                p2 = points[i + 1];

                if (minxscral == 0 || Math.Abs(p1.X - point.X) <= minxscral)
                {
                    if ((p1.X <= point.X && point.X <= p2.X) || (p2.X <= point.X && point.X <= p1.X))
                    {
                        if (p1.X != p2.X)
                        {
                            crossY = p1.Y + ((p2.Y - p1.Y) / Math.Abs(p2.X - p1.X)) * Math.Abs(p1.X - point.X);
                        }
                        else
                        {
                            if ((p1.Y - point.Y) * (p2.Y - point.Y) <= 0)
                            {
                                crossY = point.Y;
                            }
                            else
                            {
                                crossY = p1.Y + (p2.Y - p1.Y) / 2;
                            }
                        }
                        ylist.Add(crossY);
                    }
                }

                if (minyscral == 0 || Math.Abs(p1.Y - point.Y) <= minyscral)
                {
                    if ((p1.Y <= point.Y && p2.Y >= point.Y) || (p2.Y <= point.Y && p1.Y >= point.Y))
                    {
                        if (p1.Y != p2.Y)
                        {
                            crossX = p1.X + (p2.X - p1.X) / Math.Abs(p2.Y - p1.Y) * Math.Abs(p1.Y - point.Y);
                        }
                        else
                        {
                            if ((p1.X - point.X) * (p2.X - point.X) <= 0)
                            {
                                crossX = point.X;
                            }
                            else
                            {
                                crossX = p1.X + (p2.X - p1.X) / 2;
                            }
                        }
                        xlist.Add(crossX);
                    }
                }
            }

            if (xlist.Count == 0 || ylist.Count == 0)
                return false;

            xlist = xlist.Distinct().OrderBy(p => p).ToList();
            ylist = ylist.Distinct().OrderBy(p => p).ToList();

            for (int i = 0; i < xlist.Count; i++)
            {
                var curr = xlist[i];
                if (point.X < curr)
                {
                    if (i % 2 == 0)
                        return false;
                    else
                        break;
                }
                else if (i == xlist.Count - 1 && point.X > curr)
                {
                    return false;
                }
            }

            for (int j = 0; j < ylist.Count; j++)
            {
                var curr = ylist[j];
                if (point.Y < curr)
                {
                    if (j % 2 == 0)
                        return false;
                    else
                        return true;
                }
                else if (j == ylist.Count - 1 && point.Y > curr)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual ChinaArea Position(MapPoint point)
        {
            if (point == null)
                return null;

            if (Rects == null)
                return null;

            bool isInrect = false;
            int i = 0;
            foreach (var rt in Rects)
            {
                isInrect = rt.IsInclude(point);
                if (!isInrect)
                {
                    i++;
                    continue;
                }

                if (IsInArea(this.MapPointsList[i], MaxX != null ? MaxX[i] : 0, MaxY != null ? MaxY[i] : 0, point))
                {
                    return this;
                }
                else
                {
                    AddPostion(this);
                }

                i++;
            }

            return null;
        }

        public virtual void FillName(string owner)
        {
            if (!string.IsNullOrEmpty(owner))
                this.Name = owner + this.Name;
        }
    }
}
