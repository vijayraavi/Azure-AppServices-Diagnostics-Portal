using Diagnostics.DataProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.ScriptHost.Models
{
    public class SignalResponse
    {
        public SignalMetaData Metadata { get; set; }

        public List<DiagnosticData> Dataset { get; set; }

        public SignalResponse()
        {
            Metadata = new SignalMetaData();
            Dataset = new List<DiagnosticData>();
        }
    }

    public class DiagnosticData
    {
        public DataTableResponseObject Table { get; set; }

        public Rendering RenderingProperties { get; set; }

        public DiagnosticData()
        {
            Table = new DataTableResponseObject();
            RenderingProperties = new Rendering();
        }
    }

    public class SignalMetaData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class Rendering
    {
        public GraphType RenderingType { get; set; }

        public Rendering()
        {
            RenderingType = GraphType.TimeSeries;
        }

        public Rendering(GraphType type)
        {
            RenderingType = type;
        }
    }

    public enum GraphType
    {
        NoGraph = 0,
        Table,
        TimeSeries
    }
}
