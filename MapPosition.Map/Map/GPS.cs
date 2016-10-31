using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestES.Map
{
    /// <summary>
    /// http://www.oschina.net/code/snippet_260395_39205
    /// WGS-84：是国际标准，GPS坐标（Google Earth使用、或者GPS模块）
    ///GCJ-02：中国坐标偏移标准，Google Map、高德、腾讯使用
    ///BD-09：百度坐标偏移标准，Baidu Map使用
    ///WGS-84 to GCJ-02
    ///GPS.gcj_encrypt();
    ///GCJ-02 to WGS-84 粗略
    ///GPS.gcj_decrypt();
    ///GCJ-02 to WGS-84 精确(二分极限法)
    ///var threshold = 0.000000001; 目前设置的是精确到小数点后9位，这个值越小，越精确，但是javascript中，浮点运算本身就不太精确，九位在GPS里也偏差不大了
    ///GSP.gcj_decrypt_exact();
    ///GCJ-02 to BD-09
    ///GPS.bd_encrypt();
    ///BD-09 to GCJ-02
    ///GPS.bd_decrypt();
    ///求距离
    ///GPS.distance();
    /*示例：
document.write("GPS: 39.933676862706776,116.35608315379092<br />");
var arr2 = GPS.gcj_encrypt(39.933676862706776, 116.35608315379092);
document.write("中国:" + arr2['lat']+","+arr2['lon']+'<br />');
var arr3 = GPS.gcj_decrypt_exact(arr2['lat'], arr2['lon']);
document.write('逆算:' + arr3['lat']+","+arr3['lon']+' 需要和第一行相似（目前是小数点后9位相等）');


本算法 gcj算法、bd算法都非常精确，已经测试通过。
（BD转换的结果和百度提供的接口精确到小数点后4位）
请放心使用*/
    /// </summary>
    public class GPS
    {
        static double PI = 3.14159265358979324;
        static double X_PI = 3.14159265358979324 * 3000.0 / 180.0;
        private static double[] Delta(double lat, double lon)
        {
            // Krasovsky 1940
            //
            // a = 6378245.0, 1/f = 298.3
            // b = a * (1 - f)
            // ee = (a^2 - b^2) / a^2;
            var a = 6378245.0; //  a: 卫星椭球坐标投影到平面地图坐标系的投影因子。
            var ee = 0.00669342162296594323; //  ee: 椭球的偏心率。
            var dLat = TransformLat(lon - 105.0, lat - 35.0);
            var dLon = TransformLon(lon - 105.0, lat - 35.0);
            var radLat = lat / 180.0 * PI;
            var magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            var sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * PI);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * PI);

            return new[] { dLat, dLon };
        }

        //WGS-84 to GCJ-02
        public static double[] Gcj_encrypt(double wgsLat, double wgsLon)
        {
            if (OutOfChina(wgsLat, wgsLon))
                return new[] { wgsLat, wgsLon };

            var d = Delta(wgsLat, wgsLon);
            return new[] { wgsLat + d[0], wgsLon + d[1] };
        }

        //GCJ-02 to WGS-84
        public static double[] Gcj_decrypt(double gcjLat, double gcjLon)
        {
            if (OutOfChina(gcjLat, gcjLon))
                return new[] { gcjLat, gcjLon };

            var d = Delta(gcjLat, gcjLon);
            return new[] { gcjLat - d[0], gcjLon - d[1] };
        }
        //GCJ-02 to WGS-84 exactly
        public static double[] Gcj_decrypt_exact(double gcjLat, double gcjLon)
        {
            var initDelta = 0.01;
            var threshold = 0.000000001;
            var dLat = initDelta;
            var dLon = initDelta;
            var mLat = gcjLat - dLat;
            var mLon = gcjLon - dLon;
            var pLat = gcjLat + dLat;
            var pLon = gcjLon + dLon;
            double wgsLat;
            double wgsLon;
            var i = 0;
            while (true)
            {
                wgsLat = (mLat + pLat) / 2;
                wgsLon = (mLon + pLon) / 2;
                var tmp = Gcj_encrypt(wgsLat, wgsLon);
                dLat = tmp[0] - gcjLat;
                dLon = tmp[1] - gcjLon;
                if ((Math.Abs(dLat) < threshold) && (Math.Abs(dLon) < threshold))
                    break;

                if (dLat > 0) pLat = wgsLat; else mLat = wgsLat;
                if (dLon > 0) pLon = wgsLon; else mLon = wgsLon;

                if (++i > 10000) break;
            }

            return new[] { wgsLat, wgsLon };
        }

        //GCJ-02 to BD-09
        public static double[] Bd_encrypt(double gcjLat, double gcjLon)
        {
            var x = gcjLon;
            var y = gcjLat;
            var z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * X_PI);
            var theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * X_PI);
            var bdLon = z * Math.Cos(theta) + 0.0065;
            var bdLat = z * Math.Sin(theta) + 0.006;

            return new[] { bdLat, bdLon };
        }

        //BD-09 to GCJ-02
        public static double[] Bd_decrypt(double bdLat, double bdLon)
        {
            var x = bdLon - 0.0065;
            var y = bdLat - 0.006;
            var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * X_PI);
            var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * X_PI);
            var gcjLon = z * Math.Cos(theta);
            var gcjLat = z * Math.Sin(theta);

            return new[] { gcjLat, gcjLon };
        }

        //WGS-84 to Web mercator
        //mercatorLat -> y mercatorLon -> x
        public static double[] Mercator_encrypt(double wgsLat, double wgsLon)
        {
            var x = wgsLon * 20037508.34 / 180.0;
            var y = Math.Log(Math.Tan((90.0 + wgsLat) * PI / 360.0)) / (PI / 180.0);
            y = y * 20037508.34 / 180.0;
            return new[] { y, x };

        }

        // Web mercator to WGS-84
        // mercatorLat -> y mercatorLon -> x
        public static double[] Mercator_decrypt(double mercatorLat, double mercatorLon)
        {
            var x = mercatorLon / 20037508.34 * 180.0;
            var y = mercatorLat / 20037508.34 * 180.0;
            y = 180 / PI * (2 * Math.Atan(Math.Exp(y * PI / 180.0)) - PI / 2);
            return new[] { y, x };

        }

        // two point's distance
        public static double Distance(double latA, double lonA, double latB, double lonB)
        {
            var earthR = 6371000.0;
            var x = Math.Cos(latA * PI / 180.0) * Math.Cos(latB * PI / 180.0) * Math.Cos((lonA - lonB) * PI / 180);
            var y = Math.Sin(latA * PI / 180.0) * Math.Sin(latB * PI / 180.0);
            var s = x + y;
            if (s > 1) s = 1;
            if (s < -1) s = -1;
            var alpha = Math.Acos(s);
            var distance = alpha * earthR;
            return distance;
        }
        private static bool OutOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }
        private static double TransformLat(double x, double y)
        {
            var ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }
        private static double TransformLon(double x, double y)
        {
            var ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
            return ret;
        }
    }
}
