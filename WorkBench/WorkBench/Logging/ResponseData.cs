using System;
using System.Text;

namespace WorkBench.Logging
{
    public class ResponseData
    {
        public string RequestInfo;
        public string ActivityId;
        public double RequestCharge;
        public TimeSpan RequestLatency;
        public string RequestDiagnosticsString;
        public string QueryMetrics;

        private double? RequestLatencyTotalMilliseconds
        {
            get
            {
                return RequestLatency.TotalMilliseconds != 0 ?
                  this.RequestLatency.TotalMilliseconds :
                  (double?)null;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder
                .AppendFormat("Request: {0}", this.RequestInfo).AppendLine()
                .AppendFormat("ActivityId: {0}", this.ActivityId).AppendLine()
                .AppendFormat("RequestCharge: {0}", this.RequestCharge).AppendLine();

            Action<string, object> ifNotNullAppend = (propertyFriendlyLabel, property) =>
            {
                if (property != null)
                    stringBuilder
                        .AppendFormat("{0}: {1}", propertyFriendlyLabel, property).AppendLine();
            };

            ifNotNullAppend("Request Latency (ms)", this.RequestLatencyTotalMilliseconds);
            ifNotNullAppend("Request Diagnostics", this.RequestDiagnosticsString);
            ifNotNullAppend("Query Metrics", this.QueryMetrics);

            return stringBuilder.ToString();
        }
    }
}
