using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public static class ConnectivityService
    {
        public static bool IsConnected()
        {
            var current = Connectivity.Current.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                return true;
            }
            return false;
        }
    }
}
