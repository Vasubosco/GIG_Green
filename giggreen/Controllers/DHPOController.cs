using giggreen.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Concurrent;
using System.Text;
using System.Xml.Serialization;
using System.Net;
using System;
using ClosedXML.Excel;



namespace giggreen.Controllers
{

    class RequestResult
    {
        public string SoapRequestXml { get; set; }
        public string SoapResponse { get; set; }
    }
    [XmlRoot("Files")]
    public class Files
    {
        [XmlElement("File")]
        public List<File> FileList { get; set; }
    }

    public class File
    {
        [XmlAttribute("FileID")]
        public Guid FileID { get; set; }

        [XmlAttribute("FileName")]
        public string FileName { get; set; }

        [XmlAttribute("SenderID")]
        public string SenderID { get; set; }

        [XmlAttribute("ReceiverID")]
        public string ReceiverID { get; set; }

        [XmlAttribute("TransactionDate")]
        public string TransactionDate { get; set; }

        [XmlAttribute("RecordCount")]
        public int RecordCount { get; set; }

        [XmlAttribute("IsDownloaded")]
        public string IsDownloaded { get; set; }
    }
    public class DHPOController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static List<FileDetailsModel> fileDetails = new List<FileDetailsModel>();
        static List<DHPOViewModel> model = new List<DHPOViewModel>();
        static Random random = new Random();

