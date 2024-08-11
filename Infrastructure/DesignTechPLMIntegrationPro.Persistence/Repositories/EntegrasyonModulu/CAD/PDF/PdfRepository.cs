using Dapper;
using DesignTechPLMIntegrationPro.Application.Interfaces.EntegrasyonModulu.CAD.PDF;
using DesignTechPLMIntegrationPro.Application.Interfaces.LogModulu.Log;
using DesignTechPLMIntegrationPro.Domain.Entities;
using DesignTechPLMIntegrationPro.Persistence.ApiClients;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace DesignTechPLMIntegrationPro.Persistence.Repositories.EntegrasyonModulu.CAD.PDF
{
    public class PdfRepository : IPdfService
    {

        private readonly string filePath = "cadDokuman.json";
        private readonly IDbConnection conn;
        private readonly ILogService _logService;
        private readonly ApiService _apiService;
        public PdfRepository(IDbConnection db, ILogService logService, ApiService apiService)
        {
            conn = db;
            _logService = logService;
            _apiService = apiService;
        }
        public async Task SendToCadPDF()
        {
            await ProcessState("RELEASED");
        }


        private async Task ProcessState(string state, int batchSize = 1000)
        {
            try
            {
                while (true)
                {
                    var windchillApiService = new WindchillApiService();
                    var sql = $"SELECT * FROM PLM1.Ent_EPMDocState WHERE [StateDegeri] = @State";
                    using (var conn = new SqlConnection("Persist Security Info=False;User ID=oozcelik;Password=Onur.!!35;Initial Catalog=PLM1;Server=192.168.1.11;TrustServerCertificate=True"))
                    {

                        var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { State = state });




                        if (resolvedItems.Count() != 0)
                        {

                            var tasks = new List<Task>();
                            var i = 1;
                            foreach (var partItem in resolvedItems)
                            {
                                var csrfNonce = "BhUIgygXrxGloD5Wd01LynEm/GDp7ExjZUI89Htf+n3j0Ac0Qm965R177EfC5HcmbHx9xwNt3CyfkQlnMCE+tx4ulyGTkAQcR1tQxXskxVLuyA9vQyJex0dNynaX5WsZRVRg8FxzgFzGxBEVNWZa0Hovkg==";
                                var cadAssociationsJSON = await windchillApiService.GetApiData("192.168.1.11", $"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')/PartDocAssociations", "PLM-1", "Des.23!Tech", csrfNonce);
                                var cadJSON = await windchillApiService.GetApiData("192.168.1.11", $"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')?$expand=Representations", "PLM-1", "Des.23!Tech", csrfNonce);


                                var CADResponse = JsonConvert.DeserializeObject<RootObject>(cadJSON);
                                string partCode = "";
                                if (cadAssociationsJSON != null)
                                {
                                    var CADAssociationsResponse = JsonConvert.DeserializeObject<CADDocumentResponse>(cadAssociationsJSON);
                                    if (CADAssociationsResponse != null && CADAssociationsResponse.Value != null && CADAssociationsResponse.Value.Count > 0)
                                    {
                                        var firstAssociation = CADAssociationsResponse.Value.SingleOrDefault();
                                        if (firstAssociation != null && firstAssociation.ID != null)
                                        {

                                            var CADAssociations = CADAssociationsResponse.Value.SingleOrDefault().ID;
                                            string pattern = @"OR:wt\.part\.WTPart:(\d+)_Calculated_OR:wt\.epm\.EPMDocument:";
                                            Regex regex = new Regex(pattern);
                                            Match match = regex.Match(CADAssociations);

                                            if (match.Success)
                                            {
                                                partCode = match.Groups[1].Value;

                                            }
                                        }

                                    }

                                }

                                try
                                {

                                    if (cadJSON != null || cadJSON != "")
                                    {
                                        foreach (var representation in CADResponse.Representations)
                                        {


                                            if (representation != null)
                                            {
                                                if (representation.AdditionalFiles != null && representation.AdditionalFiles.Count > 0)
                                                {

                                                    foreach (var item in representation.AdditionalFiles.Where(x => x.FileName.Contains(".pdf") || x.FileName.Contains(".PDF") || x.Format == "PDF"))
                                                    {

                                                        if (item != null)
                                                        {
                                                            if (item.FileName.Contains(".pdf") || item.FileName.Contains(".PDF") || item.Format == "PDF")
                                                            {
                                                                var pdfUrl = item.URL;
                                                                var pdfFileName = item.Label;
                                                                await SendPdfToCustomerFunctionAsync(pdfUrl, pdfFileName, "http://localhost:7217/api/CAD", "http://localhost:7217/api/CAD", "SENDFILE", partItem.EPMDocID, "PLM1", conn, CADResponse, "SEND_FILE", partCode);

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }


                                    }

                                }
                                catch (Exception)
                                {

                                }




                                //var message = $"SIRA [{i}] -- {partItem.EPMDocID} --  {partItem.StateDegeri}  CAD Parca gonderildi ";

                                //tasks.Add(AppendToJsonFile(message));
                                //tasks.Add(_logService.Log(message));


                                ////await Task.Delay(1000);
                                //i++;
                                //await conn.ExecuteAsync($@"
                                //DELETE FROM [PLM1].[Ent_EPMDocState]
                                // WHERE [EPMDocID] = @EPMDocID", new { EPMDocID = partItem.EPMDocID, });
                            }
                            //tasks.Add(AppendToJsonFile($"{state} BİTTİ"));
                            //await Task.WhenAll(tasks);
                        }
                    }
                }


            }
            catch (Exception)
            {
            }

        }



        private async Task SendPdfToCustomerFunctionAsync(string pdfUrl, string pdfFileName, string apiFullUrl, string apiURL, string endPoint, long EPMDocID, string catalogValue, SqlConnection conn, RootObject CADResponse, string stateType, string partCode)
        {
            try
            {
                var tasks = new List<Task>();
                if (stateType == "SEND_FILE")
                {

                    var partName = "";
                    var partNumber = "";

                    if (!string.IsNullOrEmpty(partCode))
                    {

                        var SQL_WTPart = $"SELECT [idA3masterReference] FROM {catalogValue}.WTPart WHERE [idA2A2] = '{partCode}'";
                        var resolvedItems_SQL_WTPart = await conn.QuerySingleAsync<dynamic>(SQL_WTPart);
                        var SQL_WTPartMaster = $"SELECT [name],[WTPartNumber] FROM {catalogValue}.WTPartMaster WHERE [idA2A2] = '{resolvedItems_SQL_WTPart.idA3masterReference}'";
                        var resolvedItems_SQL_WTPartMaster = await conn.QuerySingleAsync<dynamic>(SQL_WTPartMaster);


                        partName = resolvedItems_SQL_WTPartMaster.name;
                        partNumber = resolvedItems_SQL_WTPartMaster.WTPartNumber;
                    }





                    var CADViewResponseContentInfo = new TeknikResimViewModel
                    {
                        Number = CADResponse.Number,
                        Revizyon = CADResponse.Revision,
                        DocumentType = "TR",
                        Description = CADResponse.Description,
                        ModifiedOn = CADResponse.LastModified,
                        AuthorizationDate = "",
                        ModifiedBy = CADResponse.ModifiedBy,
                        State = CADResponse.State,
                        cADContent = new CADContent
                        {
                            FileData = await DownloadPdfAsync(pdfUrl),
                            Name = pdfFileName
                        },
                        dAD_WTPART_Ilişki = new CAD_WTPART_Ilişki
                        {

                            RelatedPartName = partName,
                            RelatedPartNumber = partNumber
                        }

                    };
                    CADViewResponseContentInfo.State.Value = "50";


                    var LogJsonData = JsonConvert.SerializeObject(CADViewResponseContentInfo);
                    await _apiService.PostDataAsync(apiFullUrl, endPoint, LogJsonData);

                    await _logService.Log(LogJsonData + "CAD Döküman bilgileri gönderildi.");

                    var message = $"SIRA [{partNumber}] -- {EPMDocID} --  {pdfFileName}  CAD Parca gonderildi ";

                    tasks.Add(AppendToJsonFile(message));
                    tasks.Add(_logService.Log(message));


                    await conn.ExecuteAsync($@"
                                DELETE FROM [PLM1].[Ent_EPMDocState]
                                 WHERE [EPMDocID] = @EPMDocID", new { EPMDocID, });
                    await Task.WhenAll(tasks);

                }



                try
                {


                    await conn.ExecuteAsync($@"
        DELETE FROM [{catalogValue}].[Ent_EPMDocState]
        WHERE EPMDocID = @Ids", new { Ids = EPMDocID });

                }
                catch (Exception ex)
                {
                    //Hata mesajını veya hata günlüğünü kaydedin
                    Console.WriteLine($"Hata: {ex.Message}");
                    var CADViewResponseContentInfo = new TeknikResimViewModel
                    {
                        Number = CADResponse.Number,
                        Revizyon = CADResponse.Revision,
                        DocumentType = "TR",
                        Description = CADResponse.Description,
                        ModifiedOn = CADResponse.LastModified,
                        AuthorizationDate = "",
                        ModifiedBy = CADResponse.ModifiedBy,
                        State = CADResponse.State,
                        cADContent = new CADContent
                        {
                            FileData = await DownloadPdfAsync(pdfUrl),
                            Name = pdfFileName
                        }

                    };
                    CADViewResponseContentInfo.State.Value = "50";
                    var LogJsonData = JsonConvert.SerializeObject(CADViewResponseContentInfo);


                }
                finally
                {
                    // Bağlantıyı kapatın
                    conn.Close();
                }

            }
            catch (Exception)
            {
            }
        }

        private async Task<byte[]> DownloadPdfAsync(string pdfUrl)
        {
            try
            {

                string directoryPath = "Configuration";
                string fileName2 = "appsettings.json";
                string fileName3 = "scanpdf-425313-50117e72a809.json";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName2);
                string filePathPDFScan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName3);
                byte[] pdfBytes;



                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
                }



                // (Önceki kodlar burada)
                //string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

                //JObject jsonObject = JObject.Parse(jsonData);

                var csrfNonce = "BhUIgygXrxGloD5Wd01LynEm/GDp7ExjZUI89Htf+n3j0Ac0Qm965R177EfC5HcmbHx9xwNt3CyfkQlnMCE+tx4ulyGTkAQcR1tQxXskxVLuyA9vQyJex0dNynaX5WsZRVRg8FxzgFzGxBEVNWZa0Hovkg==";


                var _httpClient1 = new HttpClient();
                _httpClient1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"PLM-1:Des.23!Tech")));

                _httpClient1.DefaultRequestHeaders.Add("CSRF-NONCE", csrfNonce);
                using (var response = await _httpClient1.GetAsync(pdfUrl))
                {


                    if (response.IsSuccessStatusCode)
                    {
                        var dosyaAdi = Path.GetFileName(new Uri(pdfUrl).LocalPath);

                        // PDF dosyasını belirtilen dizine kaydet
                        string saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "PDF");
                        string savePath = Path.Combine(saveDirectory, dosyaAdi);

                        // Klasör yoksa oluştur
                        if (!Directory.Exists(saveDirectory))
                        {
                            Directory.CreateDirectory(saveDirectory);
                        }

                        // PDF dosyasını kaydet
                        await using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }


                        byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                        var stream = new MemoryStream(bytes);

                        // PDF'yi yükle

                        using (var pdfDocument = PdfiumViewer.PdfDocument.Load(stream))
                        {
                            // Toplam sayfa sayısını belirle
                            int totalPages = pdfDocument.PageCount;

                            // Sayfaları işle
                            List<Tuple<int, string, Bitmap>> sayfaBilgileri = new List<Tuple<int, string, Bitmap>>();
                            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                            {
                                // Sayfayı resim olarak dönüştür
                                Bitmap pageImage = ConvertPdfPageToImage(pdfDocument, pageIndex);




                                Rectangle cropArea = new Rectangle(689, 550, 114, 14);

                                Bitmap croppedImage = CropImage(pageImage, cropArea);

                                // PNG formatına dönüştür ve kaydet
                                string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"croppedImage-{pageIndex}-.png");
                                croppedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);

                                // Görüntü işleme
                                //Bitmap processedImage = PreprocessImage(croppedImage);

                                //Kırpılmış bölgeyi OCR ile tarat
                                string ocrResult = PerformOcr(croppedImage);
                                string sheetInfo = ExtractSheetInfo(ocrResult);


                                sayfaBilgileri.Add(Tuple.Create(pageIndex + 1, sheetInfo, pageImage));


                                //// Belleği temizle
                                croppedImage.Dispose();
                            }

                            // Sayfaları sheet numarasına göre sırala
                            sayfaBilgileri.Sort((a, b) =>
                            {
                                // Boş dize kontrolü ekleyerek güvenli dönüşüm yapın
                                string[] aParts = a.Item2.Split(' ');
                                string[] bParts = b.Item2.Split(' ');

                                if (aParts.Length > 0 && bParts.Length > 0)
                                {
                                    if (int.TryParse(aParts[0], out int aNumber) && int.TryParse(bParts[0], out int bNumber))
                                    {
                                        return aNumber - bNumber;
                                    }
                                }

                                // Varsayılan olarak sıralamada değişiklik yapmayın
                                return 0;
                            });

                            //// Sayfaları sheet numarasına göre sırala
                            //sayfaBilgileri.Sort((a, b) => int.Parse(a.Item2.Split(' ')[0]) - int.Parse(b.Item2.Split(' ')[0]));


                            // Yeni PDF oluştur
                            using (PdfSharp.Pdf.PdfDocument newPdfDocument = new PdfSharp.Pdf.PdfDocument())
                            {
                                foreach (var sayfaBilgisi in sayfaBilgileri)
                                {
                                    Bitmap pageImage = sayfaBilgisi.Item3;
                                    PdfSharp.Pdf.PdfPage pdfPage = newPdfDocument.AddPage();
                                    pdfPage.Width = XUnit.FromPoint(pageImage.Width);
                                    pdfPage.Height = XUnit.FromPoint(pageImage.Height);

                                    using (XGraphics gfx = XGraphics.FromPdfPage(pdfPage))
                                    {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            pageImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                            XImage xImage = XImage.FromStream(ms);
                                            //gfx.DrawImage(xImage, 0, 0);

                                            // Görüntüyü tam sayfa boyutuna sığdırmak için `DrawImage` kullanın
                                            gfx.DrawImage(xImage, 0, 0, pdfPage.Width, pdfPage.Height);
                                        }
                                    }

                                    // Belleği temizle
                                    pageImage.Dispose();
                                }

                                // Yeni PDF'yi kaydet
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    newPdfDocument.Save(ms, false);
                                    pdfBytes = ms.ToArray();

                                    // pdfBytes adlı byte dizisini API'ye gönderin
                                }
                                //newPdfDocument.Save(outputPdfPath);
                                Console.WriteLine("PDF başarıyla sıralandı ve kaydedildi.");
                            }

                        }


                        var content2 = new ByteArrayContent(pdfBytes);

                        // Dosya adını Content-Disposition başlığına ekleyin
                        content2.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = dosyaAdi // Orijinal dosya adını kullanın
                        };
                        return pdfBytes;
                    }
                    else
                    {
                        // Hata durumunu ele al
                        throw new Exception($"PDF indirme başarısız. StatusCode: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunu ele al 
                throw new Exception($"PDF indirme hatası: {ex.Message}");
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

            // Append to the file instead of overwriting it
            using (StreamWriter sw = File.AppendText(filePath))
            {
                await sw.WriteLineAsync(jsonString);
            }
        }






        #region PDF SIRALI YAPMA AYARLARI VS.
        private static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }
        public static Bitmap ConvertPdfPageToImage(PdfiumViewer.PdfDocument pdfDocument, int pageIndex)
        {
            using (var page = pdfDocument.Render(pageIndex, 600, 600, true))
            {
                return new Bitmap(page);
            }
        }

        public static Bitmap CropImage(Bitmap source, Rectangle cropArea)
        {
            Bitmap croppedImage = new Bitmap(cropArea.Width, cropArea.Height);

            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.DrawImage(source, new Rectangle(0, 0, cropArea.Width, cropArea.Height), cropArea, GraphicsUnit.Pixel);
            }

            return croppedImage;
        }



        // Görüntü ön işleme: Gri tonlama, binaryzasyon, kontrast artırma
        private static Bitmap PreprocessImage(Bitmap image)
        {
            // Gri tonlamaya çevirme
            Bitmap grayImage = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(grayImage))
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
                });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }

            // Kontrast artırma ve binaryzasyon
            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    Color pixelColor = grayImage.GetPixel(x, y);
                    int grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color newColor = grayValue > 128 ? Color.White : Color.Black;
                    grayImage.SetPixel(x, y, newColor);
                }
            }

            return grayImage;
        }

        public static Bitmap AdjustContrast(Bitmap image, double contrast)
        {
            // Kontrastı ayarlamak için bir formül
            // Değer 1'den büyükse kontrast artar, 1'den küçükse azalır. 
            double factor = 259 * (contrast + 255) / (255 * (259 - contrast));
            Bitmap contrastImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    int r = (int)Math.Min(Math.Max((c.R - 128) * factor + 128, 0), 255);
                    int g = (int)Math.Min(Math.Max((c.G - 128) * factor + 128, 0), 255);
                    int b = (int)Math.Min(Math.Max((c.B - 128) * factor + 128, 0), 255);
                    contrastImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return contrastImage;
        }
        public static Bitmap GrayscaleImage(Bitmap image)
        {
            Bitmap grayImage = new Bitmap(image.Width, image.Height);

            using (Graphics g = Graphics.FromImage(grayImage))
            {
                // Gri tonlama
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                        new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                        new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                        new float[] { 0, 0, 0, 1, 0 },
                        new float[] { 0, 0, 0, 0, 1 }
                    });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }

            return grayImage;
        }

        public static Bitmap ApplyConvolutionFilter(Bitmap source, double[,] kernel)
        {
            Bitmap result = new Bitmap(source.Width, source.Height);

            int kernelWidth = kernel.GetLength(1);
            int kernelHeight = kernel.GetLength(0);
            int kernelHalfWidth = kernelWidth / 2;
            int kernelHalfHeight = kernelHeight / 2;

            for (int y = kernelHalfHeight; y < source.Height - kernelHalfHeight; y++)
            {
                for (int x = kernelHalfWidth; x < source.Width - kernelHalfWidth; x++)
                {
                    double blue = 0.0, green = 0.0, red = 0.0;

                    for (int filterY = 0; filterY < kernelHeight; filterY++)
                    {
                        for (int filterX = 0; filterX < kernelWidth; filterX++)
                        {
                            int calcX = x + filterX - kernelHalfWidth;
                            int calcY = y + filterY - kernelHalfHeight;

                            if (calcX >= 0 && calcX < source.Width && calcY >= 0 && calcY < source.Height)
                            {
                                Color sourcePixel = source.GetPixel(calcX, calcY);
                                blue += sourcePixel.B * kernel[filterY, filterX];
                                green += sourcePixel.G * kernel[filterY, filterX];
                                red += sourcePixel.R * kernel[filterY, filterX];
                            }
                        }
                    }

                    int resultR = Math.Min(Math.Max((int)red, 0), 255);
                    int resultG = Math.Min(Math.Max((int)green, 0), 255);
                    int resultB = Math.Min(Math.Max((int)blue, 0), 255);

                    result.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB));
                }
            }

            return result;
        }

        public static Bitmap ThresholdImage(Bitmap image)
        {
            Bitmap thresholdImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    byte gray = (byte)(0.3 * c.R + 0.59 * c.G + 0.11 * c.B);
                    byte threshold = gray > 128 ? (byte)255 : (byte)0;
                    thresholdImage.SetPixel(x, y, Color.FromArgb(threshold, threshold, threshold));
                }
            }

            return thresholdImage;
        }

        public static string PerformOcr(Bitmap image)
        {
            string resultText = string.Empty;

            // Görüntüyü ölçeklendir
            Bitmap resizedImage = ResizeImage(image, image.Width * 10, image.Height * 10); // 10 kat büyütme

            // Görüntü ön işleme adımı
            Bitmap preprocessedImage = PreprocessImage(resizedImage);

            // Tesseract OCR motorunu İngilizce dil desteğiyle başlat ve sadece sayıları tanı
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {

                using (var img = BitmapToPixConverter(preprocessedImage))
                {
                    using (var page = engine.Process(img))
                    {
                        resultText = page.GetText();
                    }
                }
            }

            // OCR sonucunu düzelt
            resultText = CorrectOcrResult(resultText);

            return resultText;
        }

        private static string CorrectOcrResult(string ocrResult)
        {
            // Gerekirse burada ek düzeltmeler yapabilirsiniz
            return ocrResult.Trim();
        }


        public static string ExtractSheetInfo(string ocrResult)
        {
            try
            {


                string sheetInfo = "";

                // OCR sonucunu temizle
                string cleanedOcrResult = ocrResult.Replace("\n", " ").Replace("\r", " ").Replace("\\", " ");
                cleanedOcrResult = Regex.Replace(cleanedOcrResult, @"\s+", " "); // Fazla boşlukları tek bir boşluk ile değiştir
                cleanedOcrResult = Regex.Replace(cleanedOcrResult, @"[^a-zA-Z0-9\s]", ""); // Alfabetik ve sayısal olmayan karakterleri kaldır
                cleanedOcrResult = cleanedOcrResult.Trim(); // Başındaki ve sonundaki boşlukları kaldır

                // Temizlenmiş metni kontrol edelim
                Console.WriteLine("Temizlenmiş OCR Sonucu: " + cleanedOcrResult);

                // "SHEET" ile başlayan ve "OF" veya "0F" ile biten kısmı bul
                string[] parts = cleanedOcrResult.Split(new string[] { "SHEET ", "SHEET", "sHEET", "1SHEET" }, StringSplitOptions.None);

                // İlgilendiğimiz kısım ikinci elemandır (SHEET'ten sonraki)
                if (parts.Length > 1)
                {
                    string sheetPart = parts[1].Trim();
                    // "OF" veya "0F" ile biten kısmı ayır
                    string[] sheetNumbers = sheetPart.Split(new string[] { "OF", "0F", "or" }, StringSplitOptions.None);

                    if (sheetNumbers.Length > 1)
                    {
                        string sheetNumber = sheetNumbers[0].Trim();
                        string totalSheets = sheetNumbers[1].Trim();

                        // İstenen çıktıyı oluştur
                        sheetInfo = sheetNumber;
                        //sheetInfo = $"SHEET {sheetNumber}";
                        //sheetInfo = $"SHEET {sheetNumber} OF {totalSheets}";
                    }
                }

                return sheetInfo;
            }
            catch (Exception ex)
            {

                return "err";
            }
        }

        private static Pix BitmapToPixConverter(Bitmap image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                return Pix.LoadFromMemory(memoryStream.ToArray());
            }
        }

        #endregion




    }
}
