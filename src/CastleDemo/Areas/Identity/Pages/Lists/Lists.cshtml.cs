using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle;
using Castle.Messages.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CastleDemo.Areas.Identity.Pages.Lists
{
    [AllowAnonymous]
    public class ListsModel : PageModel
    {
        private readonly CastleClient _castleClient;

        public ListsModel(CastleClient castleClient)
        {
            _castleClient = castleClient;
        }

        public string Status { get; private set; }

        public void OnGet()
        {
        }

        // Walks through the Lists + List items API: create a list, add an item,
        // query the items and remove the list again.
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var list = await _castleClient.CreateList(new CreateListRequest
                {
                    Name = "Demo blocklist",
                    Color = "$red",
                    PrimaryField = "user.email"
                });

                var item = await _castleClient.CreateListItem(list.Id, new CreateListItemRequest
                {
                    PrimaryValue = "demo@example.com",
                    Author = new ListItemAuthor { Type = "$user", Identifier = "user:demo" }
                });

                var items = await _castleClient.QueryListItems(list.Id, new SearchQuery());

                await _castleClient.DeleteList(list.Id);

                Status = $"Created list {list.Id}, added item {item.Id}, queried {items.Count} item(s), deleted the list.";
            }
            catch (Exception e)
            {
                Status = $"Request failed: {e.Message}";
            }

            return Page();
        }
    }
}
