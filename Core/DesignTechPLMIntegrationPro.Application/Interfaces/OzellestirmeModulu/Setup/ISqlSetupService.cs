using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Interfaces.OzellestirmeModulu.Setup
{
    public interface ISqlSetupService
    {
        bool IsConnectionStringValid(out string connectionString);
        bool IsCreatedTableValid(out string connectionString);
        bool AreTablesMissing(string connectionString, out List<string> missingTables);
    }
}
