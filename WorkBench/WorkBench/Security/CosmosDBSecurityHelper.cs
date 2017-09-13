using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkBench.DataAccess;

namespace WorkBench.Security
{
    public static class CosmosDBSecurityHelper
    {
        public static async Task<User> CreateUserIfNotExistAsync(DocumentCollectionContext context, string userId)
        {
            return await CreateUserIfNotExistAsync(context.Client, context.Config.databaseName, userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="databaseName"></param>
        /// <param name="userId">The userId should be extracted from the bearer token.</param>
        /// <returns></returns>
        public static async Task<User> CreateUserIfNotExistAsync(DocumentClient client, string databaseName, string userId)
        {
            User docUser = new User
            {
                Id = userId
            };

            try
            {
                docUser = await client.ReadUserAsync(UriFactory.CreateUserUri(databaseName, userId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    docUser = await client.CreateUserAsync(UriFactory.CreateDatabaseUri(databaseName), docUser);
                }
            }

            return docUser;

        }

        public static async Task<ResourceResponse<Permission>> GrantUserPermissionAsync(
            DocumentCollectionContext context,
            User user,
            Resource resource = null,
            PermissionMode mode = PermissionMode.Read,
            int? resourceTokenExpirySeconds = null
            )
        {
            resource = resource ?? context.Collection;
            return await GrantUserPermissionAsync(context.Client, context.Config.databaseName, user, resource, mode, resourceTokenExpirySeconds);
        }

        public static async Task<ResourceResponse<Permission>> GrantUserPermissionAsync(
            DocumentClient client, 
            string databaseName, 
            User user, 
            Resource resource, 
            PermissionMode mode = PermissionMode.Read, 
            int? resourceTokenExpirySeconds = null)
        {
            ResourceResponse<Permission> permission = null;
            string permissionId = CreatePermissionId(databaseName, user.Id, resource.Id, mode.ConvertToString());
            try
            {
                permission = await client.ReadPermissionAsync(UriFactory.CreatePermissionUri(databaseName, user.Id, permissionId));

            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    permission = await client.CreatePermissionAsync(
                        user.SelfLink, 
                        CreatePermission(databaseName, resource, permissionId), 
                        new RequestOptions() { ResourceTokenExpirySeconds = resourceTokenExpirySeconds } );
                }
                else throw e;
            }
            return permission;
        }

        public static List<Permission> GetUserPermissions(DocumentClient client, User user)
        => client.CreatePermissionQuery(user.PermissionsLink).ToList();
           

        private static String ConvertToString(this Enum eff)
            => Enum.GetName(eff.GetType(), eff);

        private static Permission CreatePermission(string databaseName, Resource resource, string permissionId, PermissionMode mode = PermissionMode.Read)
        => new Permission
            {
                PermissionMode = mode,
                ResourceLink = resource.SelfLink,
                Id = permissionId
        };



        private static string CreatePermissionId(string databaseName, string userId, string resourceId, string permissionMode) 
            => string.Format("Database-{3}-User-{0}-Resource-{1}-PermissionMode-{2}", userId, resourceId, permissionMode, databaseName);

    }
}
