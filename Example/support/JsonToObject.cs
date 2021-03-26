using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Example.support
{
    class JsonToObject
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        [JsonPropertyName("support")]
        public Support Support { get; set; }
    }

    public class Support
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
    }
}
