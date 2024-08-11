using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Domain.Entities
{
    public class DesModuleSettings
    {
        [Key]
        public int ModuleSettingsID { get; set; }
        public string SettingsName { get; set; }
        public bool SettingsValue { get; set; }
    }
}
