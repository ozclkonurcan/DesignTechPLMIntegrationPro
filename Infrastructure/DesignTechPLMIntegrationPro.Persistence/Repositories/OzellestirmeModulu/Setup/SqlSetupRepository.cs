using DesignTechPLMIntegrationPro.Application.Interfaces.OzellestirmeModulu.Setup;
using DesignTechPLMIntegrationPro.Application.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Persistence.Repositories.OzellestirmeModulu.Setup
{
    public class SqlSetupRepository : ISqlSetupService
    {
        private readonly IConfiguration _configuration;
        private readonly QueryScriptService _queryScriptService;
        public SqlSetupRepository(IConfiguration configuration, QueryScriptService queryScriptService)
        {
            _configuration = configuration;
            _queryScriptService = queryScriptService;
        }

        public bool IsConnectionStringValid(out string connectionString)
        {
            connectionString = _configuration.GetConnectionString("Plm");
            return string.IsNullOrEmpty(connectionString);
        }
        public bool IsCreatedTableValid(out string connectionString)
        {
            connectionString = _configuration.GetConnectionString("Plm");
            return !CheckIfTablesExist(connectionString);
        }
        public bool AreTablesMissing(string connectionString, out List<string> missingTables)
        {
            missingTables = new List<string>();
            var jsonData = _queryScriptService.GetScripts();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var script in jsonData)
                {
                    if (DoesObjectExist(connection, script.Name, script.Type)) continue;
                    missingTables.Add(script.Name);
                }
            }

            return missingTables.Any();
        }
        private bool DoesObjectExist(SqlConnection connection, string objectName, string objectType)
        {
            string commandText;
            if (objectType == "Table")
            {
                commandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{objectName}'";
            }
            else if (objectType == "Trigger")
            {
                commandText = $"SELECT COUNT(*) FROM sys.triggers WHERE name = '{objectName}'";
            }
            else
            {
                throw new ArgumentException("Invalid object type specified. Must be 'Table' or 'Trigger'.", nameof(objectType));
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            var count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        bool CheckIfTablesExist(string connectionString)
        {
            var jsonData = _queryScriptService.GetScripts();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var table in jsonData)
                {
                    if (DoesObjectExist(connection, table.Name, table.Type)) continue;
                    return false;
                }
            }
            return true;
        }

    }
}
