using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using log4net;
using Tridion.ContentManager.CoreService.Client;
using Tridion2011ServiceStack.Models;

namespace Tridion2011ServiceStack.Repositories
{
    public class BlueCopyRepository
    {
        CoreServiceClient client = new CoreServiceClient();
        private static readonly ILog log = LogManager.GetLogger("TridionCopyItemRepository");

        public List<BlueCopyItem> CopyItem(string sourceUri, string title, string filename)
        {
            if (sourceUri == null)
                throw new Exception("Source uri, title or filename are null");

            log.InfoFormat("======Start TridionCopyItemRepository, sourceUri={0}, title={1}, filename={2} ======================", sourceUri, title, filename);
            client.ClientCredentials.Windows.ClientCredential.UserName = ConfigurationManager.AppSettings["impersonationUser"].ToString(); // "administrator";
            client.ClientCredentials.Windows.ClientCredential.Password = ConfigurationManager.AppSettings["impersonationPassword"].ToString();
            client.ClientCredentials.Windows.ClientCredential.Domain = ConfigurationManager.AppSettings["impersonationDomain"].ToString();
            List<BlueCopyItem> tridionCopyItems = new List<BlueCopyItem>();

            try
            {
                // GetSourceItem
                RepositoryLocalObjectData item = client.Read(sourceUri, new ReadOptions()) as RepositoryLocalObjectData;

                string newFilename = filename;

                // Create copy of parent item
                string copiedItemUri = CreateNewItemCopy(title, item, filename);

                // XML NodeList of Localized Items, containing ID property
                XContainer sourceLocalizedItems = GetLocalizedItems(sourceUri);

                // Localize all children of new copy and copy content from original
                foreach (XElement itemToCopy in sourceLocalizedItems.Elements())
                {
                    try
                    {
                        string uriLocalizedSource = itemToCopy.Attribute("ID").Value;
                        string newItemUri = GetLocalCopyUri(uriLocalizedSource, copiedItemUri);

                        // Localize
                        if (uriLocalizedSource == sourceUri)  // we also have the original parent uri in the list...need to not process it
                            continue;
                        else
                            LocalizeItem(newItemUri);

                        // Update New Localized Component
                        string newUri = UpdateLocalizedItem(title, uriLocalizedSource, newItemUri);

                        // Create the Response Object to send back via Ajax to our GUI Client
                        BlueCopyItem tridionItem = new BlueCopyItem()
                        {
                            Title = title, // GetCmsEditUrl(newItemUri, title),
                            Uri = newItemUri,
                            SourceTitle = itemToCopy.Attribute("Title").Value,
                            Filename = filename
                        };
                        tridionCopyItems.Add(tridionItem);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error with " + itemToCopy.Attribute("ID").Value);
                        log.Error(ex);
                        BlueCopyItem tridionItem = new BlueCopyItem()
                        {
                            Error = ex.Message
                        };
                        tridionCopyItems.Add(tridionItem);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Source + "," + ex.Message + "," + ex.ToString());
                log.Error(ex);
                throw;
            }
            return tridionCopyItems;
        }


        private string GetCmsEditUrl(string uri, string linkText)
        {
            return String.Format("<a href=\"{0}/WebUI/item.aspx?tcm={1}#id={2}\" target=\"_blank\">{3}</a>",
                ConfigurationManager.AppSettings["cmsUrl"].ToString(),
                GetItemType(uri),
                uri,
                linkText);
        }

        /// <summary>
        /// Get Item Type like '16' for Component, etc for CMS Edit link
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private string GetItemType(string uri)
        {
            if (uri.EndsWith("-64"))
                return "64";
            else if (uri.EndsWith("-4"))
                return "4";
            else if (uri.EndsWith("-2"))
                return "2";
            else
                return "16";
        }

        /// <summary>
        /// Get Tridion ItemType
        /// </summary>
        /// <param name="source">RepositoryLocalObjectData item</param>
        /// <returns>ItemType.Component or ItemType.Page</returns>
        private ItemType GetTridionItemType(RepositoryLocalObjectData source)
        {
            string itemType = source.GetType().Name;

            switch (itemType)
            {
                case "ComponentData":
                    return ItemType.Component;
                case "PageData":
                    return ItemType.Page;
            }
            return ItemType.UnknownByClient;
        }

        /// <summary>
        /// Get ItemType based on URI
        /// </summary>
        /// <param name="uri">Item URI - currently only implemented for Page and Component</param>
        /// <returns></returns>
        private ItemType GetTridionItemType(string uri)
        {
            RepositoryLocalObjectData sourceData = client.Read(uri, new ReadOptions()) as RepositoryLocalObjectData;
            return GetTridionItemType(sourceData);
        }


        /// <summary>
        /// Copy Tridion item - used for Pages and Components
        /// </summary>
        /// <param name="title">Title for new item</param>
        /// <param name="source">RepositoryLocalObjectData of item to copy</param>
        /// <param name="filename">Used for pages</param>
        /// <returns></returns>
        private string CreateNewItemCopy(string title, RepositoryLocalObjectData source, string filename)
        {
            string newItemUri = "";
            try
            {
                ItemType tridionItemType = GetTridionItemType(source);
                string orgItemUri = source.LocationInfo.OrganizationalItem.IdRef;
                var newItem = client.Copy(source.Id, orgItemUri, true, new ReadOptions());
                newItem.Title = title;
                if (tridionItemType == ItemType.Page)
                {
                    PageData pageData = newItem as PageData;
                    pageData.FileName = filename;
                    client.Update(pageData, new ReadOptions());
                }
                else
                {
                    client.Update(newItem, new ReadOptions());
                }
                newItemUri = newItem.Id;
            }
            catch (Exception ex)
            {
                log.Error(ex.Source + "," + ex.Message + "," + ex.ToString());
                log.Error(ex);
                throw;
            }

            return newItemUri;
        }

        /// <summary>
        /// Copy contents from localized item to new item
        /// </summary>
        /// <param name="title">New title</param>
        /// <param name="uriLocalizedSource">Source URI</param>
        /// <param name="newItemUri">Destination URI</param>
        /// <returns></returns>
        private string UpdateLocalizedItem(string title, string uriLocalizedSource, string newItemUri)
        {
            try
            {
                ItemType tridionItemType = GetTridionItemType(uriLocalizedSource);
                var newItem = client.Read(newItemUri, new ReadOptions()) as RepositoryLocalObjectData;
                var oldItem = client.Read(uriLocalizedSource, new ReadOptions()) as RepositoryLocalObjectData;
                if (newItem.MetadataSchema.Title != "")
                {
                    var oldItemMetadataSchema = client.Read(oldItem.MetadataSchema.IdRef, new ReadOptions()) as SchemaData;
                    newItem.Metadata = GetMetadata(oldItem.Metadata, oldItemMetadataSchema.NamespaceUri);
                }

                if (tridionItemType == ItemType.Page)
                {
                    PageData newPage = newItem as PageData;
                    PageData oldPage = oldItem as PageData;

                    newPage.ComponentPresentations = oldPage.ComponentPresentations;
                    newPage.Title = title;
                    client.Update(newPage, new ReadOptions());
                    return newPage.Id;
                }
                else if (tridionItemType == ItemType.Component)
                {
                    ComponentData newComp = newItem as ComponentData;
                    ComponentData oldComp = oldItem as ComponentData;

                    newComp.Schema = oldComp.Schema;
                    newComp.MetadataSchema = oldComp.MetadataSchema;
                    newComp.Content = oldComp.Content;
                    newComp.Metadata = oldComp.Metadata;
                    newComp.Title = title;
                    return newComp.Id;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Source + "," + ex.Message + "," + ex.ToString());
                log.Error(ex);
            }
            return uriLocalizedSource;
        }

        /// <summary>
        /// Get MetaData content for item.  Sets Metadata namespace when content is empty
        /// </summary>
        /// <param name="metadataContent">Content of metadata element</param>
        /// <param name="metadataNamespace">Namespace of metadata schema</param>
        /// <returns></returns>
        private string GetMetadata(string metadataContent, string metadataNamespace)
        {
            string metadata = "";
            if (metadataContent == "")
            {
                metadata = String.Format("<{0} />", metadataNamespace);
            }
            else
            {
                metadata = metadataContent;
            }
            return metadata;
        }

        /// <summary>
        /// Get localized URI
        /// </summary>
        /// <param name="sourceUri">Localized source URI</param>
        /// <param name="destinationUri">Blueprint parent uri</param>
        /// <returns></returns>
        private string GetLocalCopyUri(string sourceUri, string destinationUri)
        {
            string localUri = destinationUri;
            string sourcePubUri = sourceUri.Split('-')[0];  // tcm:1, 2, 64
            string destinationPubUri = destinationUri.Split('-')[0];
            localUri = localUri.Replace(destinationPubUri, sourcePubUri);
            return localUri;
        }

        /// <summary>
        /// Localize Item
        /// </summary>
        /// <param name="uri">Item URI.  Does not work with Folders / Structure Groups</param>
        private void LocalizeItem(string uri)
        {
            VersionedItemData newTridionCopy = client.Read(uri, null) as VersionedItemData;
            if (newTridionCopy.BluePrintInfo.IsLocalized == false)
            {
                client.Localize(newTridionCopy.Id, null);
            }
        }

        /// <summary>
        /// Find all localized versions of this item
        /// </summary>
        /// <param name="itemUri">Parent item</param>
        /// <returns>List of all localized items, including parent item</returns>
        private XContainer GetLocalizedItems(string itemUri)
        {
            XContainer localizedItems = null;
            try
            {
                BluePrintChainFilterData filter = new BluePrintChainFilterData();
                filter.Direction = BluePrintChainDirection.Down;
                localizedItems = client.GetListXml(itemUri, filter);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
            return localizedItems;
        }
    }
}