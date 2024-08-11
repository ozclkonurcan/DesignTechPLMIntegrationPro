using Dapper;
using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.WTPart.ResetPart;
using DesignTechPLMIntegrationPro.Application.Interfaces.LogModulu.Log;
using DesignTechPLMIntegrationPro.Persistence.ApiClients;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.WTPart.ResetPart
{
    public class ResetPartRepository : IResetPartService
    {


        private const string FilePath = "deneme.json";
        private readonly IDbConnection _dbConnection;
        private readonly ILogService _logService;
        private readonly ApiService _apiService;

        public ResetPartRepository(IDbConnection dbConnection, ILogService logService, ApiService apiService)
        {
            _dbConnection = dbConnection;
            _logService = logService;
            _apiService = apiService;
        }
        public async Task PartStatusReset()
        {
            throw new NotImplementedException();
        }


        private async Task ProcessState(string state, int batchSize = 1000)
        {
            var sql = $"SELECT * FROM PLM1.Des_LogDataTrackingProcess WHERE [statestate] = @State";
            using (var conn = new SqlConnection("Persist Security Info=False;User ID=oozcelik;Password=Onur.!!35;Initial Catalog=PLM1;Server=192.168.1.11;TrustServerCertificate=True"))
            {
                var offset = 0;
                var hasMoreRecords = true;

                while (hasMoreRecords)
                {
                    var batchedSql = $"{sql} ORDER BY PartID OFFSET @Offset ROWS FETCH NEXT @BatchSize ROWS ONLY";
                    var resolvedItems = await conn.QueryAsync<dynamic>(batchedSql, new { State = state, Offset = offset, BatchSize = batchSize });
                    if (resolvedItems.AsList().Count < batchSize)
                    {
                        hasMoreRecords = false;
                    }
                    offset += batchSize;

                    var tasks = new List<Task>();
                    var i = 1;
                    foreach (var partItem in resolvedItems)
                    {
                        var message = "";
                        if (partItem.LogMesaj != null && partItem.LogMesaj.Contains("iliskisi kaldirildi"))
                        {
                            message = $"SIRA [{i}] -- {partItem.Version} -- {state}-{partItem.PartID} + Ana Parça : {partItem.AnaParcaAd} - Muadil Parça {partItem.Number} ile iliskisi kaldirildi. ";
                        }
                        else
                        {
                            message = $"SIRA [{i}] -- {partItem.Version} -- {state}-{partItem.PartID} + Ana Parça : {partItem.AnaParcaAd} - Muadil Parça {partItem.Number} ile ilişkilendirildi. ";
                        }


                        tasks.Add(AppendToJsonFile(message));
                        tasks.Add(_logService.Log(message));
                        await Task.Delay(1000);
                        i++;
                        //                    await conn.ExecuteAsync($@"
                        //DELETE FROM [PLM1].[Des_LogDataTrackingProcess]
                        // WHERE [PartID] = @PartID", new { PartID = partItem.PartID, });
                    }
                    tasks.Add(AppendToJsonFile($"{state} BİTTİ"));
                    await Task.WhenAll(tasks);
                }
            }
        }



        private async Task AppendToJsonFile(string message)
        {
            var logData = new
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(logData, options);

            // Append to the file instead of overwriting it
            using (StreamWriter sw = File.AppendText(FilePath))
            {
                await sw.WriteLineAsync(jsonString);
            }
        }

    }
}
