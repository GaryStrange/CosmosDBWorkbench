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

            //Action<int> print = (int x) => { Console.WriteLine(x); };
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

            var testContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    config
                );

            CustomerProfile customerProfile = new CustomerProfile()
            {
                userid = "gary.strange2"
            };

            customerProfile = CosmosDbHelper.CreateDocument(testContext, customerProfile);

            //var u = CosmosDBSecurityHelper.CreateUserIfNotExistAsync(testContext.Client, testContext.Config.databaseName, "gary.strange2");

            //u.Wait();


            //var p = CosmosDBSecurityHelper.GrantUserPermissionAsync(testContext.Client, testContext.Config.databaseName, u.Result, testContext.Collection);
            //p.Wait();

            //Console.WriteLine("Using read context get user permissions");
            //var perms = CosmosDBSecurityHelper.GetUserPermissions(testContext.Client, u.Result);
            //foreach (var perm in perms)
            //{
            //    Console.WriteLine(perm.ToString());

            //}

            
            //Debug.WriteLine(string.Format("Token: {0}", p.Result.Resource.Token));

            var configToken = CosmosDbClientConfig.CreateDocDbConfigFromAppConfig(
                Configuration["EndPointUrl"],
                "type=resource&ver=1&sig=NCwcbegxynO4oSoc5+PiiQ==;dqak+owvvd4XHBtGbtNQrjKgsiZXXrwvtnnDP5UjUhlXj2sRK5W9arm5ahQ+5AqtyAH/Y5GBvrg9ml7w0wcNtlh+V8rZ0i2maXvddy8vC2wqTxm76bttkFaKBr6B/cbLJEq2V5sth6Q5Bo57LII6BJdrYd6OIV3X2MegzlGKLqxTdC24462h6kLCQMjDw3Sg5uoHXEgw1zxG8gLYorGu6TJoL01o75qQUYVSqBswvcMA/KrZk1T7VkdjWdirYXN9;",
                Configuration["DatabaseName"],
                Configuration["CollectionName"],
                Configuration["PartitionKeyPath"]
                );

            var readContext =
                DocumentCollectionContextFactory.CreateCollectionContext(
                    configToken
                );


            CustomerProfile customerProfileRead;
            customerProfileRead = CosmosDbHelper.ReadDocument<CustomerProfile>(testContext, customerProfile.Id, customerProfile.PartitionKeyValue);
            customerProfileRead = CosmosDbHelper.ReadDocument<CustomerProfile>(readContext, customerProfile.Id, customerProfile.PartitionKeyValue);

            //customerProfile = CosmosDbHelper.CreateDocument(readContext, customerProfile);

            //Console.WriteLine($"option1 = {Configuration["option1"]}");
            //Console.WriteLine($"option2 = {Configuration["option2"]}");
            //Console.WriteLine(
            //    $"suboption1 = {Configuration["subsection:suboption1"]}");
            //Console.WriteLine();

            //Console.WriteLine("Wizards:");
            //Console.Write($"{Configuration["wizards:0:Name"]}, ");
            //Console.WriteLine($"age {Configuration["wizards:0:Age"]}");
            //Console.Write($"{Configuration["wizards:1:Name"]}, ");
            //Console.WriteLine($"age {Configuration["wizards:1:Age"]}");
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }



    }
}