using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Domain.Entities
{
    public class SqlConnectionModel
    {
        [Required]
        public string Server { get; set; }
        [Required]
        public string Database { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Schema { get; set; }
        [Required]
        public string Catalog { get; set; }
    }


   
}
