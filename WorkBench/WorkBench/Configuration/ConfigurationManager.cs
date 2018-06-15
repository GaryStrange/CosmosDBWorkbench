using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkBench.Configuration
{
    public static class ExtendConfigurationBuilder
    {
        public static void PrintConfigurationValues(this IConfiguration builder, Action<string> printAction)
        {
            foreach( var pair in builder.AsEnumerable())
            {
                printAction($"{pair.Key} = {pair.Value}");
            }
        }
    }
}
