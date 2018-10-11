using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WorkBench.DataAccess;
using Microsoft.Azure.Documents;

namespace EventSourcing
{

    public class EventAction
    {
        public enum Operators { Increment, Decrement }
        public String Type { get; set; }

        public Operators Operator { get; set; }

        public String Operand { get; set; }

        public Int32 Value { get; set; }

        public Int32 CalculatedValue { get { return Value * (Operator == Operators.Increment ? 1 : -1); } }

        public static EventAction RandomEventAction()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 50);

            return new EventAction()
            {
                Type = "Register",
                Operator = randomNumber > 0 ? Operators.Decrement : Operators.Increment,
                Value = randomNumber > 0 ? 1 : 5,
                Operand = "Counter"
            };
        }
    }
    public class Event : Document, IPartitionedDocument
    {
        [JsonProperty]
        public String StreamId { get; set; }

        [JsonProperty]
        public EventAction Operation { get; set; }

        [JsonProperty]
        public DateTime EventDateTime { get; set; }

        public object PartitionKeyValue { get { return this.StreamId; } set { this.StreamId = value.ToString(); } }

        public static Event RandomEvent()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 100);

            return new Event()
            {
                StreamId = random.Next(0, 100).ToString(),
                EventDateTime = DateTime.Now,
                Operation = EventAction.RandomEventAction()
            };
        }
        
    }

    
}
