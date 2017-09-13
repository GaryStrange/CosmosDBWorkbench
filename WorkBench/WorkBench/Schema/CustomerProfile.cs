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

    public sealed class PricePoint : Document
    {

        public string Store { get; set; }
        public int Style { get; set; }
        public int Sku { get; set; }
        public decimal CurrentPrice { get; set; }
        public String StartUtc { get; set; }
        public override String ToString()
        {
            return String.Format("Record: Id {0}, Store {1}, Style {2}, Price {3} \n Effective Date {4}", this.Id, this.Store, this.Style, this.CurrentPrice
                , this.StartUtc);
        }


    }
}
