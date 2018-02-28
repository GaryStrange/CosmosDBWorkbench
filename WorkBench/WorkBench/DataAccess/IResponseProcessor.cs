using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public interface IResponseProcessor
    {
        FeedResponse<T> ProcessFeedResponse<T>(string requestInfo, Task<FeedResponse<T>> responseTask);
        ResourceResponse<T> ProcessResourceResponse<T>(string requestInfo, Task<ResourceResponse<T>> responseTask) where T : Resource, new();
        DocumentResponse<T> ProcessDocumentResponse<T>(string requestInfo, Task<DocumentResponse<T>> responseTask);
    }
}
