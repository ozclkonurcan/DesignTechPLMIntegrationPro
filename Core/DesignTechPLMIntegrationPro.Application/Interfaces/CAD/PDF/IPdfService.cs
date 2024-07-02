using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Interfaces.CAD.PDF
{
    public interface IPdfService
    {
        Task SendToCadPDF();
    }
}
