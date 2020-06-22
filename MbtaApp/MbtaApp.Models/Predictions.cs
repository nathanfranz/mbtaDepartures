using System.Collections.Generic;

namespace MbtaApp.Models
{
    public class Predictions
    {
        public List<PredictionResource> data { get; set; }
    }
    
    public class PredictionResource
    {
        public string type { get; set; }
        
        public string id { get; set; }
        
        public string trackNumber { get; set; } // this is not part of the response but added here to use in BL logic
        
        public PredictionAttributes attributes { get; set; }
        
        public Relationships relationships { get; set; }
    }
    
    public class PredictionAttributes
    {
        public string arrival_time { get; set; }
        public string departure_time { get; set; }
        
        public int direction_id { get; set; }
        
        public string status { get; set; }
    }
}