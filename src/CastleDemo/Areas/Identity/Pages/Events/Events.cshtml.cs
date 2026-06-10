using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle;
using Castle.Messages.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CastleDemo.Areas.Identity.Pages.Events
{
    [AllowAnonymous]
    public class EventsModel : PageModel
    {
        private readonly CastleClient _castleClient;

        public EventsModel(CastleClient castleClient)
        {
            _castleClient = castleClient;
        }

        public string Status { get; private set; }

        public void OnGet()
        {
        }

        // Fetches the events schema and runs a query for $login events.
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var schema = await _castleClient.EventsSchema();

                var events = await _castleClient.QueryEvents(new EventsQueryRequest
                {
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter { Field = "name", Op = "$eq", Value = "$login" }
                    }
                });

                Status = $"Schema fields: {schema.Fields?.Count ?? 0}; query returned {events.TotalCount} event(s).";
            }
            catch (Exception e)
            {
                Status = $"Request failed: {e.Message}";
            }

            return Page();
        }
    }
}
