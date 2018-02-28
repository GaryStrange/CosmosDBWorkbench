using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;
using WorkBench.DataAccess;

namespace WorkBench.Logging
{
    public class ResponseProcessor : IResponseProcessor
    {
 
        public Action<ResponseData> LoggingAction;

        public ResponseProcessor(Action<ResponseData> loggingAction)
        {
            this.LoggingAction = loggingAction;
        }

        private ResponseData LogMessage(ResponseData data)
        {
            this.LoggingAction(data);
            return data;
        }

        public DocumentResponse<T> ProcessDocumentResponse<T>(string requestInfo, Task<DocumentResponse<T>> responseTask)
        {
            var response = responseTask.Result;
            this.LogMessage(ResponseDataFactory.FromResponse(requestInfo, response));
            return response;
        }

        public FeedResponse<T> ProcessFeedResponse<T>(string requestInfo, Task<FeedResponse<T>> responseTask)
        {
            var response = responseTask.Result;
            this.LogMessage(ResponseDataFactory.FromResponse(requestInfo, response));
            return response;
        }

        public ResourceResponse<T> ProcessResourceResponse<T>(string requestInfo, Task<ResourceResponse<T>> responseTask) where T : Resource, new()
        {
            var response = responseTask.Result;
            this.LogMessage(ResponseDataFactory.FromResponse(requestInfo, response));
            return response;
        }
    }
}
