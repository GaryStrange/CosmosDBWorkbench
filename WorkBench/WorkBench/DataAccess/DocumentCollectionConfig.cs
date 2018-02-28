using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public class DocumentCollectionConfig
    {
        public static int defaultOfferThroughput = 1000;
        public string collectionName;

        public int offerThroughput;

        public IndexingPolicy indexingPolicy;
        public string PartitionKeyPath;
    }
}
