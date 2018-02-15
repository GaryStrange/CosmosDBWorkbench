using System.Collections.Specialized;


namespace WorkBench.DataAccess
{
    public static class DocumentCollectionContextFactory
    {
        public static DocumentCollectionContext CreateCollectionContext(NameValueCollection appSettings, IResponseProcessor responseProcessor)
        {
            return CreateCollectionContext(
                    CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(appSettings),
                    responseProcessor: responseProcessor
                );
        }
        public static DocumentCollectionContext CreateCollectionContext(CosmosDbClientConfig config, IResponseProcessor responseProcessor)
        {
            DocumentCollectionContext context = new DocumentCollectionContext(
                client: CosmosDBFactory.CreateClient(config),
                config: config,
                responseProcessor: responseProcessor
            );

            return context;
        }



    }
}
