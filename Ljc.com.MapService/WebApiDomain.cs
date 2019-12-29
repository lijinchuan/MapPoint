using Ljc.Com.MapService.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ljc.com.MapService
{
    public class WebApiDomain
    {
        [LJC.FrameWork.HttpApi.APIMethod]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<CityInfo> GetChineseCityInfos(GetChinaCityListRequest request)
        {
            return MapPosition.Map.MapCore.GetChinaProvinceList(request.ProvinceName)?.Where(p=>p.Cities!=null)
                .SelectMany(p => p.Cities.Select(q=>new
            CityInfo
            {
                CityName=q.Name,
                ProvinceName=p.Name,
                CountryName="中国"
            })).ToList();
        }
    }
}
