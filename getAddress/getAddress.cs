using System;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
// Microsoft Dynamics CRM namespace(s)
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;
using System.Data;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;

namespace getAddress
{
    public class getAddress : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            Guid fsCurrentId = Guid.Empty;
            IOrganizationServiceFactory serviceFactory;
            IOrganizationService service;
            OrganizationServiceContext orgContext;
            try
            {
                // Obtain the execution context from the service provider.
                Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)
                    serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));

                // The InputParameters collection contains all the data passed in the message request.
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {

                    serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    service = serviceFactory.CreateOrganizationService(context.UserId);
                    using (orgContext = new OrganizationServiceContext(service))
                    {

                        Entity entity = (Entity)context.InputParameters["Target"];
                        entity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                        if (entity.Contains("new_latitude") && entity.Contains("new_longitude"))
                        {
                            String lat = entity.GetAttributeValue<String>("new_latitude");
                            String lon = entity.GetAttributeValue<String>("new_longitude");


                            RootObject rootObject = findAddress(Convert.ToDouble(lat), Convert.ToDouble(lon));
                            if (rootObject.display_name != null)
                            {
                                Entity updatelocation = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(false));
                                updatelocation.Attributes["new_address"] = rootObject.display_name;
                                service.Update(updatelocation);

                            }



                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in retrieveing GetAddress"+ex.Message);
            }
        }

        public static RootObject findAddress(double lat, double lon)
        {

            WebClient webClient = new WebClient();

            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            webClient.Headers.Add("Referer", "http://www.microsoft.com");

            var jsonData = webClient.DownloadData("http://nominatim.openstreetmap.org/reverse?format=json&lat=" + lat + "&lon=" + lon);


            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(RootObject));
            RootObject rootObject = (RootObject)ser.ReadObject(new MemoryStream(jsonData));
            return rootObject;

        }

    }
}
