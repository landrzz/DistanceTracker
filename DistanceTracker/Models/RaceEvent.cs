using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class RaceEvent
    {
        public string EventName { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string EventYear { get; set; }
        public string EventPassCode { get; set; }
        public string EventStartTimestamp { get; set; }
        public string Id { get; set; }

        [JsonIgnore]
        public string EventDateFormatted => EventDate.ToString("dddd, MM/dd/yyyy");

    }
}
