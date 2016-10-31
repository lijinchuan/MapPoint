using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestES.Map;

namespace MapPosition.Map
{
    public class MapCore
    {
        private static string ChinaMapDataPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\MapData\\chinaBoudler.xml";
        private static Country China = null;

        private static string ForeignerMapDataPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\MapData\\foreignermappoint.xml";
        static Dictionary<string, string> ForeignerPostionDic = new Dictionary<string, string>();

        const string DefaultLocation = "未知星球";

        static MapCore()
        {
            LoadChinaCountryMap();
            LoadForeignerMap();
        }

        /// <summary>
        /// 装载中国地图
        /// </summary>
        /// <returns></returns>
        private static void LoadChinaCountryMap()
        {
            China = XMLHelper.DeSerializerFile<Country>(ChinaMapDataPath);
            China.FillName("");
        }

        private static string GetForeignerMapFindKey(double longitude, double latitude)
        {
            return longitude.ToString("f0") + "_" + latitude.ToString("f0");
        }

        private static void LoadForeignerMap()
        {
            var list = XMLHelper.DeSerializerFile<List<MapPoint>>(ForeignerMapDataPath);

            foreach (var mp in list)
            {
                if (string.IsNullOrWhiteSpace(mp.Postion)
                    || mp.Postion.IndexOf("ZERO_RESULTS") > -1)
                    continue;

                string key = GetForeignerMapFindKey(mp.X, mp.Y);
                if (!ForeignerPostionDic.ContainsKey(key))
                {
                    ForeignerPostionDic.Add(key, mp.Postion);
                }
            }
        }


        /// <summary>
        /// 定位,国内精确判断，国外粗略
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns></returns>
        public static string Location(double longitude, double latitude)
        {
            if (longitude < 0 || latitude < 0)
            {
                string key = GetForeignerMapFindKey(longitude, latitude);
                string location;
                ForeignerPostionDic.TryGetValue(key, out location);

                return location ?? DefaultLocation;
            }

            var gcj = GPS.Gcj_encrypt(latitude, longitude);
            var bd = GPS.Bd_encrypt(gcj[0], gcj[1]);
            var area = China.Position(new MapPoint(bd[1], bd[0]));

            if (area != null)
            {
                return area.Name;
            }
            else
            {
                string key = GetForeignerMapFindKey(longitude, latitude);
                string location;
                ForeignerPostionDic.TryGetValue(key, out location);

                return location ?? DefaultLocation;
            }
          
        }
    }
}
