using System;

namespace MbtaApp.Models
{
    public class DepartureResponse
    {
        public DepartureResponse(string departureTime, string trackNumber, string status, int directionId)
        {
            Carrier = "MBTA";
            DepartureTime = departureTime;
            TrackNumber = trackNumber;
            Status = status;
            DirectionId = directionId;
        }

        public string Carrier { get; set; }
        
        public int DirectionId { get; set; }
        
        public string Destination { get; set; }

        private string _departureTime;
        public string DepartureTime
        {
            get => _departureTime;
            set
            {
                if (value == "TBD")
                {
                    _departureTime = "TBD";
                }
                else
                {
                    var dateTime = Convert.ToDateTime(value);
                    var formatedDateTime = dateTime.ToString("h:mm tt");
                
                    _departureTime = formatedDateTime;
                }
            }
        }

        private string _trackNumber;
        public string TrackNumber
        {
            get => _trackNumber;
            set
            {
                if (value == "0")
                {
                    _trackNumber = "TBD";
                }
                else
                {
                    _trackNumber = value;
                }
            }
        }

        private string _status;
        public string Status         
        {
            get => _status;
            set => _status = value.ToUpper();
        }
    }
}