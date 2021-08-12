using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;

namespace Example
{
    class LogTest
    {
        private readonly ILogger<LogTest> _logger;

        public LogTest(ILogger<LogTest> logger){
            _logger = logger;
        }

        public LogTest()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "[dd/MM/yyyy HH:mm:ss.fff] ";
                });
            });
            ILogger<LogTest> logger = loggerFactory.CreateLogger<LogTest>();
            _logger = logger;
        }


        //public void testc()
        //{
        //    _logger.LogInformation("Testim");
        //    _logger.LogError("Error");
        //}

        //public string printLogs(HttpResponseMessage response)
        //{
        //    //var options = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = false, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) };

        //    var kek = response.RequestMessage.Headers;
        //    var s = kek.Select(v => string.Format($"{v.Key}={string.Join(", ", v.Value)}"));
        //    var strrr = string.Join($"\n{"", 18}", s);
        //    var requestBody = response.RequestMessage.Content;
        //    var responseBody = response.Content;
        //    //var todoItemJson = new StringContent(responseBody.ReadAsStringAsync().GetAwaiter().GetResult(), Encoding.UTF8, "application/json");

        //    //var ssssss = JsonDocument.Parse(todoItemJson.ReadAsStringAsync().GetAwaiter().GetResult());
        //    //string json;
        //    //using (var stream = new MemoryStream())
        //    //{
        //    //    Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        //    //    ssssss.WriteTo(writer);
        //    //    writer.Flush();
        //    //    json = Encoding.UTF8.GetString(stream.ToArray());
        //    //}

        //    var formated = $"\n/**\n* Request method: {response.RequestMessage.Method}" +
        //        $"\n* Request URI: {response.RequestMessage.RequestUri,32}" +
        //        $"\n* Headers: {strrr,50}" +
        //        $"\n* Request body:\n* {(requestBody != null ? requestBody.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty)}" +
        //        $"\n*\n* Response code: '{(int)response.StatusCode} {response.StatusCode}'" +
        //        $"\n* Response body:\n* {(responseBody != null ? responseBody.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty)}\n**/";

        //    //Console.WriteLine(formated);

        //    _logger.LogInformation(formated);
        //    return formated;
        //}

    }
}
