﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using MapPosition.Map;

namespace TestES
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal db=117.84002737023984M;

            //ElasticSearchClient.InsertEntityToES(new OuterRequest
            //{
            //    NickName="李金川",
            //    Uid=10000
            //});

            //var ct = Map.MapCore.LoadChinaCountryMap();

            var area = MapCore.Location(131.140361, 44.222059);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Restart();
            for (int i = 0; i < 1000000;i++ )
            {
                //area = Map.MapCore.China.Position(new MapPoint(131.140361m, 44.222059m));
                MapCore.Location(131.140361, 44.222059);
            }
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            //Console.Read();
        }
    }
}