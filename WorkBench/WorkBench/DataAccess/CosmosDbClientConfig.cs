using Microsoft.Azure.Documents.Client;
using System;

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public struct CosmosDbClientConfig : IValidate<CosmosDbClientConfig>
    {
        public string endPointUrl;
        public string authKey;
        public string databaseName;
        public DocumentCollectionConfig collectionConfig;

        public string collectionName { get { return this.collectionConfig.collectionName; } }
        public string PartionKeyField { get { return this.collectionConfig.PartitionKeyPath; } }
        public ConnectionPolicy GetConnectionPolicy()
        {
            return new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
                RequestTimeout = TimeSpan.FromMinutes(2),
                RetryOptions = new RetryOptions
                {
                    MaxRetryAttemptsOnThrottledRequests = 5,
                    MaxRetryWaitTimeInSeconds = 2
                }
            };
        }

        public static CosmosDbClientConfig CreateDocDbConfigFromAppConfig(
                string endPointUrl,
                string authKey,
                string databaseName,
                string collectionName,
                string partitionKeyPath,
                int? offerThroughput = null
            )
        {
            return new CosmosDbClientConfig()
            {
                endPointUrl = endPointUrl,
                authKey = authKey,
                databaseName = databaseName,
                collectionConfig = new DocumentCollectionConfig()
                {
                    collectionName = collectionName,
                    PartitionKeyPath = partitionKeyPath,
                    offerThroughput = offerThroughput ?? DocumentCollectionConfig.defaultOfferThroughput
                }
            }.Validate();
        }

        public static CosmosDbClientConfig CreateDocDbConfigFromAppConfig(NameValueCollection appSettings)
        {
            return CreateDocDbConfigFromAppConfig(
                endPointUrl: appSettings["EndPointUrl"],
                authKey: appSettings["AuthorizationKey"],
                databaseName: appSettings["DatabaseName"],
                collectionName: appSettings["CollectionName"],
                partitionKeyPath: appSettings["PartitionKeyPath"],
                offerThroughput: Int32.Parse( appSettings["OfferThroughput"] )
                )
                .Validate();
        }

        public CosmosDbClientConfig Validate()
        {
            if (this.endPointUrl == null) throw new NullReferenceException("endPointUrl null!");
            if (this.authKey == null) throw new NullReferenceException("authKey null!");
            if (this.databaseName == null) throw new NullReferenceException("databaseName null!");
            if (this.collectionName == null) throw new NullReferenceException("collectionName null!");
            if (this.PartionKeyField == null) throw new NullReferenceException("PartitionKeyField null!");
            return this;
        }
    };
}
