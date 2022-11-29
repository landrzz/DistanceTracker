using DryIoc.FastExpressionCompiler.LightExpression;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class LapRecord
    {
        public string LapDistance { get; set; }
        public string BibNumber { get; set; }
        public string RunnerName { get; set; }
        public string RunnerId { get; set; }
        public string RaceEventName { get; set; }
        public DateTime LapCompletedTime { get; set; }
        public int LapTimeSpan { get; set; }

        public string Id { get; set; }

        [JsonIgnore]
        public DateTime? LapCompletedTimeLocal => GetLocalTime(LapCompletedTime);

        public DateTime GetLocalTime (DateTime lapCompleted)
        {
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            DateTime estTime = TimeZoneInfo.ConvertTimeFromUtc(lapCompleted, estZone);
            return estTime;
        }
    }
}
