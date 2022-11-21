using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class LapRecord
    {
        public string LapDistance { get; set; }
        public string BibNumber { get; set; }
        public string RunnerName { get; set; }
        public string RaceEventName { get; set; }
        public DateTime LapCompletedTime { get; set; }
        public int LapTimeSpan { get; set; }
        public string Id { get; set; }
    }
}
