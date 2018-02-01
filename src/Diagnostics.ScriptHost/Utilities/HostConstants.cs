using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.Utilities
{
    public class HostConstants
    {
        public const string RegistryRootPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\IIS Extensions\Web Hosting Framework";

        // Ideally, this should move to Data Providers

        public static TimeSpan KustoDataRetentionPeriod = TimeSpan.FromDays(-30);

        public static TimeSpan KustoDataLatencyPeriod = TimeSpan.FromMinutes(15);

        public const int DefaultTimeGrainInMinutes = 5;

        public const string KustoTimeFormat = "yyyy-MM-dd HH:mm:ss";

        #region Time Grain Constants

        public static List<Tuple<TimeSpan, TimeSpan, bool>> TimeGrainOptions = new List<Tuple<TimeSpan, TimeSpan, bool>>
            {
                // 5 minute grain - max time range 1 day
                new Tuple<TimeSpan, TimeSpan, bool>(TimeSpan.FromMinutes(5), TimeSpan.FromDays(1), true),

                // 30 minute grain - max time range 3 days
                new Tuple<TimeSpan, TimeSpan, bool>(TimeSpan.FromMinutes(30), TimeSpan.FromDays(3),  false),
                
                // 1 hour grain - max time range 7 days
                new Tuple<TimeSpan, TimeSpan, bool>(TimeSpan.FromHours(1), TimeSpan.FromDays(7), false),
            };

        #endregion


    }
}
