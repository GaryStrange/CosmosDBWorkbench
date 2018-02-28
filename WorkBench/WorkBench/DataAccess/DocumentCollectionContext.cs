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
    public class DocumentCollectionContext : ICollectionContext
    {
        private DocumentClient _client;
        private CosmosDbClientConfig _config;
        private DocumentCollection _collection;

        public DocumentCollection Collection { get { return _collection; } }
        public DocumentClient Client { get { return _client; } }
        public CosmosDbClientConfig Config { get { return _config; } }

        public IResponseProcessor ResponseProcessor { get; private set; }

        public DocumentCollectionContext(DocumentClient client, CosmosDbClientConfig config, IResponseProcessor responseProcessor)
        {
            _client = client;
            _config = config;
            _collection = null;
            _collection = CreateContextIfNotExists();
            ResponseProcessor = responseProcessor;
        }

        public DocumentCollectionContext RefreshClient()
        {
            this._client = CosmosDBFactory.CreateClient(this._config);
            return this;
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


        public async Task<ResourceResponse<Document>> Execute<TNewResult>(Func<Task<ResourceResponse<Document>>> function, string requestInfo)
        {
            var t = function();
            var context = this;
            return await t
                .ContinueWith(tsk => context.ResponseProcessor.ProcessResourceResponse(
                    requestInfo
                    , tsk)
                 );
        }







        //public T ProcessFeedResponse<T, K>(T response) where T : IFeedResponse<K>
        //{
        //    throw new NotImplementedException();
        //}

        ~DocumentCollectionContext()
        {
            CosmosDbHelper.DeleteCollectionIfExistsAsync(this).Wait();
        }

    }

}
