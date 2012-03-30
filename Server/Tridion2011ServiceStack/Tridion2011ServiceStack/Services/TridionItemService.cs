using ServiceStack.ServiceInterface;
using Tridion2011ServiceStack.Models;
using Tridion2011ServiceStack.Repositories;

namespace Tridion2011ServiceStack.Services
{
    public class TridionItemService : RestServiceBase<TridionItem>
    {
        public TridionItemRepository Repository { get; set; }  //Injected by IOC

        public override object OnGet(TridionItem request)
        {
            return Repository.GetByUri(request.Uri);
        }
        	
        public override object OnPost(TridionItem tridionItem)
		{
            return tridionItem;
            //return Repository.Store(tridionItem);
		}

		public override object OnPut(TridionItem tridionItem)
		{
            return tridionItem;
			//return Repository.Store(tridionItem);
		}

		public override object OnDelete(TridionItem request)
		{
			//Repository.DeleteById(request.Uri);
			return null;
		}
    }
}