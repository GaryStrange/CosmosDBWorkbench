using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WorkBench.DataAccess;
using WorkBench.Schema;
using WorkBench.Security;
using System.Linq;

namespace WorkBench
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private bool ContinousRead = true;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            Action<string> print = (string s) => { Console.WriteLine("{0} = {1}", s, Configuration[s]); };
            print("EndPointUrl");
            print("DatabaseName");
            print("CollectionName");
            print("PartitionKeyPath");

            var config = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                Configuration["EndPointUrl"],
                Configuration["AuthorizationKey"],
                Configuration["DatabaseName"],
                Configuration["CollectionName"],
                Configuration["PartitionKeyPath"]
                );

            var program = new Program();
            program.EventualConsistencyTest(config);
            Console.WriteLine("Press any key.");
            Console.ReadKey();
            return;
            var masterKeyContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                );
            //SecurityTest(masterKeyContext);

            WishList list = CreateWishList();

            list = CosmosDbHelper.CreateDocument(masterKeyContext, list);

            Console.WriteLine("Wish Size = {0}", list.Wishes[0].Size);
            Console.WriteLine("_ts = {0}", list.TTL);

            list.Wishes[0].Size = "Medium";
            list = CosmosDbHelper.UpsertDocument(masterKeyContext, list);

            Console.WriteLine("Wish Size = {0}", list.Wishes[0].Size);
            Console.WriteLine("_ts after document update = {0}", list.TTL);

            var predicates = new System.Collections.Specialized.NameValueCollection() { { "_ts", list.TTL.ToString() } };
            var response = CosmosDbHelper.RequestDocumentByEquality<WishList>(masterKeyContext, null, predicates);
            var list2 = response.AsEnumerable();
            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }

        private static WishList CreateWishList()
        {
            return new WishList()
            {
                CustomerId = Guid.NewGuid().ToString(),
                Wishes = new Wish[1]
                {
                    new Wish() { Name = "Wish upon a star", Size = "Big" }
                }

            };
        }

        private static void SecurityTest(DocumentCollectionContext masterKeyContext)
        {
            CustomerProfile customerProfile = new CustomerProfile()
            {
                userid = "gary.strange2"
                            ,
                emailHash = "gary.strange@asos.com"
                            ,
                email = "gary.strange@asos.com"
            };

            customerProfile = CosmosDbHelper.CreateDocument(masterKeyContext, customerProfile);

            var u = CosmosDBSecurityHelper.CreateUserIfNotExistAsync(masterKeyContext, "gary.strange2");

            u.Wait();


            //var p = CosmosDBSecurityHelper.GrantUserPermissionAsync(masterKeyContext, u.Result);
            //p.Wait();
            Console.WriteLine("Using master context get user permissions");
            var perms = CosmosDBSecurityHelper.GetUserPermissions(masterKeyContext.Client, u.Result);
            Microsoft.Azure.Documents.Permission p = null;
            foreach (var perm in perms)
            {
                p = perm;
                Console.WriteLine(perm.ToString());

            }

            


            Debug.WriteLine(string.Format("Token: {0}", p.Token));
            //Token: type=resource&ver=1&sig=78cIhp9WoTZIE2gR3UNitA==;uBBdPh7ittGwaFqW2Rh3Xeioadz492/WIT1EffjBV5xDaKgAkIHh1pnZSwrHuO3tXI8JFGRnoK+ZRWXWUzgRnRgDpWCwOS8sCx5S+aFCmL30UhKfdSrpTzYjRkfXRbRS6IgiPOP8A/S6QRqAvNa7Qlc7hoZQKL1SlE9OGzWugwh9dAQAn/dqmzXDgIJhmmFO9g8JaosfTK2pQke83VPHO6ipKpY+/tOp+lEgI9RyfPg5JjFjHEergLGrVVp2X0RN;
            //Token: type=resource&ver=1&sig=3nqgPei+Mkil1hIWOaq7qg==;5xp+9V+zF8kQ63qnI79DG1k2gPAxLd7TiHN2K92e6/heehBVg9mSlHUVuGQhnGJoOSLU+e1hkpCb/Aer++NfEsfDn/KM4Lfr9HLBC/R4T1KdwXvqWadFAbVlaIWLaTD5I4XKWjbxKw4OzosqLOUCgeEbSSvuBDO+KcNxD+i8JFWNSW5PE7CUWGFLZy2G/v1sNAQ5KwUfDvJvdVBrqNKJKzz2pDXA3CU6hMS0bYT8DJnF7tAqUsPY9fq6K2E6bonJ;

            var configToken = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                Configuration["EndPointUrl"],
                p.Token,
                Configuration["DatabaseName"],
                Configuration["CollectionName"],
                Configuration["PartitionKeyPath"]
                );

            var readOnlyContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    configToken
                );



            var customerProfileMaster = CosmosDbHelper.ReadDocument<CustomerProfile>(masterKeyContext, customerProfile.Id, customerProfile.PartitionKeyValue);
            customerProfileMaster.Wait();

            customerProfile = customerProfileMaster.Result;
            customerProfile.userid = "gary.strange3";
            customerProfile = CosmosDbHelper.UpsertDocument(masterKeyContext, customerProfile);

            var responseTask = CosmosDbHelper.ReadDocument(readOnlyContext, customerProfile.Id, customerProfile.PartitionKeyValue);
            responseTask.Wait();

            //customerProfileRead = (CustomerProfile)responseTask.Result;


            customerProfileRead = customerProfileMaster.Result;

            //var customerProfileRead = CosmosDbHelper.ReadDocument<CustomerProfile>(readOnlyContext, customerProfile.Id, customerProfile.PartitionKeyValue);

            // Fails with expected 403 status code
            customerProfile = CosmosDbHelper.CreateDocument(readOnlyContext, customerProfile);

            Console.ReadKey();
        }

        public void EventualConsistencyTest(CosmosDbClientConfig config)
        {
            var primaryContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                );

            var secondaryContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                );

            var writeTasks = new List<Task>();
            var readTasks = new List<Task>();
            var p = ScoreCard.NewScoreCard();
            Console.WriteLine(p);
            var g = Guid.NewGuid();
            Console.WriteLine(g);
            p.Id = g.ToString();

            int loops = 1000;
            p = CosmosDbHelper.CreateDocument(primaryContext, p);
            Console.WriteLine(p.ToString());
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));

            //writeTasks.Add(this.ReadDocumentsAsync(primaryContext, g, 4000));

            //Console.WriteLine("Start writing {0}", DateTime.UtcNow);
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops, 
                (card, score) => card.Player1 = score )
            );
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
    (card, score) => card.Player2 = score)
);
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
    (card, score) => card.Player3 = score)
);
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
    (card, score) => card.Player4 = score)
);
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
    (card, score) => card.Player5 = score)
);
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
    (card, score) => card.Player6 = score)
);
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
    (card, score) => card.Player7 = score)
);
            writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
    (card, score) => card.Player8 = score)
);
            //Console.WriteLine("Stop writing {0}", DateTime.UtcNow);

            //var readTasks = new List<Task>();
            //Promotion prevP = Promotion.NewPromotion();
            //int max_loops = 100;
            //int loop_num = 0;
            //while (prevP.CampaignCategoryId < loops)
            //{


            //    var pp = CosmosDbHelper.ReadDocument<Promotion>(primaryContext, g.ToString(), g.ToString());
            //    var newP = (Promotion)pp.Result;
            //    Console.Write(newP.Timestamp.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",");
            //    if (newP.Timestamp < prevP.Timestamp)
            //        throw new Exception("Stale Read!");

            //    prevP = newP;
            //    //readTasks.Add(x);
            //    if (loop_num == max_loops) break;
            //    loop_num++;
            //}

            Task.WaitAll(writeTasks.ToArray());
            ContinousRead = false;
            Task.WaitAll(readTasks.ToArray());
        }

        private async Task WriteDocumentsAsync(DocumentCollectionContext primaryContext, ScoreCard p, int loops, Action<ScoreCard, int> updateScore)
        {
            for (int i = 0; i < loops; i++)
            {
                //Console.WriteLine("Upsert");
                updateScore(p, i);
                var x = await CosmosDbHelper.UpsertDocumentAsync(primaryContext, p);

            }
        }

        private async Task ReadDocumentsAsync(DocumentCollectionContext context, Guid g, int loops)
        {
            //Console.WriteLine("Start reading {0}", DateTime.UtcNow);
            var prevP = ScoreCard.NewScoreCard();
            int loop_num = 0;

            Action<ScoreCard, ScoreCard> consistencyCheck = (t1, t2) =>
            {
                if (t2.Player1 < t1.Player1 ||
                    t2.Player2 < t1.Player2 ||
                    t2.Player3 < t1.Player3 ||
                    t2.Player4 < t1.Player4 ||
                    t2.Player5 < t1.Player5 ||
                    t2.Player6 < t1.Player6 ||
                    t2.Player7 < t1.Player7 ||
                    t2.Player8 < t1.Player8)
                {
                    throw new Exception(
                        String.Format("Inconsistent data read! \n T2: {0}\n T1: {1}", t2,t1)
                        );
                }
            };

            while (ContinousRead)
            {
                //context.RefreshClient();
                //Console.WriteLine("Read");
                var chance1 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());
                var chance2 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());
                var chance3 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());
                //await Task.Delay(TimeSpan.FromMilliseconds(5));
                var newP = (ScoreCard)chance1;
                //Console.Write(newP.Timestamp.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",");
                consistencyCheck((ScoreCard)chance1, (ScoreCard)chance2);
                consistencyCheck((ScoreCard)chance2, (ScoreCard)chance3);

                //readTasks.Add(x);
                if (loop_num == loops) break;
                loop_num++;
            }
            //Console.WriteLine("Stop reading {0}", DateTime.UtcNow);
        }
    }
}