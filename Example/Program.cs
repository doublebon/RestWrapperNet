using Example.support;
using RestWrapperCore;
using System;
using System.Net.Http;
using System.Text.Json;

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
            requestWithJsonElementAsResponse();
            requestWithStatusCodeAsResult();
            requestWithJsonAsObject();
            requestWithPostObject();
        }

        private static void requestWithJsonElementAsResponse()
        {
            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .Build()
                        .SendGet<JsonElement>()
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Result from jsonElement: {response.GetProperty("data").GetProperty("email")}");
        }

        private static void requestWithStatusCodeAsResult()
        {
            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .Build()
                        .SendGet(disableBodyLogs: true)
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Result status code: {(int)response.StatusCode}");
        }

        private static void requestWithJsonAsObject()
        {
            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .Build()
                        .SendGet<JsonToObject>()
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Result as object: {response.Data.Email}");
        }

        private static void requestWithPostObject()
        {
            var postObj = new ObjToJson { Job = "leader", Name = "morpheus" };

            var response = RestWrapper.CreateWithHttpClient(_client)
                        .Url($"https://reqres.in/api/users/2")
                        .Build()
                        .SendPost<JsonElement, ObjToJson>(postObj)
                        .GetAwaiter().GetResult();
            Console.WriteLine($"\n>>> Result of post object: {response.GetProperty("id")}");
        }
    }
}
