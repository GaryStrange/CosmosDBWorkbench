using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents;
using WorkBench.DataAccess;

namespace WorkBench.Schema
{
    //[JsonConverter(typeof(CustomerLifecycleStatusConverter))]
    public class CustomerLifecycleStatus
    {
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }

        public CustomerLifecycleStatus Touch()
        {
            this.Timestamp = DateTime.Now;
            return this;
        }
    }

    public class CustomerSavedItem : Document, IPartitionedDocument
    {
        private IList<CustomerLifecycleStatus> lifecycleStatus;

        public CustomerSavedItem()
        {
            lifecycleStatus = new List<CustomerLifecycleStatus>();
        }

        public CustomerSavedItem(IList<CustomerLifecycleStatus> lifecycleStatus,
            string id,
            string eTag = null,
            DateTime documentLastUpdatedDateTime = default(DateTime))
        {
            this.lifecycleStatus = lifecycleStatus;
        }

        [JsonProperty(PropertyName = "uuId")]
        public string Uuid { get; set; }

        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "isAnonymous")]
        public bool IsAnonymous { get; set; }

        public int ProductId { get; set; }

        public int? VariantId { get; set; }

        public string ColourCode { get; set; }

        public string ColourwayId { get; set; }

        public bool IsDeleted { get; set; }

        [JsonProperty]
        public IList<CustomerLifecycleStatus> LifecycleStatus
        {
            get => lifecycleStatus;
            private set => lifecycleStatus = value;
        }

        [JsonProperty]
        public string[] Tags { get; set; }

        public int BasketSavedItemId { get; set; }

        public object PartitionKeyValue => this.Uuid;

        public void AddLifecycleStatus(string status)
        {
            var customerLifecycleStatus = new CustomerLifecycleStatus { Status = status, Timestamp = DateTime.UtcNow };
            lifecycleStatus.Add(customerLifecycleStatus);
        }

        public void SetLifecycleStatus(IList<CustomerLifecycleStatus> lifecycleStatuses)
        {
            lifecycleStatus = lifecycleStatuses;
        }
    }
}
