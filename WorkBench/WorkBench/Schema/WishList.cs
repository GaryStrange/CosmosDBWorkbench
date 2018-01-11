using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WorkBench.DataAccess;
using WorkBench.DataAccess.SchemaAttributes;

namespace WorkBench.Schema
{
    public class WishList : Document, IPartitionedDocument
    {
        [JsonProperty]
        public string CustomerId { get; set; }

        [JsonProperty]
        public Wish[] Wishes { get; set; }

        [JsonProperty(PropertyName = "_ts")]
        public int? TTL { get; set; }

        public object PartitionKeyValue => this.CustomerId;
    }

    public class Wish
    {
        [Index(IsIncluded = true, HasEqualtiyQueries = true)]
        public string Name { get; set; }

        public string Size { get; set; }
    }
}
