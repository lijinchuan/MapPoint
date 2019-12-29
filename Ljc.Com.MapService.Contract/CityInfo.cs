using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ljc.Com.MapService.Contract
{
    public class CityInfo
    {
        /// <summary>
        /// 国家名字
        /// </summary>
        public string CountryName
        {
            get;
            set;
        }

        /// <summary>
        /// 省名称
        /// </summary>
        public string ProvinceName
        {
            get;
            set;
        }
        /// <summary>
        /// 城市名称
        /// </summary>
        public string CityName
        {
            get;
            set;
        }
    }
}
