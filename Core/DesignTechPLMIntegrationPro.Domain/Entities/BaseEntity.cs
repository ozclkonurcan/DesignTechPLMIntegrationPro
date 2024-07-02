using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Domain.Entities
{
    public abstract class BaseEntity
    {

        public BaseEntity()
        {
            TransferID = Guid.NewGuid().ToString();
        }

        public string TransferID { get; set; }

  
    }
}
