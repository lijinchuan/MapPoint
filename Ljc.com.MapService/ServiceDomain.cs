using Ljc.Com.MapService.Contract;
using LJC.FrameWork.EntityBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ljc.com.MapService
{
    public class ServiceDomain : LJC.FrameWork.SOA.ESBService
    {
        public ServiceDomain() 
            : base(Consts.ServiceNo, false, false, serviceName: "mapservice", endPointName: "地图服务")
        {

        }

        public override object DoResponse(int funcId, byte[] Param, string clientid)
        {
            switch (funcId)
            {
                case Consts.Fun_GetChinaCityList:
                    {
                        var req = EntityBufCore.DeSerialize<GetChinaCityListRequest>(Param);
                        return new WebApiDomain().GetChineseCityInfos(req);
                    }
            }
            return base.DoResponse(funcId, Param, clientid);
        }
    }
}
