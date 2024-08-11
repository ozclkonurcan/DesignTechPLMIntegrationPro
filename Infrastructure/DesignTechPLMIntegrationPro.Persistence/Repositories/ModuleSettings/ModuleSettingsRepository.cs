using Dapper;
using DesignTechPLMIntegrationPro.Application.Interfaces.ModuleSettings;
using DesignTechPLMIntegrationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Persistence.Repositories.ModuleSettings
{
    public class ModuleSettingsRepository : IModuleSettingsService
    {
        private readonly IDbConnection _dbConnection;

        public ModuleSettingsRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<DesModuleSettings>> GetAllSettingsAsync()
        {
            var sql = "SELECT * FROM PLM1.Des_Module_Settings";
            return await _dbConnection.QueryAsync<DesModuleSettings>(sql);
        }

        public async Task<DesModuleSettings> GetSettingAsync(string settingName)
        {
            var sql = "SELECT * FROM PLM1.Des_Module_Settings WHERE SettingsName = @Name";
            return await _dbConnection.QueryFirstOrDefaultAsync<DesModuleSettings>(sql, new { Name = settingName });
        }

        public async Task<bool> GetSettingValueAsync(string settingName)
        {
            var sql = "SELECT SettingsValue FROM PLM1.Des_Module_Settings WHERE SettingsName = @Name";
            var value = await _dbConnection.QueryFirstOrDefaultAsync<bool>(sql, new { Name = settingName });
            return value;
        }
    }
}
