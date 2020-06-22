using System.Collections.Generic;

namespace MbtaApp.Models
{
    public class Routes
    {
        public List<RouteResource> data { get; set; }
    }

    public class RouteResource
    {
        public string id { get; set; }
        
        public RouteAttributes attributes { get; set; }
    }

    public class RouteAttributes
    {
        public List<string> direction_destinations { get; set; }
    }
}