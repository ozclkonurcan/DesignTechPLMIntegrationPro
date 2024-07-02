using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Interfaces.WTPart.Alternate
{
    public interface IAlternateStateService
    {
        Task AlternateState();

        Task RemovedAlternateState();
    }
}
