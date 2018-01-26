using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using WorkBench.DataAccess;

namespace WorkBench.Schema
{
    
    public class ScoreCard : Document, IPartitionedDocument
    {
        [JsonProperty]
        public int Player1 { get; set; }

        [JsonProperty]
        public int Player2 { get; set; }

        [JsonProperty]
        public int Player3 { get; set; }

        [JsonProperty]
        public int Player4 { get; set; }

        [JsonProperty]
        public int Player5 { get; set; }

        [JsonProperty]
        public int Player6 { get; set; }

        [JsonProperty]
        public int Player7 { get; set; }

        [JsonProperty]
        public int Player8 { get; set; }

        public object PartitionKeyValue => this.Id;

        //object IPartitionedDocument.PartitionKeyValue => throw new NotImplementedException();

        public override string ToString()
        {
            var s = JsonConvert.SerializeObject(this);
            return base.ToString() + s;
        }

        public static ScoreCard NewScoreCard()
        {
            return new ScoreCard();
        }
    }
}
