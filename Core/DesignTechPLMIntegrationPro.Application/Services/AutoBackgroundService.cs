using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.CAD.PDF;
using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.WTPart.State;
using DesignTechPLMIntegrationPro.Application.Interfaces.ModuleSettings;
using DesignTechPLMIntegrationPro.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DesignTechPLMIntegrationPro.Application.Services
{
    public class AutoBackgroundService : BackgroundService
    {
        private readonly ILogger<AutoBackgroundService> _logger;
        private readonly IStateService _stateService;
        private readonly IPdfService _pdfService;
        private readonly IModuleSettingsService _moduleSettingsService;
        private readonly IDbConnection _dbConnection;

        public AutoBackgroundService(ILogger<AutoBackgroundService> logger, IStateService stateService, IPdfService pdfService, IDbConnection dbConnection, IModuleSettingsService moduleSettingsService)
        {
            _logger = logger;
            _stateService = stateService;
            _pdfService = pdfService;
            _dbConnection = dbConnection;
            _moduleSettingsService = moduleSettingsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool isEnabled = await _moduleSettingsService.GetSettingValueAsync("EnableEntegrationModuleProcessing");


                    if(isEnabled)
                    {
                    await Task.WhenAll(
                        _stateService.RELEASED(),
                        _stateService.INWORK(),
                        _stateService.CANCELLED(),
                        _pdfService.SendToCadPDF()


                    );
                    
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing background tasks.");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // İşlemlerin her dakika çalışmasını bekleyebilirsiniz
            }
        }
    }
}
