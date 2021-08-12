using Example.support;
using RestWrapperCore;
using System;
using System.Net.Http;
using System.Text.Json;
using static RestWrapperCore.RestWrapper;

namespace Example
{
    class Program
    {
        private static readonly HttpClientHandler _clientHandler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        //Needs for use one client instance
        private static readonly HttpClient _client = new HttpClient(_clientHandler, false) { Timeout = new TimeSpan(0, 5, 0) };



        static void Main(string[] args)
        {
            //RestWrapper.LogsEnabled = true;
            RequestWithJsonElementAsResponse();
            RequestWithPostAsStatus();
            RequestWithParsingResponse();
            RequestWithStatusCodeAsResult();
            RequestWithPostAsStatus();
            RequestWithJsonAsObject();
            RequestWithPostObject();
            RequestWithWrongUrl();
        }

        private static void RequestWithJsonElementAsResponse()
        {
            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .LogsEnabled(false)
                        .Build()
                        .SendGet<JsonElement>()
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Result from jsonElement: {response.GetProperty("data").GetProperty("email")}");
        }

        private static void RequestWithStatusCodeAsResult()
        {
            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .AddHeaders("Custom","Test")
                        .Build()
                        .SendGet(disableBodyLogs: true)
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Result status code: {(int)response.StatusCode}");
        }

        private static void RequestWithJsonAsObject()
        {
            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .Build()
                        .SendGet<JsonToObject>()
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Result as object: {response.Data.Email}");
        }

        private static void RequestWithParsingResponse()
        {
            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .Build()
                        .SendGet()
                        .GetAwaiter().GetResult();
            var resp = RestWrapper.ParseAs<JsonToObject>(response);

            Console.WriteLine($"\n>>> Result as object: {resp.Data.Email}");
        }

        private static void RequestWithPostObject()
        {
            var postObj = new ObjToJson {Job = "leader", Name = "morpheus"};

            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .Build()
                        .SendPost<JsonElement, ObjToJson>(postObj)
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Id from response is: {response.GetProperty("id")}");
        }

        private static int RequestWithPostAsStatus()
        {
            var postObj = new ObjToJson { Job = "leader", Name = "morpheus" };

            return (int)RestWrapper.CreateWithHttpClient(_client)
                            .Url($"https://reqres.in/api/users/2")
                            .AddHeaders("Custom", "Test")
                            .AddHeaders("Custom2", "Test2")
                            .AddHeaders("Custom3", "Test2")
                            .Build()
                            .SendPost<ObjToJson>(postObj)
                            .GetAwaiter().GetResult().StatusCode;
        }

        private static int RequestWithWrongUrl()
        {
            var postObj = new ObjToJson { Job = "leader", Name = "morpheus" };

            return (int)RestWrapper.CreateWithHttpClient(_client)
                            .Url($"raasdasdasdsdasdasdeqres.in/users/2")
                            .AddHeaders("Custom", "Test")
                            .AddHeaders("Custom2", "Test2")
                            .Build()
                            .SendPost<ObjToJson>(postObj)
                            .GetAwaiter().GetResult().StatusCode;
        }
    }
}
