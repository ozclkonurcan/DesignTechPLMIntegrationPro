using DesignTechPLMIntegrationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Interfaces.ModuleSettings
{
    public interface IModuleSettingsService
    {
        Task<bool> GetSettingValueAsync(string settingName);
        Task<DesModuleSettings> GetSettingAsync(string settingName);
        Task<IEnumerable<DesModuleSettings>> GetAllSettingsAsync();
    }
}
