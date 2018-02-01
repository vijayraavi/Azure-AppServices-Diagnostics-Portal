using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.Models
{
    public class OperationContext
    {
        public SiteResource Resource;

        public string StartTime;

        public string EndTime;

        public OperationContext(SiteResource resource, string startTimeStr, string endTimeStr)
        {
            Resource = resource;
            StartTime = startTimeStr;
            EndTime = endTimeStr;
        }
    }
}
