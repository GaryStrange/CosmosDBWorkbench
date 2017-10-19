using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public interface ICollectionContext
    {
        DocumentClient Client { get; }
        Uri CollectionUri { get; }
        Uri DocumentUri(string documentId);

        ResourceResponse<Document> ProcessResourceResponse(string requestInfo, Task<ResourceResponse<Document>> response);
        //T ProcessResourceResponse<T>(string requestInfo, Task<T> response) where T : IResourceResponse<Document>;
        //IResourceResponseBase ProcessResourceResponse(string requestInfo, Task<IResourceResponseBase> response);
        T ProcessFeedResponse<T, K>(T response) where T : IFeedResponse<K>;

        DocumentResponse<T> ProcessDocumentResponse<T>(string requestInfo, Task<DocumentResponse<T>> response);

        //Task<TNewResult> Execute<TNewResult>(Func<Task<TNewResult>, TNewResult> function);
    }
}
