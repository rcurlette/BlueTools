using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tridion2011ServiceStack.Models;
using Tridion2011ServiceStack.Repositories;
using ServiceStack.ServiceInterface;

namespace Tridion2011ServiceStack.Services
{
    public class TridionBlueCopyService : RestServiceBase<BlueCopyItem>
    {
        public BlueCopyRepository Repository { get; set; }  //Injected by IOC

        public override object OnGet(BlueCopyItem request)
        {
            return Repository.CopyItem(request.Uri, request.Title, request.Filename);
        }

        public override object OnPost(BlueCopyItem tridionItem)
        {
            return Repository.CopyItem(tridionItem.Uri, tridionItem.Title, tridionItem.Filename);
            //return Repository.Store(tridionItem);
        }

        public override object OnPut(BlueCopyItem tridionItem)
        {
            return tridionItem;
            //return Repository.Store(tridionItem);
        }

        public override object OnDelete(BlueCopyItem request)
        {
            //Repository.DeleteById(request.Uri);
            return null;
        }
    }
}