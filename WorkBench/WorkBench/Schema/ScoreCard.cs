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
        public int Score { get; set; }

        [JsonProperty]
        public string Round { get; set; }

        [JsonProperty]
        public int Player { get; set; }

        //This is a test
        //This is another test

        //[JsonProperty]
        //public int Player1 { get; set; }

        //[JsonProperty]
        //public int Player2 { get; set; }

        //[JsonProperty]
        //public int Player3 { get; set; }

        //[JsonProperty]
        //public int Player4 { get; set; }

        //[JsonProperty]
        //public int Player5 { get; set; }

        //[JsonProperty]
        //public int Player6 { get; set; }

        //[JsonProperty]
        //public int Player7 { get; set; }

        //[JsonProperty]
        //public int Player8 { get; set; }

        public object PartitionKeyValue => this.Round;

        //object IPartitionedDocument.PartitionKeyValue => throw new NotImplementedException();

        public override string ToString()
        {
            var s = JsonConvert.SerializeObject(this, Formatting.Indented);
            return s;
        }

        public static ScoreCard NewScoreCard()
        {
            return new ScoreCard();
        }
    }
}
