using DesignTechPLMIntegrationPro.Application.Interfaces.OzellestirmeModulu.Setup;
using DesignTechPLMIntegrationPro.Application.Services;
using DesignTechPLMIntegrationPro.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Security.AccessControl;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace DesignTechPLMIntegrationPro.MVC.Controllers
{
    public class SetupController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISqlSetupService _sqlSetupService;
        private readonly QueryScriptService _queryScriptService;
		public SetupController(IConfiguration configuration, ISqlSetupService sqlSetupService, QueryScriptService queryScriptService)
		{
			_configuration = configuration;
			_sqlSetupService = sqlSetupService;
			_queryScriptService = queryScriptService;
		}

		[HttpGet]
        public IActionResult Index()
        {
         if (_sqlSetupService.IsConnectionStringValid(out var connectionString))
        {
            return View(); // SQL ayarlarının yapılmadığı sayfa
        }
        return RedirectToAction("Install", "Setup"); // Tabloların kurulduğu sayfa
        }

        [HttpPost]
        public IActionResult Index(SqlConnectionModel model)
        {
            if (ModelState.IsValid)
            {
                // Bağlantı testini yapın
                if (TestSqlConnection(model))
                {
                    // Bağlantı bilgilerini kaydedin (appsettings.json'a veya başka bir yere)
                    SaveConnectionSettings(model);

                    // Kurulum sayfasına yönlendirin
                    return RedirectToAction("Install");
                }
                else
                {
                    ModelState.AddModelError("", "SQL bağlantısı başarısız. Lütfen bilgilerinizi kontrol edin.");
                    TempData["ErrorMessage"] = "SQL bağlantısı başarısız. Lütfen bilgilerinizi kontrol edin.";
                }
            }
            return View(model);
        }

        private bool TestSqlConnection(SqlConnectionModel model)
        {
            var connectionString = $"Persist Security Info=False;User ID={model.UserId};Password={model.Password};Initial Catalog={model.Database};Server={model.Server};TrustServerCertificate=True";


            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void SaveConnectionSettings(SqlConnectionModel model)
        {
            var connectionString =
                $"Persist Security Info=False;User ID={model.UserId};" +
                $"Password={model.Password};Initial Catalog={model.Catalog};" +
                $"Server={model.Server};TrustServerCertificate=True";
            // Configuration'ı kullanarak bağlantı bilgilerini kaydedin.
            _configuration["Catalog"] = model.Catalog;
            _configuration["DatabaseSchema"] = model.Schema;
            _configuration["ConnectionStrings:Plm"] = connectionString;

            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var appSettingsText = await System.IO.File.ReadAllTextAsync(path);
            JObject appSettingsJson = JObject.Parse(appSettingsText);
            appSettingsJson["Catalog"] = model.Catalog;

            // ConnectionStrings bölümünü kontrol et ve eğer yoksa oluştur
            if (appSettingsJson["ConnectionStrings"] == null)
            {
                appSettingsJson["ConnectionStrings"] = new JObject();
            }


            if (appSettingsJson["connectionType"] == null)
            {
                appSettingsJson["connectionType"] = true; // Bağlantı başarılı
            }
            // Bağlantı dizesini appsettings.json dosyasına kaydet
            appSettingsJson["ConnectionStrings"]["Plm"] = connectionString;

            // "Catalog" adında bir değişken ekleyin veya güncelleyin
            appSettingsJson["Catalog"] = model.Catalog;
            appSettingsJson["DatabaseSchema"] = model.Schema;

            // Yeni ayarları JSON olarak dönüştür
            string newAppSettingsText = appSettingsJson.ToString(Newtonsoft.Json.Formatting.Indented);

            // Dosyayı güncelle
            await System.IO.File.WriteAllTextAsync(path, newAppSettingsText);


            //TempData["SuccessMessage"] = "Bağlantı başarılı.";
        }
        [HttpGet]
        public IActionResult Install()
        {


			//var connectionString = _configuration.GetConnectionString("Plm");
			if (_sqlSetupService.IsCreatedTableValid(out var connectionString))
            {
                var missingTables = GetMissingTables();

                return View(missingTables);
            }
            return RedirectToAction("Index", "Ldap");
      

        }

        [HttpPost]
        public async Task<IActionResult> Install(string[] tablesToInstall)
        {

            // Tablolar kuruluyorsa Install eylemi çağır
            if (tablesToInstall != null && tablesToInstall.Length > 0)
            {
                var tableInfos = new List<TableInfo>();
                foreach (var table in tablesToInstall)
                {
                    // InstallTable eylemini doğrudan çağırıyoruz
                    await InstallTable(table);
                    tableInfos.Add(new TableInfo(table, TableStatus.Installed,""));
                }
                return Json(tableInfos);
            }

            // Eğer tablolar kurulmamış ise Install eylemi çağır
            return View("Install", GetMissingTables());
        }

        [HttpPost]
        public async Task<IActionResult> InstallTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return BadRequest("Tablo adı boş veya null olamaz.");
            }

			var jsonData = _queryScriptService.GetScripts();

			var respData = jsonData.Where(x => x.Name == tableName).SingleOrDefault();

			var connectionString = _configuration.GetConnectionString("Plm");





            using (var connection = new SqlConnection(connectionString))
            {
                var commandText = "";
				if (respData.Type == "Table")
				{
					commandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{respData.Name}'";
				}
				else if (respData.Type == "Trigger")
				{
					commandText = $"SELECT COUNT(*) FROM sys.triggers WHERE name = '{respData.Name}'";
				}
				else
				{
					throw new ArgumentException("Invalid object type specified. Must be 'Table' or 'Trigger'.", nameof(respData.Type));
				}

	


				connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = commandText;
                var count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    // Tablo zaten kurulu, başarılı yanıt dön
                    return Json(new { status = "Kurulu", tableName = tableName });
                }
                else
                {
                    await CreateTableAsync(connection, tableName);
                    await Task.Delay(2000); // Kurulum işlemi simülasyonu için

                    return Json(new { status = "Kuruldu", tableName = tableName });
                }
            }
        }

        private async Task CreateTableAsync(SqlConnection connection, string tableName)
        {
            try
            {
				// Burada tabloları oluşturma işlemini yapın
				// Örneğin:
				var jsonData = _queryScriptService.GetScripts();

				var respData = jsonData.Where(x => x.Name == tableName).SingleOrDefault();

				respData.Script = respData.Script.Replace("{schema}", _configuration["DatabaseSchema"]);

				if (respData.Type == "Table")
				{
					var cmd = connection.CreateCommand();
					cmd.CommandText = respData.Script;
					await cmd.ExecuteNonQueryAsync();
				}


				if (respData.Type == "Trigger")
				{
					var cmd = connection.CreateCommand();
					cmd.CommandText = respData.Script;
					await cmd.ExecuteNonQueryAsync();
				}
			}
            catch (Exception ex)
            {

                throw;
            }

		
        }



        private List<TableInfo> GetMissingTables()
        {
            // Bağlantı dizesini alın
            var connectionString = _configuration.GetConnectionString("Plm");

			var jsonData = _queryScriptService.GetScripts();

			var tableInfos = new List<TableInfo>();
            foreach (var table in jsonData)
            {
                var tableStatus = GetObjectStatus(table.Name,table.Type);
                tableInfos.Add(new TableInfo(table.Name, tableStatus,table.Type));
            }


            return tableInfos;
        }



		private TableStatus GetObjectStatus(string objectName, string objectType)
		{
			var connectionString = _configuration.GetConnectionString("Plm");

			using var connection = new SqlConnection(connectionString);
			connection.Open();

			var cmd = connection.CreateCommand();

			if (objectType == "Table")
			{
				cmd.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{objectName}'";
			}
			else if (objectType == "Trigger")
			{
				cmd.CommandText = $"SELECT COUNT(*) FROM sys.triggers WHERE name = '{objectName}'";
			}
			else
			{
				throw new ArgumentException("Invalid object type specified. Must be 'Table' or 'Trigger'.", nameof(objectType));
			}

			var count = (int)cmd.ExecuteScalar();
			return count == 0 ? TableStatus.NotInstalled : TableStatus.Installed;
		}





		public async Task CreateTable(string table)
        {
            // Bağlantı dizesini alın
            var connectionString = _configuration.GetConnectionString("Plm");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                // Tablo oluşturma SQL komutunu buraya ekleyin
                cmd.CommandText = $"CREATE TABLE {table} (id INT PRIMARY KEY)"; // Örnek
                cmd.ExecuteNonQuery();
            }
        }





        bool CheckIfTablesExist(string connectionString)
        {
            // Tablo varlığını kontrol etmek için kodu buraya ekleyin
            // Örnek olarak, `Table1` ve `Table2` tablolarını kontrol ediyoruz
            var tables = new[] { "Table1", "Table2","Table3" };
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var table in tables)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}'";
                    var count = (int)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }




    }
}

// TableInfo sınıfı
public class TableInfo
{
    public string TableName { get; set; }
    public TableStatus Status { get; set; }
    public string Type { get; set; }

	public TableInfo(string tableName, TableStatus status, string type)
	{
		TableName = tableName;
		Status = status;
		Type = type;
	}
}

// Durum enum'ı
public enum TableStatus
{
    NotInstalled,
    Installing,
    Installed,
    Error,
    Updating
}

