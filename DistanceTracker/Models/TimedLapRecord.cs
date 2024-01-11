using DryIoc.FastExpressionCompiler.LightExpression;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class TimedLapRecord : INotifyPropertyChanged
    {
        public string LapDistance { get; set; }
        public string BibNumber { get; set; }
        public string RunnerName { get; set; }
        public string RunnerId { get; set; }
        public string RaceEventName { get; set; }
        public string LapStartedTime { get; set; } = "START";
        public string LapCompletedTime { get; set; } = "STOP";
        public DateTime CreatedTime { get; set; }

        public string Id { get; set; }

        [JsonIgnore]
        public DateTime? LapStartedDateTime => TryParseDateTime(LapStartedTime);

        [JsonIgnore]
        public DateTime? LapCompletedDateTime => TryParseDateTime(LapCompletedTime);

        [JsonIgnore]
        public DateTime? LapStartedTimeLocal => GetLocalTime(LapStartedDateTime);

        [JsonIgnore]
        public DateTime? LapCompletedTimeLocal => GetLocalTime(LapCompletedDateTime);

        [JsonIgnore]
        public bool IsLapFinished => LapCompletedDateTime.HasValue;

        [JsonIgnore]
        public string LapStartDisplay => GetStartForDisplay(LapStartedTimeLocal);

        [JsonIgnore]
        public string LapStopDisplay => GetStopForDisplay(LapCompletedTimeLocal);

        [JsonIgnore]
        public string LapDistanceDisplay => $"{LapDistance} Mile";


        [JsonIgnore]
        public string TotalLapTime => GetTotalLapTime(LapStartedTimeLocal, LapCompletedTimeLocal);

        public string GetTotalLapTime(DateTime? startTime, DateTime? endTime)
        {
            if (startTime.HasValue && endTime.HasValue)
            {
                DateTime start = (DateTime)startTime;
                DateTime end = (DateTime)endTime;

                // Calculate the time delta
                TimeSpan timeDelta = end - start;

                System.Diagnostics.Debug.WriteLine(timeDelta.ToString());

                // Format and display the time delta
                //string formattedTimeDelta = timeDelta.ToString(@"hh\:mm\:ss");
                string formattedTimeDelta = timeDelta.ToString();
                return formattedTimeDelta;
            }
            else
                return string.Empty;
        }

        public static string GetStartForDisplay(DateTime? startTimeLocal)
        {
            if (startTimeLocal.HasValue)
                return startTimeLocal.ToString();
            else
                return "START";
        }

        public static string GetStopForDisplay(DateTime? stopTimeLocal)
        {
            if (stopTimeLocal.HasValue)
                return stopTimeLocal.ToString();
            else
                return "STOP";
        }

        public static DateTime? TryParseDateTime(string dateTimeString)
        {
            if (DateTime.TryParse(dateTimeString, out DateTime result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public DateTime? GetLocalTime (DateTime? lapCompleted)
        {
            if (lapCompleted.HasValue)
            {
                TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                DateTime estTime = TimeZoneInfo.ConvertTimeFromUtc((DateTime)lapCompleted, estZone);
                return estTime;
            }
            else
                return null;           
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class TimedLapRecordUpdateModel
    {
        public string LapStartedTime { get; set; }
        public string LapCompletedTime { get; set; }
    }
}
