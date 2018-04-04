using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace WorkBench.Logging
{
    public static class ResponseDataFactory
    {
        public static ResponseData FromResponse(string requestInfo, ResourceResponseBase response)
        {
            return new ResponseData()
            {
                RequestInfo = requestInfo,
                ActivityId = response.ActivityId,
                RequestCharge = response.RequestCharge,
                RequestLatency = response.RequestLatency,
                RequestDiagnosticsString = response.RequestDiagnosticsString
            };
        }

        public static ResponseData FromResponse<T>(string requestInfo, FeedResponse<T> response)
        {
            return new ResponseData()
            {
                RequestInfo = requestInfo,
                ActivityId = response.ActivityId,
                RequestCharge = response.RequestCharge,
                QueryMetrics = JsonConvert.SerializeObject(response.QueryMetrics, Formatting.Indented)
            };
        }
    }
}
