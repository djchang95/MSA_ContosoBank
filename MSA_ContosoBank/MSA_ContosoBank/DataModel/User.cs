﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MSA_ContosoBank.DataModel
{
    [Serializable]
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public string createdAt { get; set; }

        [JsonProperty(PropertyName = "updatedAt")]
        public string updatedAt { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string version { get; set; }

        [JsonProperty(PropertyName = "deleted")]
        public bool deleted { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public double balance { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string userName { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }
    }
}