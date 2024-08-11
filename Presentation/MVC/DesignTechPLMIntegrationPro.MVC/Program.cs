using DesignTechPLMIntegrationPro.Application.Interfaces.OzellestirmeModulu.Setup;
using DesignTechPLMIntegrationPro.Application.Services;
using DesignTechPLMIntegrationPro.MVC.Controllers;
using DesignTechPLMIntegrationPro.Persistence.Repositories.OzellestirmeModulu.Setup;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);



// Configuration'� olu�turun
builder.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build());

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ISqlSetupService, SqlSetupRepository>();
builder.Services.AddScoped<LdapService>();
builder.Services.AddScoped<QueryScriptService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Ldap/Index"; // Giri� sayfas�na y�nlendirme
        options.AccessDeniedPath = "/Ldap/Index"; // Eri�im reddedildi�inde y�nlendirme
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect(context.RedirectUri);


            //TempData["ErrorMessage"] = "Bu sayfaya eri�mek i�in giri� yapman�z gerekiyor.";
            return Task.CompletedTask;
        };
    });
var app = builder.Build();



//// Ba�lant� ayarlar� kontrol�
//var configuration = app.Services.GetRequiredService<IConfiguration>();
//var connectionString = configuration.GetConnectionString("Plm");
//if (string.IsNullOrEmpty(connectionString))
//{
//    app.Use(async (context, next) =>
//    {
//        if (!context.Request.Path.StartsWithSegments("/Setup"))
//        {
//            context.Response.Redirect("/Setup/Index");
//            return;
//        }
//        await next.Invoke();
//    });
//}
//else
//{
//    //Tablo varl���n� kontrol edin
//    if (!CheckIfTablesExist(connectionString))
//    {
//        app.Use(async (context, next) =>
//        {
//            if (!context.Request.Path.StartsWithSegments("/Setup/Install"))
//            {
//                context.Response.Redirect("/Setup/Install");
//                return;
//            }
//            await next.Invoke();
//        });
//    }
//    else
//    {
//        app.Use(async (context, next) =>
//        {
//            if (!context.Request.Path.StartsWithSegments("/Ldap/Index"))
//            {
//                context.Response.Redirect("/Ldap/Index");
//                return;
//            }
//            await next.Invoke();
//        });
//    }
//}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Setup}/{action=Index}/{id?}");

app.Run();




//bool CheckIfTablesExist(string connectionString)
//{
//    // Tablo varl���n� kontrol etmek i�in kodu buraya ekleyin
//    // �rnek olarak, `Table1` ve `Table2` tablolar�n� kontrol ediyoruz
//    var tables = new[] { "Table1", "Table2" };
//    using (var connection = new SqlConnection(connectionString))
//    {
//        connection.Open();
//        foreach (var table in tables)
//        {
//            var cmd = connection.CreateCommand();
//            cmd.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}'";
//            var count = (int)cmd.ExecuteScalar();
//            if (count == 0)
//            {
//                return false;
//            }
//        }
//    }
//    return true;
//}