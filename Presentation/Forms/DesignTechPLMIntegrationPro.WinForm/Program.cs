using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.CAD.PDF;
using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.WTPart.Alternate;
using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.WTPart.State;
using DesignTechPLMIntegrationPro.Application.Interfaces.LogModulu.Log;
using DesignTechPLMIntegrationPro.Persistence.ApiClients;
using DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.CAD.PDF;
using DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.WTPart.Alternate;
using DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.WTPart.State;
using DesignTechPLMIntegrationPro.Persistence.Repositories.LogModulu.Log;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace DesignTechPLMIntegrationPro.WinForm
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var serviceProvider = ConfigureServices();

            // Ana Formu oluþturma ve DI Container'ý verme
            System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // DI konteynerini kullanarak ana formu oluþtur
            var mainForm = serviceProvider.GetRequiredService<HomePage>();
            System.Windows.Forms.Application.Run(mainForm);
            //System.Windows.Forms.Application.Run(new HomePage(serviceProvider.GetRequiredService<ILogService>()));
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Baðlantý dizesini kullanarak IDbConnection hizmetini ekleyin
            services.AddTransient<IDbConnection>(sp =>
                new SqlConnection("Persist Security Info=False;User ID=oozcelik;Password=Onur.!!35;Initial Catalog=PLM1;Server=192.168.1.11;TrustServerCertificate=True"));

            // StateRepository hizmetini ekleyin
            services.AddTransient<IStateService, StateRepository>();
            services.AddTransient<IAlternateStateService, AlternateStateRepository>();

            // LogService ve PdfService hizmetlerini ekleyin
            services.AddSingleton<ILogService, LogRepository>();
            services.AddSingleton<IPdfService, PdfRepository>();
            services.AddSingleton<ApiService>();
            // Ana formu ekleyin
            services.AddTransient<HomePage>();

            return services.BuildServiceProvider();
        }
    }
}