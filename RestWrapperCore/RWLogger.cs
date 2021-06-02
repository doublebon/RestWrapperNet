using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RestWrapperCore
{
    class RWLogger
    {
        private readonly ILogger<RWLogger> _logger;

        public RWLogger(ILogger<RWLogger> logger)
        {
            _logger = logger;
        }

        public RWLogger()
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
            ILogger<RWLogger> logger = loggerFactory.CreateLogger<RWLogger>();
            _logger = logger;
        }

        public string PrintLogs(HttpResponseMessage response)
        {
            var rawHeaders = response.RequestMessage.Headers.Select(v => string.Format($"{v.Key}={string.Join(", ", v.Value)}"));
            var formatedHeaders = string.Join($"\n{"",18}", rawHeaders);
            var requestBody = response.RequestMessage.Content;
            var responseBody = response.Content;

            var formated = $"\n/**\n* Request method: {response.RequestMessage.Method}" +
                $"\n* Request URI: {response.RequestMessage.RequestUri,32}" +
                $"\n* Headers: {formatedHeaders,50}" +
                $"\n* Request body:\n* {(requestBody != null ? requestBody.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty)}" +
                $"\n*\n* Response code: '{(int)response.StatusCode} {response.StatusCode}'" +
                $"\n* Response body:\n* {(responseBody != null ? responseBody.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty)}\n**/";

            _logger.LogInformation(formated);
            return formated;
        }
    }
}
