using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public struct DocumentCollectionContext : ICollectionContext
    {
        private DocumentClient _client;
        private CosmosDbClientConfig _config;
        private DocumentCollection _collection;

        public DocumentCollection Collection { get { return _collection; } }
        public DocumentClient Client { get { return _client; } }
        public CosmosDbClientConfig Config { get { return _config; } }

        

        public DocumentCollectionContext(DocumentClient client, CosmosDbClientConfig config)
        {
            _client = client;
            _config = config;
            _collection = null;
            _collection = CreateContextIfNotExists();
        }

        private DocumentCollection CreateContextIfNotExists()
        {
            //CosmosDbHelper.CreateDatabaseIfNotExists(this.Client, this.Config.databaseName)
            //    .Wait();

            var c = CosmosDbHelper.CreateDocumentCollectionIfNotExists(
                this.Client
                , this.Config.databaseName
                , this.Config.collectionConfig
                )
                ;//.Wait();

            return c.Result;
        }

        public Uri CollectionUri
        {
            get
            { return UriFactory.CreateDocumentCollectionUri(Config.databaseName, Config.collectionName); }
        }

        public Uri DocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(this.Config.databaseName, this.Config.collectionConfig.collectionName, documentId);
        }



        public T ProcessResourceResponse<T>(string requestInfo, T response) where T : IResourceResponseBase
        {
            Debug.WriteLine(String.Format("Request: {0}\nRequestCharge: {1}", requestInfo, response.RequestCharge));
            return response;
        }


        public T ProcessFeedResponse<T, K>(T response) where T : IFeedResponse<K>
        {
            throw new NotImplementedException();
        }
    }

}
