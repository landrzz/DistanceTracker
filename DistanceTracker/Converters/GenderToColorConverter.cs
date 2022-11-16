using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DistanceTracker
{
    public class GenderToColorConverter : IValueConverter
    {
        static readonly Color unknownColorDefault = Colors.LightGray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culutre)
        {
            try
            {
                var gender = value as string;
                var genderLower = gender.ToLower();

                if (genderLower.Contains("female"))
                {
                    return Colors.LightPink;
                }
                else if (genderLower.Contains("male"))
                {
                    return Colors.LightBlue;
                }
                return unknownColorDefault;
            }
            catch (Exception ex)
            {
                return unknownColorDefault;
            }
        }

        public object ConvertBack(object v, Type tt, object p, CultureInfo c) => null;
    }
}
