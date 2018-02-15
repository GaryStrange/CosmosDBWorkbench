using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public interface ICollectionContext
    {
        DocumentClient Client { get; }
        Uri CollectionUri { get; }
        Uri DocumentUri(string documentId);

        IResponseProcessor ResponseProcessor { get; }


        //Task<TNewResult> Execute<TNewResult>(Func<Task<TNewResult>, TNewResult> function);
    }
}
