using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostic.DataProviders
{
    public interface IDataProviderConfiguration
    {
        void PostInitialize();
    }
}
