using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WorkBench.DataAccess.SchemaAttributes;

namespace WorkBench.Schema
{
    public class CustomerProfile : Document
    {
        [JsonProperty(PropertyName = "userid")]
        public string userid;

        private string _email;
        public string email
        {
            get { return _email; }
            set { _email = value; emailHash = HashProperty.sha256(value); }
        }

        [JsonProperty(PropertyName = "emailHash")]
        private string emailHash { get; set; }

        public object PartitionKeyValue => this.userid;
    }

    internal static class HashProperty
    {
        public static string sha256(string propertyValue)
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(propertyValue));
                // Get the hashed string.  
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

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
