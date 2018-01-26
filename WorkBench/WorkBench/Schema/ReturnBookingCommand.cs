using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Text;
using WorkBench.DataAccess;

namespace WorkBench.Schema
{
    public class ReturnBookingCommand : Document, IPartitionedDocument
    {
        public string bodyType { get; set; }

        public int customerId { get; set; }

        public string returnReference { get; set; }

        public string orderReference { get; set; }

        public DateTime timeStampUtc { get; set; }

        public int returnMethodId { get; set; }

        public object PartitionKeyValue => customerId;
    }
}
