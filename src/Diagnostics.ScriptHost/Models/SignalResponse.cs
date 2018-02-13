using Diagnostics.DataProviders;
using System.Collections.Generic;

namespace Diagnostics.ScriptHost.Models
{
    public class SignalResponse
    {
        public Definition Metadata { get; set; }

        public List<DiagnosticData> Dataset { get; set; }

        public SignalResponse()
        {
            Metadata = new Definition();
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
