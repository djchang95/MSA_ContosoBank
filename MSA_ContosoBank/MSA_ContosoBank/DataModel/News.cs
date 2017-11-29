using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSA_ContosoBank.DataModel
{
    [Serializable]
    public class News
    {
        public class Rootobject
        {
            [JsonProperty(PropertyName = "status")]
            public string status { get; set; }

            [JsonProperty(PropertyName = "articles")]
            public Article[] articles { get; set; }

        }

        public class Source
        {
            [JsonProperty(PropertyName = "id")]
            public string id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }
        }

        public class Article
        {
            [JsonProperty(PropertyName = "source")]
            public Source source { get; set; }

            [JsonProperty(PropertyName = "author")]
            public string author { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string title { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string description { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string url { get; set; }

            [JsonProperty(PropertyName = "urlToImage")]
            public string urlToImage { get; set; }

            [JsonProperty(PropertyName = "publishedAt")]
            public DateTime publishedAt { get; set; }
            
        }
    }
}