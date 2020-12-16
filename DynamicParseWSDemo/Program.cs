using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Schema;

namespace DynamicParseWSDemo
{
    class Program
    {
        private static void Main(string[] args)
        {
            //WSProxy.CallWebService(@"http://10.57.254.141:58251/Adapter/SMSC_R2R_DispatchService.asmx",
            //    "test", "test", new string[] { "Test" });

            //WSHandler.GenerateServiceAndMethodName(@"http://10.57.254.141:58251/Adapter/SMSC_R2R_ControllerService.asmx");
            var webService = WSFactory.CreateWebService(@"http://10.57.254.141:58251/Adapter/SMSC_R2R_DispatchService.asmx");

            WSFactory.CallWebService(webService);

        }


    }
}
