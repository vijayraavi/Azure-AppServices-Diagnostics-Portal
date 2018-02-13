using Diagnostics.ScriptHost.Utilities;

namespace Diagnostics.ScriptHost.Models
{
    public class OperationContext
    {
        public SiteResource Resource;

        public string StartTime;

        public string EndTime;

        public string TimeGrain;

        public OperationContext(SiteResource resource, string startTimeStr, string endTimeStr)
        {
            Resource = resource;
            StartTime = startTimeStr;
            EndTime = endTimeStr;
            TimeGrain = HostConstants.DefaultTimeGrainInMinutes.ToString();
        }
    }
}
