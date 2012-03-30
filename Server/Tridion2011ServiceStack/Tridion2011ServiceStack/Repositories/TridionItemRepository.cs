using System;
using Tridion.ContentManager.CoreService.Client;
using Tridion2011ServiceStack.Models;
using System.Configuration;


namespace Tridion2011ServiceStack.Repositories
{
    public class TridionItemRepository
    {
        public TridionItem GetByUri(string uri)
        {
            TridionItem tridionItem = new TridionItem();
            try
            {
                CoreServiceClient client = new CoreServiceClient();
                client.ClientCredentials.Windows.ClientCredential.UserName = ConfigurationManager.AppSettings["impersonationUser"].ToString(); // "administrator";
                client.ClientCredentials.Windows.ClientCredential.Password = ConfigurationManager.AppSettings["impersonationPassword"].ToString();
                client.ClientCredentials.Windows.ClientCredential.Domain = ConfigurationManager.AppSettings["impersonationDomain"].ToString();
                IdentifiableObjectData objectData = client.Read(uri, null) as IdentifiableObjectData;
                FullVersionInfo versionInfo = objectData.VersionInfo as FullVersionInfo;
                tridionItem.Title = objectData.Title;
                tridionItem.Uri = uri;
                tridionItem.LastModifiedBy = versionInfo.Revisor.Title;
            }
            catch (Exception ex)
            {
                tridionItem.Error = ex.Source + ", " + ex.Message + ", " + ex.ToString();
            }
            return tridionItem;
        }
    }
}