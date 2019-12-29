using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Ljc.com.MapService
{
    public partial class Service1 : ServiceBase
    {
        ServiceDomain service = null;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LJC.FrameWork.HttpApi.APIFactory.Init("Ljc.com.MapService");

            service = new ServiceDomain();
            service.StartService();
        }

        protected override void OnStop()
        {
            if (service != null)
            {
                service.UnRegisterService();

                service.Dispose();
            }
        }
    }
}
