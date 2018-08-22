using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WorkBench.DataAccess;
using WorkBench.Schema;
using WorkBench.Security;
using System.Linq;
using WorkBench.Logging;
using WorkBench.Configuration;

namespace WorkBench
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private bool ContinousRead = true;
        private bool MakeWrites = true;
        private List<Task> writeTasks = new List<Task>();
        private int delayMs = 50;

        private DocumentCollectionContext primaryContext;
        private Guid g;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();


            IConfigurationSection section = Configuration.GetSection("ForScoreCard");



            var config = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                section["EndPointUrl"],
                section["AuthorizationKey"],
                section["DatabaseName"],
                section["CollectionName"],
                section["PartitionKeyPath"],
                section["ConsistencyLevel"]
                );

            var program = new Program();
            Console.WriteLine("Key 1 for Eventual Consistency Test.");
            Console.WriteLine("Key 2 for Read Scale Test.");
            Console.WriteLine("Key 3 for Saved Items Wishlist Document Test.");
            Console.WriteLine("Key 4 for Saved Items 500 item test.");
            Console.WriteLine("Key 5 for Cross Partition test.");
            ConsoleKeyInfo cki;
            cki = Console.ReadKey();
            switch (cki.Key)
            {
                case ConsoleKey.D1: program.EventualConsistencyTest(config); break;
                case ConsoleKey.D2:
                    {
                        program.ScaleTest(config);

                        Console.WriteLine("Press Esc to Exit.");
                        do
                        {
                            cki = Console.ReadKey();
                            Console.Write(" --- You pressed ");
                            if (cki.Key == ConsoleKey.DownArrow) program.delayMs += 10;
                            if (cki.Key == ConsoleKey.UpArrow && program.delayMs != 0)
                                program.delayMs -= 10;
                            //if (cki.Key == ConsoleKey.RightArrow) program.ScaleOut();
                            Console.WriteLine(cki.Key.ToString());
                        } while (cki.Key != ConsoleKey.Escape);
                        program.MakeWrites = false;
                    }; break;
                case ConsoleKey.D3: program.SavedItemsTest(Configuration); break;
                case ConsoleKey.D4: program.SavedItems500Test(Configuration); break;
                case ConsoleKey.D5: program.CrossPartitionTest(Configuration); break;
            }
            Console.WriteLine();
            Console.WriteLine("End of program.");
            Console.ReadKey();
            return;
            var masterKeyContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                    , new ResponseProcessor( (s) => Debug.WriteLine(s) )
                );
            //SecurityTest(masterKeyContext);

            WishList list = CreateWishList("Gift List");

            list = CosmosDbHelper.CreateDocument(masterKeyContext, list);

            //Console.WriteLine("Wish Size = {0}", list.Wishes[0].Size);
            Console.WriteLine("_ts = {0}", list.TTL);

            //list.Wishes[0].Size = "Medium";
            list = CosmosDbHelper.UpsertDocument(masterKeyContext, list);

            //Console.WriteLine("Wish Size = {0}", list.Wishes[0].Size);
            Console.WriteLine("_ts after document update = {0}", list.TTL);

            var predicates = new System.Collections.Specialized.NameValueCollection() { { "_ts", list.TTL.ToString() } };
            var response = CosmosDbHelper.RequestDocumentByEquality<WishList>(masterKeyContext, null, predicates);
            //var list2 = response.AsEnumerable();
            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }

        private void CrossPartitionTest(IConfigurationRoot configuration)
        {
            var masterKeyContext = this.CreateContextFromAppConfig(configuration, "ForSavedItemsConfig");

            var predicates = new System.Collections.Specialized.NameValueCollection() { { "customerId", "ee6487d7-4639-4a0e-95f9-c2bcb227b13a" } };
            var response = CosmosDbHelper.RequestDocumentByEquality<WishList>(masterKeyContext, null, predicates);

            response.Wait();
        }

        private DocumentCollectionContext CreateContextFromAppConfig(IConfigurationRoot config, string sectionName)
        {
            var section = Configuration.GetSection(sectionName);

            section.PrintConfigurationValues(Console.WriteLine);

            var cosmosConfig = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                section["EndPointUrl"],
                section["AuthorizationKey"],
                section["DatabaseName"],
                section["CollectionName"],
                section["PartitionKeyPath"],
                section["ConsistencyLevel"]
                );

            return
                DocumentCollectionContextFactory.CreateCollectionContext(
                    cosmosConfig
                    , new ResponseProcessor((s) => Debug.WriteLine(s))
                );
        }
        public void SavedItems500Test(IConfigurationRoot config)
        {
            var masterKeyContext = this.CreateContextFromAppConfig(config, "ForSavedItemsConfig");

            CustomerLifecycleStatus status = new CustomerLifecycleStatus() { Status = "AddToList" };
            string CustomerId = Guid.NewGuid().ToString();

            List<Task<CustomerSavedItem>> tasks = new List<Task<CustomerSavedItem>>();
            for (int i = 0; i < 500; i++)
            {
                tasks.Add(
                CosmosDbHelper.CreateDocumentAsync(masterKeyContext,
                    new CustomerSavedItem(
                        new List<CustomerLifecycleStatus>() { status.Touch() },
                        Guid.NewGuid().ToString()
                        )
                    { CustomerId = CustomerId,
                    Uuid = CustomerId}
                        )
                        );

            }
            Task.WaitAll(tasks.ToArray());

            status = new CustomerLifecycleStatus() { Status = "AddToWishList" };

            List<Task<CustomerSavedItem>> addToWishListtasks = new List<Task<CustomerSavedItem>>();
            tasks.ForEach(t =>
            {
                var doc = t.Result;
                doc.Tags = new string[1] { "Birthday List" };
                doc.LifecycleStatus.Add(status);
                addToWishListtasks.Add(
                    CosmosDbHelper.UpsertDocumentAsync(masterKeyContext, t.Result)
                    );
                    }
                    );

            Task.WaitAll(addToWishListtasks.ToArray());

        }

        
        public void SavedItemsTest(IConfigurationRoot config)
        {
            var masterKeyContext = this.CreateContextFromAppConfig(config, "ForSavedItemsConfig");

            WishList wList = CosmosDbHelper.CreateDocument(masterKeyContext, Program.CreateWishList("Party List"));
            CosmosDbHelper.ReadDocumentAsync(masterKeyContext, wList).Wait();
            CosmosDbHelper.GetDocument<WishList>(masterKeyContext, wList.Id, wList.PartitionKeyValue);
            //var newList = Program.CreateWishList("Party List");
            //newList.Id = wList.Id;
            //newList.PartitionKeyValue = wList.PartitionKeyValue;
            wList.Wishes.Remove(wList.Wishes[0]);
            for (int i = 0; i < 20; i++)
            {
                wList.Wishes[i].ImageUrl = "images.asos-media.com/products/jack-jones-dark-blue-washed-jeans-in-loose-fit-with-engineered-details/6737719-1-darkblue";
                wList.Wishes[i].SavedItemId = Guid.NewGuid().ToString();
            }
            wList.Wishes.Add(new Wish() { SavedItemId = Guid.NewGuid().ToString(), CreatedDate = DateTime.Now });
            
            CosmosDbHelper.UpsertDocumentAsync(masterKeyContext, wList).Wait();

        }
        private static WishList CreateWishList(string name)
        {

            List<Wish> wishes = new List<Wish>(500);
            for (int i = 0; i < 500; i++)
            {
                wishes.Add( new Wish()
                {
                    SavedItemId = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.Now//,
                    //ImageUrl = "images.asos-media.com/products/jack-jones-dark-blue-washed-jeans-in-loose-fit-with-engineered-details/6737719-1-darkblue"
                });
            }

            return new WishList()
            {
                CustomerId = Guid.NewGuid().ToString(),
                Wishes = wishes,
                Name = name

            };
        }

        private static void QueryOrderReturnsTest(CosmosDbClientConfig config)
        {
            var primaryContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                    , new ResponseProcessor((s) => Debug.WriteLine(s))
                );

            //var predicates = new System.Collections.Specialized.NameValueCollection() { { "customerId", 135931325 } };
            //CosmosDbHelper.RequestDocumentByEquality<ReturnBookingCommand>(primaryContext, 135931325, predicates);
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
                    , new ResponseProcessor( (s) => Debug.WriteLine(s) )
                );



            var customerProfileMaster = CosmosDbHelper.ReadDocument<CustomerProfile>(masterKeyContext, customerProfile.Id, customerProfile.PartitionKeyValue);
            customerProfileMaster.Wait();

            customerProfile = customerProfileMaster.Result;
            customerProfile.userid = "gary.strange3";
            customerProfile = CosmosDbHelper.UpsertDocument(masterKeyContext, customerProfile);

            var responseTask = CosmosDbHelper.ReadDocument<CustomerProfile>(readOnlyContext, customerProfile.Id, customerProfile.PartitionKeyValue);
            responseTask.Wait();

            //customerProfileRead = (CustomerProfile)responseTask.Result;


            var customerProfileRead = customerProfileMaster.Result;

            //var customerProfileRead = CosmosDbHelper.ReadDocument<CustomerProfile>(readOnlyContext, customerProfile.Id, customerProfile.PartitionKeyValue);

            // Fails with expected 403 status code
            customerProfile = CosmosDbHelper.CreateDocument(readOnlyContext, customerProfile);

            Console.ReadKey();
        }

        private void ScaleOut(List<Task> writeTasks, DocumentCollectionContext context, int loops = 300)
        {
            writeTasks.Add(WriteDocumentsAsync(context
                , new ScoreCard()
                {
                    Id = (writeTasks.Count() + 1).ToString(),
                    Round = g.ToString(),
                    Player = writeTasks.Count() + 1,
                }
                , loops
                ));
        }

        public async void ScaleTest(CosmosDbClientConfig config)
        {
            var primaryContext =
               DocumentCollectionContextFactory.CreateCollectionContext(
                   config
                   , new ResponseProcessor((s) => Console.WriteLine(s))
               );

            g = Guid.NewGuid();

            var writeTasks = new List<Task>();
            this.ScaleOut(writeTasks, primaryContext, -1);
            this.ScaleOut(writeTasks, primaryContext, -1);
            await Task.WhenAll(writeTasks.ToArray());
        }
        public async void EventualConsistencyTest(CosmosDbClientConfig config)
        { 
             var primaryContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                    , new ResponseProcessor( (s) => Console.WriteLine(s) )
                );
            var secondaryContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                    , new ResponseProcessor((s) => Console.WriteLine(s))
                );
            Console.WriteLine("Client consistency level = {0}", primaryContext.Client.ConsistencyLevel);


            var writeTasks = new List<Task>();
            var readTasks = new List<Task>();
            var p = ScoreCard.NewScoreCard();
            Console.WriteLine(p);
            g = Guid.NewGuid();
            Debug.WriteLine(g);
            Console.ReadKey();
                p.Id = g.ToString();

                int loops = 300;
            var p2 = CosmosDbHelper.CreateDocumentAsync(primaryContext, p);
            p.Id = Guid.NewGuid().ToString();
            p2 = CosmosDbHelper.CreateDocumentAsync(primaryContext, p);
            //    Console.WriteLine(p.ToString());
            p2.Wait();
                readTasks.Add(this.ReadDocumentsAsync(primaryContext, g));
            readTasks.Add(this.ReadDocumentsAsync(secondaryContext, g));
            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g));
            readTasks.Add(this.ReadDocumentsAsync(secondaryContext, g));

            readTasks.Add(this.ReadDocumentsAsync(primaryContext, g));
            ////readTasks.Add(this.ReadDocumentsAsync(primaryContext, g));
            ////readTasks.Add(this.ReadDocumentsAsync(primaryContext, g));
            ////readTasks.Add(this.ReadDocumentsAsync(primaryContext, g));
            ////readTasks.Add(this.ReadDocumentsAsync(primaryContext, g));


            this.ScaleOut(writeTasks, primaryContext);
            this.ScaleOut(writeTasks, secondaryContext);
            this.ScaleOut(writeTasks, primaryContext);
            this.ScaleOut(writeTasks, secondaryContext);

    //        writeTasks.Add(WriteDocumentsAsync(primaryContext
    //, new ScoreCard()
    //{
    //    Id = (writeTasks.Count() + 1).ToString(),
    //    Round = g.ToString(),
    //    Player = writeTasks.Count() + 1,
    //}
    //, loops
    //));
    //        writeTasks.Add(WriteDocumentsAsync(secondaryContext
    //, new ScoreCard()
    //{
    //    Id = (writeTasks.Count() + 1).ToString(),
    //    Round = g.ToString(),
    //    Player = writeTasks.Count() + 1,
    //}
    //, loops
    //));
    //        writeTasks.Add(WriteDocumentsAsync(primaryContext
    //, new ScoreCard()
    //{
    //    Id = (writeTasks.Count() + 1).ToString(),
    //    Round = g.ToString(),
    //    Player = writeTasks.Count() + 1,
    //}
    //, loops
    //));
    //        writeTasks.Add(WriteDocumentsAsync(secondaryContext
    //, new ScoreCard()
    //{
    //    Id = (writeTasks.Count() + 1).ToString(),
    //    Round = g.ToString(),
    //    Player = writeTasks.Count() + 1,
    //}
    //, loops
    //));

            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player1 = score )
            //);
            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player2 = score)
            //);
            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player3 = score)
            //);
            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player4 = score)
            //);
            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player5 = score)
            //);
            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player6 = score)
            //);
            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player7 = score)
            //);
            //writeTasks.Add(WriteDocumentsAsync(primaryContext, p, loops,
            //    (card, score) => card.Player8 = score)
            //);
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

            //await Task.WhenAll(writeTasks.ToArray());

            Task.WaitAll(writeTasks.ToArray());
            Console.WriteLine("Writes finished.");
            ContinousRead = false;
            Task.WaitAll(readTasks.ToArray());
        }

        private async Task WriteDocumentsAsync(DocumentCollectionContext primaryContext, ScoreCard p, int loops, Action<ScoreCard, int> updateScore = null)
        {
            Func<int, bool> loopCheck;
            if (loops == -1) loopCheck = (i) => true;
            else loopCheck = (i) => i < loops;

            for (int i = 0; 
                loopCheck(i) && 
                this.MakeWrites
                ; i++)
            {
                //Console.WriteLine("Upsert");
                //updateScore(p, i);
                p.Score = i;
                var x = await CosmosDbHelper.UpsertDocumentAsync(primaryContext, p);
                if (this.delayMs> 0) await Task.Delay(this.delayMs);
                p = x;

            }

        }

        private async Task ReadDocumentsAsync(DocumentCollectionContext context, Guid g, int loops = 4000)
        {
            //Console.WriteLine("Start reading {0}", DateTime.UtcNow);
            var prevP = ScoreCard.NewScoreCard();
            int loop_num = 0;

            //Action<ScoreCard, ScoreCard> consistencyCheck = (t1, t2) =>
            //{
            //    if (t2.Player1 < t1.Player1 ||
            //        t2.Player2 < t1.Player2 ||
            //        t2.Player3 < t1.Player3 ||
            //        t2.Player4 < t1.Player4 ||
            //        t2.Player5 < t1.Player5 ||
            //        t2.Player6 < t1.Player6 ||
            //        t2.Player7 < t1.Player7 ||
            //        t2.Player8 < t1.Player8)
            //    {
            //        //this.MakeWrites = false; //stop the write process.
            //        throw new Exception(
            //            String.Format("Inconsistent data read! \n T2: {0}\n T1: {1}", t2,t1)
            //            );
            //    }
            //};
            Microsoft.Azure.Documents.Client.FeedResponse<ScoreCard> prev_r = new Microsoft.Azure.Documents.Client.FeedResponse<ScoreCard>();
            List<ScoreCard> prev_c = new List<ScoreCard>();
            var predicates = new System.Collections.Specialized.NameValueCollection() { { "Round", g.ToString() } };
            while (ContinousRead)
            {
                //context.RefreshClient();
                //Console.WriteLine("Read");

                
                var r = await CosmosDbHelper.RequestDocumentByEquality<ScoreCard>(context, g.ToString(), predicates);
                var c = r.ToList();

                var r2 = await CosmosDbHelper.RequestDocumentByEquality<ScoreCard>(context, g.ToString(), predicates);
                var c2 = r2.ToList();

                for (int i = 0; i < prev_c.Count; i++)
                {
                    if (prev_c[i].Player != c[i].Player)
                        throw new Exception("Unexpected player.");
                    if (prev_c[i].Round != c[i].Round)
                        throw new Exception("Unexpected round.");
                    if (prev_c[i].Score > c[i].Score)
                        throw new Exception(String.Format("Inconsistent data read! \n Session Token: {2} \n T2: {0}\n SessionToken: {3}\n T1: {1}", c[i], prev_c[i], r.SessionToken, prev_r.SessionToken ));
                }

                for (int i = 0; i < c.Count; i++)
                {
                    if (c[i].Player != c2[i].Player)
                        throw new Exception("Unexpected player.");
                    if (c[i].Round != c2[i].Round)
                        throw new Exception("Unexpected round.");
                    if (c[i].Score > c2[i].Score)
                        //throw new Exception(String.Format("Inconsistent data read! \n T2: {0}\n T1: {1}", c2[i], c[i]));
                    throw new Exception(String.Format("Inconsistent data read! \n Session Token: {2} \n T2: {0}\n SessionToken: {3}\n T1: {1}", c2[i], c[i], r2.SessionToken, r.SessionToken));
                }

                prev_c = c2;
                prev_r = r2;
                //context.RefreshClient();
                //var tsk1 = CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());
                //tsk1.Wait();

                //var chance1 = tsk1.Result;

                //var chance2 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());
                //var chance3 = chance2;
                //var chance3 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());

                //var chance1 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());
                //var chance2 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());
                //var chance3 = await CosmosDbHelper.ReadDocument<ScoreCard>(context, g.ToString(), g.ToString());



                //await Task.Delay(TimeSpan.FromMilliseconds(5));
                //var newP = (ScoreCard)chance1;
                //Console.Write(newP.Timestamp.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + ",");
                //consistencyCheck((ScoreCard)chance1, (ScoreCard)chance2);
                //consistencyCheck((ScoreCard)chance2, (ScoreCard)chance3);

                //readTasks.Add(x);
                if (loop_num == loops) break;
                loop_num++;
                //await Task.Delay(this.delayMs);
            }
            //Console.WriteLine("Stop reading {0}", DateTime.UtcNow);
        }
    }
}