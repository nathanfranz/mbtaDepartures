namespace MbtaApp.Models
{
    // This is the same model for schedules and predictions
    public class Relationships
    {
        public Route route { get; set; }
        
        public Stop stop { get; set; }
        public Trip trip { get; set; }
    }
    
    public class Route
    {
        public Data data { get; set; }
    }
    
    public class Stop
    {
        public Data data { get; set; }
    }
    
    public class Trip
    {
        public Data data { get; set; }
    }
    
    public class Data
    {
        public string id { get; set; }
        
        public string type { get; set; }
    }
}