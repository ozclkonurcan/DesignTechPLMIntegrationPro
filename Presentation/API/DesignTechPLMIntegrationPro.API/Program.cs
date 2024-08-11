using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.CAD.PDF;
using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.WTPart.Alternate;
using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.WTPart.State;
using DesignTechPLMIntegrationPro.Application.Interfaces.LogModulu.Log;
using DesignTechPLMIntegrationPro.Application.Interfaces.ModuleSettings;
using DesignTechPLMIntegrationPro.Application.Interfaces.OzellestirmeModulu.Setup;
using DesignTechPLMIntegrationPro.Application.Services;
using DesignTechPLMIntegrationPro.Persistence.ApiClients;
using DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.CAD.PDF;
using DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.WTPart.Alternate;
using DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.WTPart.State;
using DesignTechPLMIntegrationPro.Persistence.Repositories.LogModulu.Log;
using DesignTechPLMIntegrationPro.Persistence.Repositories.ModuleSettings;
using DesignTechPLMIntegrationPro.Persistence.Repositories.OzellestirmeModulu.Setup;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using PdfSharp.Charting;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<AutoBackgroundService>();

builder.Services.AddTransient<IDbConnection>(sp =>
          new SqlConnection("Persist Security Info=False;User ID=oozcelik;Password=Onur.!!35;Initial Catalog=PLM1;Server=192.168.1.11;TrustServerCertificate=True"));
// Diðer servisleriniz burada eklenir

builder.Services.AddTransient<IStateService, StateRepository>();
builder.Services.AddTransient<IAlternateStateService, AlternateStateRepository>();
builder.Services.AddTransient<IModuleSettingsService, ModuleSettingsRepository>();

// LogService ve PdfService hizmetlerini ekleyin
builder.Services.AddSingleton<ILogService, LogRepository>();
builder.Services.AddSingleton<IPdfService, PdfRepository>();
builder.Services.AddSingleton<ApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
