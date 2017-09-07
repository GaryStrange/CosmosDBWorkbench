using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkBench.Schema
{
    public class CustomerProfile : Document
    {
        [JsonProperty(PropertyName = "userid")]
        public string userid;

        public object PartitionKeyValue => this.userid;
    }
}
