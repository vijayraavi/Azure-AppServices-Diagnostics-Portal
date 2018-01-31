using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.DataProviders.Models
{
    class SignalResponse
    {
        public DataTableResponseObject Data { get; set; }

        public SignalMetaData SignalMetaData { get; set; }

        public RenderingDefinition RenderingDefinition { get; set; }

        public SignalResponse()
        {
            Data = new DataTableResponseObject();
            SignalMetaData = new SignalMetaData();
        }
    }

    public class SignalMetaData
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }
    }

    public class RenderingDefinition
    {
        public SignalGraphType SignalGraphType { get; set; }
    }

    public enum SignalGraphType
    {
        NoGraph = 0,
        Table,
        TimeSeries
    }
}
