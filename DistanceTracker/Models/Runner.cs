using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class Runner
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Age => BirthDate.ToAgeString();
        public string Sex { get; set; }
        public string BibNumber { get; set; }
        public string EventName { get; set; }
    }
}
