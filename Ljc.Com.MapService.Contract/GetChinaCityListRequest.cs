using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ljc.Com.MapService.Contract
{
    public class GetChinaCityListRequest
    {
        /// <summary>
        /// 省名称，可以不填写
        /// </summary>
        public string ProvinceName
        {
            get;
            set;
        }
    }
}
