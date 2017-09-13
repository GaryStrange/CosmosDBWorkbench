using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WorkBench.DataAccess;
using WorkBench.Schema;
using WorkBench.Security;

namespace WorkBench
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }



        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            Action<string> print = (string s) => { Console.WriteLine("{0} = {1}", s,  Configuration[s]); };
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

            var masterKeyContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                );

            CustomerProfile customerProfile = new CustomerProfile()
            {
                userid = "gary.strange2"
            };

            customerProfile = CosmosDbHelper.CreateDocument(masterKeyContext, customerProfile);

            var u = CosmosDBSecurityHelper.CreateUserIfNotExistAsync(masterKeyContext, "gary.strange2");

            u.Wait();


            var p = CosmosDBSecurityHelper.GrantUserPermissionAsync(masterKeyContext, u.Result);
            p.Wait();

            Console.WriteLine("Using read context get user permissions");
            //var perms = CosmosDBSecurityHelper.GetUserPermissions(testContext.Client, u.Result);
            //foreach (var perm in perms)
            //{
            //    Console.WriteLine(perm.ToString());

            //}


            Debug.WriteLine(string.Format("Token: {0}", p.Result.Resource.Token));

            var configToken = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                Configuration["EndPointUrl"],
                p.Result.Resource.Token,
                Configuration["DatabaseName"],
                Configuration["CollectionName"],
                Configuration["PartitionKeyPath"]
                );

            var readOnlyContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    configToken
                );


            CustomerProfile customerProfileRead;
            customerProfileRead = CosmosDbHelper.ReadDocument<CustomerProfile>(masterKeyContext, customerProfile.Id, customerProfile.PartitionKeyValue);
            customerProfileRead = CosmosDbHelper.ReadDocument<CustomerProfile>(readOnlyContext, customerProfile.Id, customerProfile.PartitionKeyValue);

            // Fails with expected 403 status code
            customerProfile = CosmosDbHelper.CreateDocument(readOnlyContext, customerProfile);

            Console.ReadKey();
        }



    }
}