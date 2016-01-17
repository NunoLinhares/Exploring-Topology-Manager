using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using Tridion.TopologyManager.Client;
using System.ServiceModel;

namespace ExploreTopMan
{
    class Program
    {
        private static TopologyManagerClient _ttm;
        private static List<CdTopologyData> _topologies = null;
        private static List<CdEnvironmentData> _environments = null;
        private static List<WebsiteData> _websites = null;
        private static List<WebApplicationData> _webApplications = null;
        private static List<MappingData> _mappings = null;

        static void Main(string[] args)
        {
            _ttm = new TopologyManagerClient(new Uri("http://184.72.104.164:81/ttm201501"))
            {
                Credentials = new NetworkCredential("Administrator", "Tr1d10n")
            };

            foreach (CmEnvironmentData cm in GetCmEnvironmentList())
            {
                Console.WriteLine("Found cm with ID: " + cm.Id);
            }

            foreach (CdTopologyTypeData ttd in GetTopologyTypes())
            {
                Console.WriteLine("Exploring Topology Type: " + ttd.Name);
                foreach (CdTopologyData topology in GetTopologiesOfType(ttd.Id) )
                {
                    Console.WriteLine("  >  Topology Name: " + topology.Name);

                    foreach (string id in topology.CdEnvironmentIds)
                    {
                        CdEnvironmentData cdEnvironment = GetCdEnvironmentById(id);
                        if (cdEnvironment != null)
                        {
                            Console.WriteLine(".Environment Id: " + cdEnvironment.Id);
                            Console.WriteLine("..Is offline: " + cdEnvironment.IsOffline);
                            Console.WriteLine("..Purpose: " + cdEnvironment.EnvironmentPurpose);
                            foreach (WebsiteData website in GetWebsitesByEnvironmentId(cdEnvironment.Id))
                            {
                                Console.WriteLine("...Website ID: " + website.Id);
                                Console.WriteLine("...Website Purpose: " + website.EnvironmentPurpose);
                                foreach (string url in website.BaseUrls)
                                {
                                    Console.WriteLine("...Website base Url: " + url);    
                                }
                                foreach (WebApplicationData webApp in GetWebApplicationsByWebsiteId(website.Id))
                                {
                                    Console.WriteLine("....WebApp Id: " + webApp.Id);
                                    Console.WriteLine("....WebApp Context Url: " + webApp.ContextUrl);
                                    foreach (MappingData mapping in GetMappingsByWebAppId(webApp.Id))
                                    {
                                        Console.WriteLine(".....Mapped publication: " + mapping.PublicationId);
                                        Console.WriteLine(".....Mapping ID: " + mapping.Id);
                                        Console.WriteLine(".....Relative URL: " + mapping.RelativeUrl);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.Read();

        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public static List<CmEnvironmentData> GetCmEnvironmentList()
        {
            return _ttm.CmEnvironments.ToList();
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public static List<CdTopologyTypeData> GetTopologyTypes()
        {
            return _ttm.CdTopologyTypes.ToList();
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public static List<CdTopologyData> GetTopologiesOfType(string topologyTypeId)
        {
            if(_topologies == null)
                _topologies = _ttm.CdTopologies.ToList();
            return _topologies.Where(data => data.CdTopologyTypeId == topologyTypeId).ToList();
            
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public static CdEnvironmentData GetCdEnvironmentById(string environmentId)
        {
            if (_environments == null)
                _environments = _ttm.CdEnvironments.ToList();
            return _environments.FirstOrDefault(cdEnvironment => cdEnvironment.Id.Equals(environmentId));
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public static List<WebsiteData> GetWebsitesByEnvironmentId(string environmentId)
        {
            if (_websites == null)
                _websites= _ttm.Websites.ToList();
            return _websites.Where(data => data.CdEnvironmentId == environmentId).ToList();
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public static List<WebApplicationData> GetWebApplicationsByWebsiteId(string websiteId)
        {
            if (_webApplications == null)
                _webApplications = _ttm.WebApplications.ToList();
            return _webApplications.Where(data => data.WebsiteId == websiteId).ToList();
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public static List<MappingData> GetMappingsByWebAppId(string webAppId)
        {
            if (_mappings == null)
                _mappings = _ttm.Mappings.ToList();
            return _mappings.Where(data => data.WebApplicationId == webAppId).ToList();
        }

    }
}