        //private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public DHPOController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        {
            this.Environment = _environment;
        }



        //public HomeController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        //{
        //    this.Environment = _environment;
        //}


        public IActionResult Index()
        {
            TempData["UserName"] = null;
            TempData["RoleName"] = null;
            return View();
        }

        [HttpPost]
        public IActionResult ExportData([FromBody] List<DHPOViewModel> data)
        {
            if (data == null || data.Count == 0)
            {
                return BadRequest("Data is null or empty");
            }

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DHPOData");

                    // Set the column headers
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true; // Set column header bold
                    worksheet.Cell(1, 1).Value = "FileID";
                    worksheet.Cell(1, 2).Value = "FileName";
                    worksheet.Cell(1, 3).Value = "SenderID";
                    worksheet.Cell(1, 4).Value = "ReceiverID";
                    worksheet.Cell(1, 5).Value = "TransactionDate";
                    worksheet.Cell(1, 6).Value = "RecordCount";
                    worksheet.Cell(1, 7).Value = "IsDownloaded";                    
                    

                    // Populate the data
                    int row = 2;
                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.FileID;
                        worksheet.Cell(row, 2).Value = item.FileName;
                        worksheet.Cell(row, 3).Value = item.SenderID;
                        worksheet.Cell(row, 4).Value = item.ReceiverID;
                        worksheet.Cell(row, 5).Value = item.TransactionDate;
                        worksheet.Cell(row, 6).Value = item.RecordCount;
                        worksheet.Cell(row, 7).Value = item.IsDownloaded;
                        // Add more data if needed up to column G
                        row++;
                    }

                    // Autofit columns for better readability
                    worksheet.Columns().AdjustToContents();

                    // Set borders around cells
                    var range = worksheet.Range(worksheet.FirstCellUsed(), worksheet.LastCellUsed());
                    var borders = range.Style.Border;
                    borders.OutsideBorder = XLBorderStyleValues.Thin;
                    borders.OutsideBorderColor = XLColor.Black;
                    borders.InsideBorder = XLBorderStyleValues.Thin;
                    borders.InsideBorderColor = XLColor.Black;

                    // Remove the second row if it's empty
                    if (worksheet.Cell(2, 1).IsEmpty())
                    {
                        worksheet.Row(2).Delete();
                    }

                    // Save the Excel file to a memory stream
                    using (var stream = new System.IO.MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        // Return the file as a downloadable response
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DHPOData.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Index(string category, string dateRangeFrom, string dateRangeTo, string searchFile, string searchPartner)
        {

            var requestQueue = new ConcurrentQueue<string>();

            // Create HttpClient with Proxy Configuration
            var httpClientHandler = new HttpClientHandler
            {
                // Replace 'proxyAddress' and 'proxyPort' with your actual proxy settings
                Proxy = new WebProxy("http://10.97.2.93:8080"),
                UseProxy = true,
            };
            model = bindtempdata();
            return View(model);
            // Replace with your actual SOAP endpoint URL and SOAPAction

            string soapAction = "https://www.shafafiya.org/v2/SearchTransactions";

            // Create HttpClient (consider using a single instance for multiple requests)
            HttpClient httpClient = new HttpClient(httpClientHandler);

            httpClient.DefaultRequestHeaders.Add("SOAPAction", soapAction);

            // Set the Content-Type header to indicate SOAP request
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/soap+xml; charset=UTF-8");
            // Increase the timeout to a larger value (e.g., 300 seconds)
            httpClient.Timeout = TimeSpan.FromSeconds(300);


            try
            {


                DateTime datevalue = (Convert.ToDateTime(dateRangeFrom.ToString()));

                String dy = datevalue.Day.ToString();
                String mn = datevalue.Month.ToString();
                String yy = datevalue.Year.ToString();


                DateTime datevalue1 = (Convert.ToDateTime(dateRangeTo.ToString()));

                String dy1 = datevalue1.Day.ToString();
                String mn1 = datevalue1.Month.ToString();
                String yy1 = datevalue1.Year.ToString();


                DateTime startInDate = new DateTime(int.Parse(yy), int.Parse(mn), int.Parse(dy));
                DateTime endOutDate = new DateTime(int.Parse(yy1), int.Parse(mn1), int.Parse(dy1));


                // fri
                //DateTime startInDate = new DateTime(2023, 3, 1);
                //DateTime endOutDate = new DateTime(2023, 5, 30);
                TimeSpan daysInterval = endOutDate - startInDate;
                int daysBetweenDates = daysInterval.Days;

                int numberOfPoints = 24;
                DateTime startDate = startInDate;
                DateTime endDate = endOutDate;
                Console.WriteLine("Dividing Date Range into Days values:" + daysBetweenDates);
                for (int k = 0; k <= daysBetweenDates; k += 2)
                {
                    //long daysToAdd = (long)(daysBetweenDates * k);
                    endDate = startDate.AddDays(2);
                    Console.WriteLine("Starting Date Range from :" + startDate + " to End Date:" + endDate);
                    TimeSpan interval = endDate - startDate;
                    double totalTicks = interval.Ticks;

                    DateTime startTime = startDate;
                    DateTime endTime = endOutDate;

                    double intervalBetweenPoints = totalTicks / numberOfPoints;
                    for (int i = 0; i <= numberOfPoints; i++)
                    {
                        long ticksToAdd = (long)(intervalBetweenPoints);
                        DateTime pointDate = startDate.AddTicks(ticksToAdd);
                        string formattedTimeFrom = startTime.ToString("yyyy-MM-dd HH:mm:ss");
                        //startTime.ToString("dd/MM/yyyy HH:mm:ss");

                        endTime = startTime.AddTicks(ticksToAdd);

                        string formattedTimeTo = endTime.ToString("yyyy-MM-dd HH:mm:ss");
                        //endTime.ToString("dd/MM/yyyy HH:mm:ss");

                        Console.WriteLine("Starting Time Range from :" + formattedTimeFrom + " to End Date:" + formattedTimeTo);
                        startTime = endTime;

                        /* 2 – Claim.Submission transaction only;  numberOfPoints=24 for two days :: 9 minutes  numberOfPoints=48 for two days :: 11 minutes
                         4 – Person.Register transaction only;
                         8 – Remittance.Advice transaction only;  numberOfPoints=24 for two days :: 9 minutes   numberOfPoints=48 for two days :: 11 minutes
                         16 – Prior.Request transaction only;
                         32 – Prior.Authorization transaction only; numberOfPoints=24 for two days :: 9 minutes
                         -1 – Search for all transactions;*/
                        // Construct the SOAP request XML
                        string soapRequest = $@"
                              <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:v2=""https://www.shafafiya.org/v2/"">
                                <soap:Header/>
                                <soap:Body>
                                <v2:SearchTransactions> 
                             <v2:login>axa</v2:login>
                             <v2:pwd>axa2012</v2:pwd>
                             <v2:direction>2</v2:direction>
                             <v2:callerLicense>A026</v2:callerLicense>
                             <v2:transactionID>{category}</v2:transactionID>
                             <v2:transactionStatus>1</v2:transactionStatus>
                             <v2:transactionFromDate>{formattedTimeFrom}</v2:transactionFromDate>
                           <v2:transactionToDate>{formattedTimeTo}</v2:transactionToDate>
                             <v2:minRecordCount>1</v2:minRecordCount>
                             <v2:maxRecordCount>10000</v2:maxRecordCount>
                          </v2:SearchTransactions> 
                         </soap:Body>
                         </soap:Envelope>";

                        // Construct the SOAP request XML
                        string soapRequestDownloaded = $@"
                          <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:v2=""https://www.shafafiya.org/v2/"">
                            <soap:Header/>
                             <soap:Body>
                         <v2:SearchTransactions> 
                         <v2:login>axa</v2:login>
                         <v2:pwd>axa2012</v2:pwd>
                         <v2:direction>2</v2:direction>
                         <v2:callerLicense>A026</v2:callerLicense>
                         <v2:transactionID>{category}</v2:transactionID>
                         <v2:transactionStatus>2</v2:transactionStatus>
                         <v2:transactionFromDate>{formattedTimeFrom}</v2:transactionFromDate>
                       <v2:transactionToDate>{formattedTimeTo}</v2:transactionToDate>
                         <v2:minRecordCount>1</v2:minRecordCount>
                         <v2:maxRecordCount>10000</v2:maxRecordCount>
                      </v2:SearchTransactions> 
                     </soap:Body>
                     </soap:Envelope>";
                        // Add your SOAP request XMLs to the queue
                        requestQueue.Enqueue(soapRequest);
                        requestQueue.Enqueue(soapRequestDownloaded);

                    }
                    startDate = endDate;


                    List<Task> processingTasks = new List<Task>();
                    var results = new ConcurrentBag<RequestResult>();

                    while (!requestQueue.IsEmpty)
                    {
                        if (requestQueue.TryDequeue(out string soapRequestXml))
                        {
                            processingTasks.Add(ProcessSoapRequestAsync(httpClient, soapRequestXml, results));
                        }
                    }

                    await Task.WhenAll(processingTasks);

                    // Present the consolidated results in the UI
                    /* foreach (var result in results)
					 {

						 Console.WriteLine($"Request XML: {result.SoapRequestXml}");
						 Console.WriteLine($"Response: {result.SoapResponse}");
						 Console.WriteLine();
					 }*/

                }


            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the request

                Console.WriteLine($"Error construction Date request: {ex.StackTrace}");
                Console.WriteLine($"Error construction Date request: {ex.Message}");
            }
            if (model != null && model.Count > 0)
            {

            }
            else
            {


            }

            return View(model);
        }
        static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        public List<DHPOViewModel> bindtempdata()
        {
            List<DHPOViewModel> records = new List<DHPOViewModel>();
            DateTime startDate = new DateTime(2020, 1, 1);
            DateTime endDate = new DateTime(2023, 12, 31);
            for (int i = 0; i < 100; i++)
            {
                records.Add(new DHPOViewModel
                {
                    FileID = RandomString(10),
                    FileName = RandomString(8) + ".txt",
                    SenderID = RandomString(5),
                    ReceiverID = RandomString(5),
                    TransactionDate = RandomDate(startDate, endDate).ToString("yyyy-MM-dd"),
                    RecordCount = random.Next(1, 1001).ToString(),
                    IsDownloaded = random.Next(0, 2) == 0 ? "True" : "False",
                    fromdate = RandomDate(startDate, endDate).ToString("yyyy-MM-dd")
                });
            }
            return records;
        }

        static DateTime RandomDate(DateTime start, DateTime end)
        {
            int range = (end - start).Days;
            return start.AddDays(random.Next(range));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        static async Task GetTaskAsync()
        {
            var requestQueue = new ConcurrentQueue<string>();

            // Create HttpClient with Proxy Configuration
            var httpClientHandler = new HttpClientHandler
            {
                // Replace 'proxyAddress' and 'proxyPort' with your actual proxy settings
                Proxy = new WebProxy("http://10.97.2.93:8080"),
                UseProxy = true,
            };

            // Replace with your actual SOAP endpoint URL and SOAPAction

            string soapAction = "https://www.shafafiya.org/v2/SearchTransactions";

            // Create HttpClient (consider using a single instance for multiple requests)
            HttpClient httpClient = new HttpClient(httpClientHandler);

            httpClient.DefaultRequestHeaders.Add("SOAPAction", soapAction);

            // Set the Content-Type header to indicate SOAP request
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/soap+xml; charset=UTF-8");
            // Increase the timeout to a larger value (e.g., 300 seconds)
            httpClient.Timeout = TimeSpan.FromSeconds(300);


            try
            {
                DateTime startInDate = new DateTime(2023, 3, 1);
                DateTime endOutDate = new DateTime(2023, 5, 30);
                TimeSpan daysInterval = endOutDate - startInDate;
                int daysBetweenDates = daysInterval.Days;

                int numberOfPoints = 24;
                DateTime startDate = startInDate;
                DateTime endDate = endOutDate;
                Console.WriteLine("Dividing Date Range into Days values:" + daysBetweenDates);
                for (int k = 0; k <= daysBetweenDates; k += 2)
                {
                    //long daysToAdd = (long)(daysBetweenDates * k);
                    endDate = startDate.AddDays(2);
                    Console.WriteLine("Starting Date Range from :" + startDate + " to End Date:" + endDate);
                    TimeSpan interval = endDate - startDate;
                    double totalTicks = interval.Ticks;

                    DateTime startTime = startDate;
                    DateTime endTime = endOutDate;

                    double intervalBetweenPoints = totalTicks / numberOfPoints;
                    for (int i = 0; i <= numberOfPoints; i++)
                    {
                        long ticksToAdd = (long)(intervalBetweenPoints);
                        DateTime pointDate = startDate.AddTicks(ticksToAdd);
                        string formattedTimeFrom = startTime.ToString("dd/MM/yyyy HH:mm:ss");

                        endTime = startTime.AddTicks(ticksToAdd);

                        string formattedTimeTo = endTime.ToString("dd/MM/yyyy HH:mm:ss");

                        Console.WriteLine("Starting Time Range from :" + formattedTimeFrom + " to End Date:" + formattedTimeTo);
                        startTime = endTime;

                        /* 2 – Claim.Submission transaction only;  numberOfPoints=24 for two days :: 9 minutes  numberOfPoints=48 for two days :: 11 minutes
                         4 – Person.Register transaction only;
                         8 – Remittance.Advice transaction only;  numberOfPoints=24 for two days :: 9 minutes   numberOfPoints=48 for two days :: 11 minutes
                         16 – Prior.Request transaction only;
                         32 – Prior.Authorization transaction only; numberOfPoints=24 for two days :: 9 minutes
                         -1 – Search for all transactions;*/
                        // Construct the SOAP request XML
                        string soapRequest = $@"
                              <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:v2=""https://www.shafafiya.org/v2/"">
                                <soap:Header/>
                                <soap:Body>
                                <v2:SearchTransactions> 
                             <v2:login>axa</v2:login>
                             <v2:pwd>axa2012</v2:pwd>
                             <v2:direction>2</v2:direction>
                             <v2:callerLicense>A026</v2:callerLicense>
                             <v2:transactionID>16</v2:transactionID>
                             <v2:transactionStatus>1</v2:transactionStatus>
                             <v2:transactionFromDate>{formattedTimeFrom}</v2:transactionFromDate>
                           <v2:transactionToDate>{formattedTimeTo}</v2:transactionToDate>
                             <v2:minRecordCount>1</v2:minRecordCount>
                             <v2:maxRecordCount>10000</v2:maxRecordCount>
                          </v2:SearchTransactions>

                         </soap:Body>
                         </soap:Envelope>";

                        // Construct the SOAP request XML
                        string soapRequestDownloaded = $@"
                          <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:v2=""https://www.shafafiya.org/v2/"">
                            <soap:Header/>
                             <soap:Body>
                         <v2:SearchTransactions> 
                         <v2:login>axa</v2:login>
                         <v2:pwd>axa2012</v2:pwd>
                         <v2:direction>2</v2:direction>
                         <v2:callerLicense>A026</v2:callerLicense>
                         <v2:transactionID>16</v2:transactionID>
                         <v2:transactionStatus>2</v2:transactionStatus>
                         <v2:transactionFromDate>{formattedTimeFrom}</v2:transactionFromDate>
                       <v2:transactionToDate>{formattedTimeTo}</v2:transactionToDate>
                         <v2:minRecordCount>1</v2:minRecordCount>
                         <v2:maxRecordCount>10000</v2:maxRecordCount>
                      </v2:SearchTransactions>

                     </soap:Body>
                     </soap:Envelope>";
                        // Add your SOAP request XMLs to the queue
                        requestQueue.Enqueue(soapRequest);
                        requestQueue.Enqueue(soapRequestDownloaded);

                    }
                    startDate = endDate;


                    List<Task> processingTasks = new List<Task>();
                    var results = new ConcurrentBag<RequestResult>();

                    while (!requestQueue.IsEmpty)
                    {
                        if (requestQueue.TryDequeue(out string soapRequestXml))
                        {
                            processingTasks.Add(ProcessSoapRequestAsync(httpClient, soapRequestXml, results));
                        }
                    }

                    await Task.WhenAll(processingTasks);

                    // Present the consolidated results in the UI
                    /*foreach (var result in results)
                    {

                        Console.WriteLine($"Request XML: {result.SoapRequestXml}");
                        Console.WriteLine($"Response: {result.SoapResponse}");
                        Console.WriteLine();
                    }*/

                }


            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the request

                Console.WriteLine($"Error construction Date request: {ex.StackTrace}");
                Console.WriteLine($"Error construction Date request: {ex.Message}");
            }
        }

        static async Task ProcessSoapRequestAsync(HttpClient client, string soapRequestXml, ConcurrentBag<RequestResult> results)
        {
            // Service URL remains the same
            string endpointUrl = "https://shafafiya.doh.gov.ae/v3/webservices.asmx";

            // Create the HTTP content
            var httpContent = new StringContent(soapRequestXml, Encoding.UTF8, "application/soap+xml");

            Console.WriteLine("**************ProcessSoapRequestAsync*******************");

            try
            {
                // Send the SOAP request and get the response
                HttpResponseMessage response = await client.PostAsync(endpointUrl, httpContent);

                string soapResponse = await response.Content.ReadAsStringAsync();



                // Create a result object and store it in the results collection
                var requestResult = new RequestResult
                {
                    SoapRequestXml = soapRequestXml,
                    SoapResponse = soapResponse
                };


                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(@soapResponse);

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                namespaceManager.AddNamespace("soap", "http://www.w3.org/2003/05/soap-envelope");
                namespaceManager.AddNamespace("ns", "https://www.shafafiya.org/v2/");

                XmlNode foundTransactionsNode = xmlDoc.SelectSingleNode("//soap:Envelope/soap:Body/ns:SearchTransactionsResponse/ns:foundTransactions", namespaceManager);

                if (foundTransactionsNode != null)
                {
                    XmlNode cdataNode = foundTransactionsNode.SelectSingleNode("text()", namespaceManager);
                    if (cdataNode != null)
                    {
                        string embeddedXml = cdataNode.Value;

                        // Deserialize the CDATA content into the class
                        var serializer = new XmlSerializer(typeof(Files));
                        using (var reader = XmlReader.Create(new StringReader(embeddedXml)))
                        {
                            var files = (Files)serializer.Deserialize(reader);
                            if (files != null && files.FileList != null && files.FileList.Count > 0)
                            {

                                Console.WriteLine("Extracted Embedded XML COUNT:" + files.FileList.Count);
                                // Access the deserialized data
                                /* foreach (var file in files.FileList)
                                 {
                                     Console.WriteLine($"FileID: {file.FileID}");
                                     Console.WriteLine($"FileName: {file.FileName}");
                                     // ... and so on for other properties
                                 }*/
                                foreach (var file in files.FileList)
                                {
                                    DHPOViewModel detailsModel = new DHPOViewModel();

                                    detailsModel.FileID = file.FileID.ToString();

                                    detailsModel.FileName = file.FileName.ToString();
                                    detailsModel.SenderID = file.SenderID.ToString();
                                    detailsModel.ReceiverID = file.ReceiverID.ToString();
                                    detailsModel.TransactionDate = file.TransactionDate.ToString();
                                    detailsModel.RecordCount = file.RecordCount.ToString();
                                    detailsModel.IsDownloaded = file.IsDownloaded.ToString();


                                    model.Add(detailsModel);
                                }



                            }
                        }


                        //   Console.WriteLine(embeddedXml);
                    }
                    else
                    {
                        //Console.WriteLine("No embedded XML found inside CDATA.");
                    }
                }
                else
                {
                    Console.WriteLine("No <foundTransactions> node found.");
                }


                results.Add(requestResult);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the request

                Console.WriteLine($"Error sending SOAP request: {ex.StackTrace}");
                Console.WriteLine($"Error sending SOAP request: {ex.Message}");
            }



        }
    }
}