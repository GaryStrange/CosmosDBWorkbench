using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public class DocumentCollectionConfig : IValidate<DocumentCollectionConfig>
    {
        public static int MinimumOfferThroughput = 400;
        public string collectionName;
        public int offerThroughput = DocumentCollectionConfig.MinimumOfferThroughput;
        public IndexingPolicy indexingPolicy;
        public string PartitionKeyPath;


        public DocumentCollectionConfig Validate()
        {
            List<Exception> innerExceptions = new List<Exception>();

            if (this.offerThroughput < DocumentCollectionConfig.MinimumOfferThroughput)
                innerExceptions.Add(new ArgumentOutOfRangeException("offerThroughput", "OfferThroughput too small."));
            if (this.collectionName is null)
                innerExceptions.Add(new ArgumentNullException("collectionName", "collectionName can not be null."));
            if (this.PartitionKeyPath is null)
                innerExceptions.Add(new ArgumentNullException("PartitionKeyPath", "PartitionKeyPath can not be null."));

            if (innerExceptions.Count > 0)
                throw new AggregateException("DocumentCollectionConfig failed validation", innerExceptions);

            return this;
        }
    }
}
