using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WorkBench.DataAccess;
using WorkBench.Logging;

namespace EventSourcing
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private bool MakeWrites = true;
        private List<Task> writeTasks = new List<Task>();

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("eventsourcing.appsettings.json");

            Configuration = builder.Build();


            IConfigurationSection section = Configuration.GetSection("ForEventSourcing");



            var config = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                section["EndPointUrl"],
                section["AuthorizationKey"],
                section["DatabaseName"],
                section["CollectionName"],
                section["PartitionKeyPath"],
                section["ConsistencyLevel"]
                );

            

            var primaryContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                    , new ResponseProcessor((s) => Console.WriteLine(s))
                );

            section = Configuration.GetSection("ForEventSourcingSnapshot");

            config = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                section["EndPointUrl"],
                section["AuthorizationKey"],
                section["DatabaseName"],
                section["CollectionName"],
                section["PartitionKeyPath"],
                section["ConsistencyLevel"]
                );

            var snapShotContext =
    DocumentCollectionContextFactory.CreateCollectionContext(
        config
        , new ResponseProcessor((s) => Console.WriteLine(s))
    );

            var e = Event.RandomEvent();

            var r = CosmosDbHelper.UpsertObjectAsync(snapShotContext, new
                {
                    id = "1",
                    incrementRate = 0.0,
                    decrementRate = 0.0,
                    counter = 1000000
                }, null);

            var t = CosmosDbHelper.CreateDocumentAsync(primaryContext, Event.RandomEvent());
            t.Wait();
            var program = new Program();
            var writing = program.WriteDocumentsAsync(() => CosmosDbHelper.CreateDocumentAsync(primaryContext, Event.RandomEvent()));
            var writing2 = program.WriteDocumentsAsync(() => CosmosDbHelper.CreateDocumentAsync(primaryContext, Event.RandomEvent()));
            writing.Wait();
            writing2.Wait();
        }


        private async Task WriteDocumentsAsync<T>( Func<Task<T>> func)//, ScoreCard p, int loops, Action<ScoreCard, int> updateScore = null)
        {
            while (this.MakeWrites)
            {
                await func();
            }
            
            //Func<int, bool> loopCheck;
            //if (loops == -1) loopCheck = (i) => true;
            //else loopCheck = (i) => i < loops;

            //for (int i = 0;
            //    loopCheck(i) &&
            //    this.MakeWrites
            //    ; i++)
            //{
            //    //Console.WriteLine("Upsert");
            //    //updateScore(p, i);
            //    p.Score = i;
            //    //var x = await CosmosDbHelper.UpsertDocumentAsync(primaryContext, p);
            //    //if (this.delayMs > 0) await Task.Delay(this.delayMs);
            //    //p = x;

            //}

        }
    }
}
