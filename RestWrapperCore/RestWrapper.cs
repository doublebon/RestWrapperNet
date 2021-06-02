using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RestWrapperCore
{
    public class RestWrapper
    {
        #region MainVariables

        public static bool LogsEnabled { get; set; } = false;
        private static HttpClient _client;
        private static RWLogger   _logger = new RWLogger();

        RestWrapper(HttpClient client)
        {
            _client = client;
        }

        private string Url { get; set; }
        private string UrlPathSegment { get; set; }
        private Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        private Dictionary<string, string> Query { get; } = new Dictionary<string, string>();
        private Dictionary<string, string> PathParams { get; } = new Dictionary<string, string>();
        private bool SecureConnect { get; set; } = true;
        private int UrlPort { get; set; } = -1;
        private static ThreadLocal<string> _threadUrl = new ThreadLocal<string>();

        public static Builder CreateWithHttpClient(HttpClient client)
        {
            return new Builder(client);
        }
        #endregion

        #region Builder
        public class Builder
        {
            private readonly RestWrapper _restCore;

            public Builder(HttpClient client)
            {
                _restCore = new RestWrapper(client);
            }

            public Builder Url(string baseUrl)
            {
                _restCore.Url = baseUrl.Replace("https://","").Replace("http://","");
                return this;
            }

            public Builder UrlPathSegment(string urlPath)
            {
                _restCore.UrlPathSegment = urlPath;
                return this;
            }

            public Builder AddHeaders(string key, string value)
            {
                _restCore.Headers.Add(key, value);
                return this;
            }

            public Builder AddHeaders(Dictionary<string, string> headers)
            {
                foreach (var (key, value) in headers)
                {
                    _restCore.Headers.Add(key, value);
                }
                return this;
            }

            public Builder AddQuery(string key, string value)
            {
                _restCore.Query.Add(key, value);
                return this;
            }

            public Builder AddQuery(Dictionary<string, string> queryDict)
            {
                foreach (var (key, value) in queryDict)
                {
                    _restCore.Query.Add(key, value);
                }
                return this;
            }

            public Builder PathParams(string key, string value)
            {
                _restCore.PathParams.Add(key, value);
                return this;
            }

            public Builder SecureConnect(bool isSecure)
            {
                _restCore.SecureConnect = isSecure;
                return this;
            }

            public Builder UrlPort(int urlPort)
            {
                _restCore.UrlPort = urlPort;
                return this;
            }

            public RestWrapper Build()
            {
                return _restCore;
            }

            // public static implicit operator RestCore(Builder builder)
            // {
            //     return builder._restCore;
            // }
        }
        #endregion

        #region RequestsConfigure
        private string GetConfiguredUrl()
        {
            if (Url == null) throw new NoNullAllowedException("Url can't be empty !");

            var protocol = SecureConnect ? "https" : "http";
            var coreUrl = new Uri($"{protocol}://{Url}");
            var fullUrl = UrlPathSegment != null ? new Uri(coreUrl, UrlPathSegment).OriginalString : coreUrl.OriginalString;

            if (PathParams.Count > 0)
            {
                foreach (var (key, value) in PathParams)
                {
                    if (!fullUrl.Contains(key))
                    {
                        throw new KeyNotFoundException($"Path param '{{{key}}}' not fount at url: " + fullUrl);
                    }
                    fullUrl = fullUrl.Replace("{" + key + "}", value);
                }
            }

            var builder = new UriBuilder(fullUrl);
            builder.Port = UrlPort;

            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var (key, value) in Query)
            {
                query[key] = value;
            }
            builder.Query = query.ToString();

            return builder.ToString();
        }

        #endregion

        #region Get
        public async Task<TClass> SendGet<TClass>(bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var url = GetConfiguredUrl();
            Console.WriteLine($"\n> GET {url}\n");
            var httpResponse = await _client.GetAsync(url);

            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            //Check that response status code is 200
            httpResponse.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<TClass>(await httpResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public async Task<HttpResponseMessage> SendGet(bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var url = GetConfiguredUrl();
            Console.WriteLine($"\n> GET {url}\n");
            var httpResponse = await _client.GetAsync(url);

            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            return httpResponse;
        }

        #endregion

        #region Post
        public async Task<TClass> SendPost<TClass, T>(T obj, bool ignoreNullValues = false, bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var options = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = ignoreNullValues, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) };
            var serializedObject = JsonSerializer.Serialize(obj, options);
            var todoItemJson = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var url = GetConfiguredUrl();

            Console.WriteLine($"\n> POST {url}");
            if (!disableBodyLogs)
                Console.WriteLine($"-> Request Body:\n{serializedObject}");

            var httpResponse = await _client.PostAsync(url, todoItemJson);
            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            httpResponse.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<TClass>(await httpResponse.Content.ReadAsStringAsync(), options);
        }

        public async Task<HttpResponseMessage> SendPostMultipart(MultipartFormDataContent form, bool ignoreNullValues = false, bool disableBodyLogs = false)
        {
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var url = GetConfiguredUrl();

            Console.WriteLine($"\n> POST {url}");
            
            var httpResponse = await _client.PostAsync(url, form);
            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            return httpResponse;
        }

        public async Task<HttpResponseMessage> SendPost<T>(T obj, bool ignoreNullValues = false, bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var options = new JsonSerializerOptions { IgnoreNullValues = ignoreNullValues, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) };
            var serializedObject = JsonSerializer.Serialize(obj, options);
            var todoItemJson = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var url = GetConfiguredUrl();

           
            //Console.WriteLine($"\n> POST {url}");
            //if (!disableBodyLogs)
            //    Console.WriteLine($"-> Request Body:\n{serializedObject}");

            var httpResponse = await _client.PostAsync(url, todoItemJson);
            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (LogsEnabled)
                _logger.PrintLogs(httpResponse);

            //if (disableBodyLogs)
            //    Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            //else
            //    Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            return httpResponse;
        }

        #endregion

        #region Put
        public async Task<TClass> SendPut<TClass, T>(T obj, bool ignoreNullValues = false, bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var options = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = ignoreNullValues, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) };
            var serializedObject = JsonSerializer.Serialize(obj, options);
            var todoItemJson = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var url = GetConfiguredUrl();

            Console.WriteLine($"\n> PUT {url}");
            if (!disableBodyLogs)
                Console.WriteLine($"-> Request Body:\n{serializedObject}");

            //Console.WriteLine($"\n> PUT {url}\n-> Request Body:\n{serializedObject}");

            var httpResponse = await _client.PutAsync(url, todoItemJson);
            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();
            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            httpResponse.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<TClass>(await httpResponse.Content.ReadAsStringAsync(), options);
        }

        public async Task<HttpResponseMessage> SendPut<T>(T obj, bool ignoreNullValues = false, bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var options = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = ignoreNullValues, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) };
            var serializedObject = JsonSerializer.Serialize(obj, options);
            var todoItemJson = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var url = GetConfiguredUrl();
            Console.WriteLine($"\n> PUT {url}");
            if (!disableBodyLogs)
                Console.WriteLine($"-> Request Body:\n{serializedObject}");

            var httpResponse = await _client.PutAsync(url, todoItemJson);
            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            return httpResponse;
        }

        #endregion

        #region Delete
        public async Task<TClass> SendDelete<TClass>(bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var url = GetConfiguredUrl();
            Console.WriteLine($"\n> DELETE {url}\n");
            var httpResponse = await _client.DeleteAsync(url);

            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            //Check that response status code is 200
            httpResponse.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<TClass>(await httpResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public async Task<HttpResponseMessage> SendDelete(bool disableBodyLogs = false)
        {
            //Setting headers
            foreach (var (key, value) in Headers)
                _client.DefaultRequestHeaders.Add(key, value);

            var url = GetConfiguredUrl();
            Console.WriteLine($"\n> DELETE {url}\n");
            var httpResponse = await _client.DeleteAsync(url);

            //Clean default headers after request
            _client.DefaultRequestHeaders.Clear();

            if (disableBodyLogs)
                Console.WriteLine($"<- Response status code: {(int)httpResponse.StatusCode}");
            else
                Console.WriteLine($"<- Response body: \n{httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            return httpResponse;
        }

        #endregion

        #region Parser
        public static TClass ParseAs<TClass>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<TClass>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult(), new JsonSerializerOptions() { WriteIndented = true });
        }

        #endregion
    }
}
