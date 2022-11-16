using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public static class DateTimeExtensions
    {
        public static string ToAgeString(this DateTime dob)
        {
            DateTime today = DateTime.Today;

            int months = today.Month - dob.Month;
            int years = today.Year - dob.Year;

            if (today.Day < dob.Day)
            {
                months--;
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            int days = (today - dob.AddMonths((years * 12) + months)).Days;

            return years.ToString();
            //return $"{years} year{((years == 1) ? "" : "s")}, {months} month{((months == 1) ? "" : "s")} and {days} day{((days == 1) ? "" : "s")}";
        }
    }
}
