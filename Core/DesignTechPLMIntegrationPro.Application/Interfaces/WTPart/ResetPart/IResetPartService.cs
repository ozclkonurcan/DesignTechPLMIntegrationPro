﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Interfaces.WTPart.ResetPart
{
    public interface IResetPartService
    {
        Task PartStatusReset();
    }
}
