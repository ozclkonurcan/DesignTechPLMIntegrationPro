using DesignTechPLMIntegrationPro.Application.Interfaces.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DesignTechPLMIntegrationPro.Persistence.ApiClients
{
    public class VeriDepo
    {
        public static List<string> JsonVeriListesi { get; set; } = new List<string>();
    }

    public class ApiErrorResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        // Diğer gerekli özellikler
    }


    public class ApiService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;

        public ApiService(ILogService logService)
        {
            _logService = logService;
        }

        private async Task<ApiErrorResponse> DeserializeResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiErrorResponse>(content); // Json.NET kullanılıyor.
        }

        public async Task<string> PostDataAsync(string apiFullUrl, string endpoint, string jsonContent)
        {
            var errorContent = "";
            try
            {


                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    //var request = new HttpRequestMessage(HttpMethod.Post, $"{apiURL}/{endpoint}");
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{apiFullUrl}/{endpoint}");
                    var content = new StringContent(jsonContent.ToString(), Encoding.UTF8, "application/json");
                    request.Content = content;

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await DeserializeResponse(response);
                        VeriDepo.JsonVeriListesi.Add("");

                        if (apiResponse.success == true)
                        {
                            VeriDepo.JsonVeriListesi.Add("");
                            return await response.Content.ReadAsStringAsync();

                        }
                        else
                        {
                            // Başarısız durum işleme alınacak
                            errorContent = $"API başarısız yanıt: {apiResponse.message}";

                            throw new HttpRequestException($"API başarısız yanıt: {apiResponse.message}");
                        }
                    }
                    else
                    {
                        // Günlük için yanıt içeriğini kaydet
                        errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Hata Yanıt İçeriği: {errorContent}");

                        //MessageBox.Show($"HTTP isteği, durum kodu {response.StatusCode} ile başarısız oldu. Hata Mesajı : {errorContent}", "API HATASI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        throw new HttpRequestException($"HTTP isteği, durum kodu {response.StatusCode} ile başarısız oldu");
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                //_logService.Log(LogJsonContent+ ex.Message.ToString() + "Parça gönderilmedi - (API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!)" + apiFullUrl + "/" + endpoint);
                //MessageBox.Show($"API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                throw;
            }
            catch (Exception ex)
            {
                var message = ex is ArgumentException ? ex.Message : " Hata mesajı : " + ex.Message;
                //_logService.Log(LogJsonContent+ "Parça gönderilmedi  - UYGULAMA HATASI - ('" + message + "') - API HATASI ('" + errorContent + "') - " + apiFullUrl + "/" + endpoint);
                //MessageBox.Show($"Hata:  {message} ", "UYGULAMA HATASI", MessageBoxButtons.OK, MessageBoxIcon.Information);

                throw;
            }
        }

        //İnternet bağlantısnın kontrolünü yapıyoruz burada.
        bool IsConnectedToInternet()
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send("www.google.com", 1000);
                return reply.Status == IPStatus.Success;
            }
        }

     




    }
}
