using Dapper;
using DesignTechPLMIntegrationPro.Application.Interfaces.Log;
using DesignTechPLMIntegrationPro.Application.Interfaces.WTPart.State;
using DesignTechPLMIntegrationPro.Domain.Entities;
using DesignTechPLMIntegrationPro.Persistence.ApiClients;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Persistence.Repositories.WTPart.State
{
    public class StateRepository : IStateService
    {
        private const string FilePath = "deneme.json";
        private readonly IDbConnection _dbConnection;
        private readonly ILogService _logService;
        private readonly ApiService _apiService;

        public StateRepository(IDbConnection dbConnection, ILogService logService, ApiService apiService)
        {
            _dbConnection = dbConnection;
            _logService = logService;
            _apiService = apiService;
        }

        public async Task CANCELLED()
        {
            await ProcessStateCANCELLED("CANCELLED");
        }

        public async Task INWORK()
        {
            await ProcessStateINWORK("INWORK");
        }

        public async Task RELEASED()
        {
            await ProcessStateRELEASED("RELEASED");
        }

        private async Task ProcessStateCANCELLED(string state, int batchSize = 2000)
        {
            try
            {
                while (true)
                {
                    var windchillApiService = new WindchillApiService();
                var sql = "SELECT * FROM PLM1.Des_LogDataTrackingProcess WHERE [statestate] = @State ";

                var offset = 0;
                var hasMoreRecords = true;
                var anaPart = new AnaPart();
                var anaPartCancelled = new AnaPartCancelled();

                using (var conn = new SqlConnection("Persist Security Info=False;User ID=oozcelik;Password=Onur.!!35;Initial Catalog=PLM1;Server=192.168.1.11;TrustServerCertificate=True"))
                {
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
                            var csrfNonce = "BhUIgygXrxGloD5Wd01LynEm/GDp7ExjZUI89Htf+n3j0Ac0Qm965R177EfC5HcmbHx9xwNt3CyfkQlnMCE+tx4ulyGTkAQcR1tQxXskxVLuyA9vQyJex0dNynaX5WsZRVRg8FxzgFzGxBEVNWZa0Hovkg==";
                            var json = await windchillApiService.GetApiData("192.168.1.11", $"ProdMgmt/Parts('OR:wt.part.WTPart:{partItem.PartID}')?$expand=Alternates($expand=AlternatePart)", "PLM-1", "Des.23!Tech", csrfNonce);
                            var response = JsonConvert.DeserializeObject<Part>(json);
                            var JsonData = JsonConvert.SerializeObject(response);
                            var message = $"SIRA [{i}] -- {response.ID} - {response.Number} - {response.State.Value} - {response.EntegrasyonDurumu} - {response.EntegrasyonTarihi}";
                            if (response.State.Value == state)
                            {
                                var jsonDataAPI = "";
                                if (response.State.Value == "CANCELLED")
                                {
                                    response.State.Value = "P";
                                    response.State.Display = "Pasif";
                                    anaPartCancelled = new AnaPartCancelled
                                    {
                                        Number = response.Number,
                                        State = response.State,

                                    };
                                    jsonDataAPI = JsonConvert.SerializeObject(anaPartCancelled);
                                }


                                await _apiService.PostDataAsync("http://localhost:7217/api/PART", "CANCELLED", jsonDataAPI);


                                if (response.State.Value == "P")
                                {
                                    response.State.Value = "CANCELLED";
                                    response.State.Display = "Cancelled";
                                }


                                await conn.ExecuteAsync(
                                "INSERT INTO [PLM1].[Change_Notice_LogTable] ([TransferID],[idA2A2], [ProcessTimestamp], [updateStampA2],[statestate], [name], [WTPartNumber],[Version],[VersionID]) " +
                                "VALUES (@TransferID, @idA2A2, @ProcessTimestamp, @updateStampA2, @statestate, @name, @WTPartNumber, @Version, @VersionID)",
                                new
                                {
                                    TransferID = response.TransferID,
                                    idA2A2 = response.ID.Split(':')[2],
                                    ProcessTimestamp = DateTime.UtcNow,
                                    updateStampA2 = response.LastModified,
                                    statestate = response.State.Value,
                                    name = response.Name,
                                    WTPartNumber = response.Number,
                                    Version = response.Version,
                                    VersionID = response.VersionID
                                });

                                tasks.Add(AppendToJsonFile(message));
                                tasks.Add(_logService.Log(message));

                                    await conn.ExecuteAsync($@"
                        DELETE FROM [PLM1].[Des_LogDataTrackingProcess]
                         WHERE [PartID] = @PartID", new { PartID = partItem.PartID, });
                                }
                            }
                            await Task.Delay(1000);
                            i++;
                   
                        tasks.Add(AppendToJsonFile($"{state} BİTTİ"));
                        await Task.WhenAll(tasks);
                    }
                }
            }
            }
            catch (Exception)
            {

            }
        }



        private async Task ProcessStateINWORK(string state, int batchSize = 2000)
        {
            try
            {
                while (true) { 
                var windchillApiService = new WindchillApiService();
                var sql = "SELECT * FROM PLM1.Des_LogDataTrackingProcess WHERE [statestate] = @State AND [Version] NOT LIKE 'A%' ";

                var offset = 0;
                var hasMoreRecords = true;
                var anaPart = new AnaPart();
                var anaPartCancelled = new AnaPartCancelled();

                using (var conn = new SqlConnection("Persist Security Info=False;User ID=oozcelik;Password=Onur.!!35;Initial Catalog=PLM1;Server=192.168.1.11;TrustServerCertificate=True"))
                {
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
                            var csrfNonce = "BhUIgygXrxGloD5Wd01LynEm/GDp7ExjZUI89Htf+n3j0Ac0Qm965R177EfC5HcmbHx9xwNt3CyfkQlnMCE+tx4ulyGTkAQcR1tQxXskxVLuyA9vQyJex0dNynaX5WsZRVRg8FxzgFzGxBEVNWZa0Hovkg==";
                            var json = await windchillApiService.GetApiData("192.168.1.11", $"ProdMgmt/Parts('OR:wt.part.WTPart:{partItem.PartID}')?$expand=Alternates($expand=AlternatePart)", "PLM-1", "Des.23!Tech", csrfNonce);
                            var response = JsonConvert.DeserializeObject<Part>(json);
                            var message = $"SIRA [{i}] -- {response.ID} - {response.Number} - {response.State.Value} - {response.EntegrasyonDurumu} - {response.EntegrasyonTarihi}";

                            if (response.State.Value == state && !response.Version.StartsWith('A'))
                            {


                                await conn.ExecuteAsync(
                                "INSERT INTO [PLM1].[Change_Notice_LogTable] ([TransferID],[idA2A2], [ProcessTimestamp], [updateStampA2],[statestate], [name], [WTPartNumber],[Version],[VersionID]) " +
                                "VALUES (@TransferID, @idA2A2, @ProcessTimestamp, @updateStampA2, @statestate, @name, @WTPartNumber, @Version, @VersionID)",
                                new
                                {
                                    TransferID = response.TransferID,
                                    idA2A2 = response.ID.Split(':')[2],
                                    ProcessTimestamp = DateTime.UtcNow,
                                    updateStampA2 = response.LastModified,
                                    statestate = response.State.Value,
                                    name = response.Name,
                                    WTPartNumber = response.Number,
                                    Version = response.Version,
                                    VersionID = response.VersionID
                                });

                                tasks.Add(AppendToJsonFile(message));
                                tasks.Add(_logService.Log(message));
                                    await conn.ExecuteAsync($@"
                        DELETE FROM [PLM1].[Des_LogDataTrackingProcess]
                         WHERE [PartID] = @PartID", new { PartID = partItem.PartID, });
                                }
                            await Task.Delay(1000);
                            i++;
                    
                        }
                        tasks.Add(AppendToJsonFile($"{state} BİTTİ"));
                        await Task.WhenAll(tasks);
                    }
                }
                }

            }
            catch (Exception)
            {

            }
        }



        private async Task ProcessStateRELEASED(string state, int batchSize = 2000)
        {
            try
            {
                while (true)
                {
                    var windchillApiService = new WindchillApiService();
                var sql = "SELECT * FROM PLM1.Des_LogDataTrackingProcess WHERE [statestate] = @State ";

                var offset = 0;
                var hasMoreRecords = true;
                var anaPart = new AnaPart();
                var anaPartCancelled = new AnaPartCancelled();

                using (var conn = new SqlConnection("Persist Security Info=False;User ID=oozcelik;Password=Onur.!!35;Initial Catalog=PLM1;Server=192.168.1.11;TrustServerCertificate=True"))
                {
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
                            var csrfNonce = "BhUIgygXrxGloD5Wd01LynEm/GDp7ExjZUI89Htf+n3j0Ac0Qm965R177EfC5HcmbHx9xwNt3CyfkQlnMCE+tx4ulyGTkAQcR1tQxXskxVLuyA9vQyJex0dNynaX5WsZRVRg8FxzgFzGxBEVNWZa0Hovkg==";
                            var json = await windchillApiService.GetApiData("192.168.1.11", $"ProdMgmt/Parts('OR:wt.part.WTPart:{partItem.PartID}')?$expand=Alternates($expand=AlternatePart)", "PLM-1", "Des.23!Tech", csrfNonce);
                            var response = JsonConvert.DeserializeObject<Part>(json);
                            var JsonData = JsonConvert.SerializeObject(response);

                            var message = $"SIRA [{i}] -- {response.ID} - {response.Number} - {response.State.Value} - {response.EntegrasyonDurumu} - {response.EntegrasyonTarihi}";

                            if (response.State.Value == state)
                            {
                                var jsonDataAPI = "";

                                if (response.State.Value == "RELEASED")
                                {
                                    response.State.Value = "A";
                                    response.State.Display = "Aktif";
                                    anaPart = new AnaPart
                                    {
                                        Number = response.Number,
                                        Name = response.Name,
                                        Fai = "H",
                                        MuhasebeKodu = "0000000",
                                        PlanlamaTipiKodu = "P",
                                        PLM = "E",
                                        State = response.State,
                                        TransferID = response.TransferID,
                                        Description = response.Description,
                                        BirimKodu = response.BirimKodu,
                                        CLASSIFICATION = response.CLASSIFICATION
                                    };
                                    jsonDataAPI = JsonConvert.SerializeObject(anaPart);
                                }


                                await _apiService.PostDataAsync("http://localhost:7217/api/PART", "RELEASED_PART", jsonDataAPI);


                                if (response.State.Value == "A")
                                {
                                    response.State.Value = "RELEASED";
                                    response.State.Display = "Released";
                                }
                          

                                await conn.ExecuteAsync(
                                "INSERT INTO [PLM1].[Change_Notice_LogTable] ([TransferID],[idA2A2], [ProcessTimestamp], [updateStampA2],[statestate], [name], [WTPartNumber],[Version],[VersionID]) " +
                                "VALUES (@TransferID, @idA2A2, @ProcessTimestamp, @updateStampA2, @statestate, @name, @WTPartNumber, @Version, @VersionID)",
                                new
                                {
                                    TransferID = response.TransferID,
                                    idA2A2 = response.ID.Split(':')[2],
                                    ProcessTimestamp = DateTime.UtcNow,
                                    updateStampA2 = response.LastModified,
                                    statestate = response.State.Value,
                                    name = response.Name,
                                    WTPartNumber = response.Number,
                                    Version = response.Version,
                                    VersionID = response.VersionID
                                });

                                tasks.Add(AppendToJsonFile(message));
                                tasks.Add(_logService.Log(message));
                                    await conn.ExecuteAsync($@"
                        DELETE FROM [PLM1].[Des_LogDataTrackingProcess]
                         WHERE [PartID] = @PartID", new { PartID = partItem.PartID, });
                                }
                            await Task.Delay(1000);
                            i++;
                  
                        }
                        tasks.Add(AppendToJsonFile($"{state} BİTTİ"));
                        await Task.WhenAll(tasks);
                    }
                }
            }
            }
            catch (Exception)
            {
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

            string jsonString = System.Text.Json.JsonSerializer.Serialize(logData, options);

            using (StreamWriter sw = File.AppendText(FilePath))
            {
                await sw.WriteLineAsync(jsonString);
            }
        }
    }
}
