using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DesignTechPLMIntegrationPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackgroundController : ControllerBase
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private BackgroundService _backgroundService;
    }
}
