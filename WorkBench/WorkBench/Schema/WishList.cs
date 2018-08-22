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
        [JsonProperty(PropertyName = "uuid")]
        public string CustomerId { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public List<Wish> Wishes { get; set; }

        [JsonProperty(PropertyName = "_ts")]
        public int? TTL { get; set; }

        public object PartitionKeyValue { get { return this.CustomerId; } set { this.CustomerId = value.ToString(); } }
    }

    public class Wish
    {
        [Index(IsIncluded = true, HasEqualtiyQueries = true)]
        public string SavedItemId { get; set; }

        [JsonProperty]
        public DateTime CreatedDate { get; set; }

        [JsonProperty]
        public string ImageUrl { get; set; }
    }
}
