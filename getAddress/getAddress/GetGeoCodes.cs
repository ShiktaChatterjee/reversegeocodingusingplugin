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
using System.Xml.Linq;

namespace getAddress
{
  public  class GetGeoCodes : IPlugin
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
                        Geocoding(entity, service);
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        public static void Geocoding(Entity entity, IOrganizationService service)
        {
            if (entity.Contains("address1_composite"))
            {
                String address1 = entity.GetAttributeValue<String>("address1_composite");
                String[] geocodes = GetGeoCodesGoogle(address1);
                if (geocodes.Length == 2 && geocodes[0] != null && geocodes[1] != null)
                {
                    Entity uentity = new Entity(entity.LogicalName);
                    uentity.Id = entity.Id;
                    uentity["address1_latitude"] = Convert.ToDecimal(geocodes[0]);
                    uentity["address1_longitude"] = Convert.ToDecimal(geocodes[1]);
                    service.Update(uentity);
                }

            }
            if (entity.Contains("address2_composite"))
            {
                String address2 = entity.GetAttributeValue<String>("address2_composite");
                String[] geocodes = GetGeoCodesGoogle(address2);
                if (geocodes.Length == 2 && geocodes[0] != null && geocodes[1] != null)
                {

                    Entity uentity = new Entity(entity.LogicalName);
                    uentity.Id = entity.Id;
                    uentity["address2_latitude"] = Convert.ToDecimal(geocodes[0]);
                    uentity["address2_longitude"] = Convert.ToDecimal(geocodes[1]);
                    service.Update(uentity);
                }

            }
        }

        public static String[] GetGeoCodesGoogle(String address)
        {
            String[] geocodes = new String[2];
            try
            {
                if (!String.IsNullOrEmpty(address))
                {
                    string requestUri = string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false", Uri.EscapeDataString(address));

                    WebRequest request = WebRequest.Create(requestUri);

                    WebResponse response = request.GetResponse();

                    XDocument xdoc = XDocument.Load(response.GetResponseStream());
                    String status = xdoc.Element("GeocodeResponse").Element("status").Value;
                    if (status == "OK")
                    {
                        XElement result = xdoc.Element("GeocodeResponse").Element("result");
                        XElement locationElement = result.Element("geometry").Element("location");
                        XElement lat = locationElement.Element("lat");
                        XElement lng = locationElement.Element("lng");
                        String latvalue = lat.Value;
                        String lngvalue = lng.Value;
                        geocodes[0] = latvalue;
                        geocodes[1] = lngvalue;

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return geocodes;
        }

    }
}
