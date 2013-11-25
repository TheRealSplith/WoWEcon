using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WoWEcon.Models;

namespace WoWEcon.Controllers.Api
{
    public class AuctionAPIController : ApiController
    {
        WoWAuctionContext wac = new WoWAuctionContext();

        [HttpGet]
        public IEnumerable<Item> AutoComplete(String query = "")
        {
            return String.IsNullOrEmpty(query) ? wac.Items.Take(5).ToList() : wac.Items.Where(i => i.Name.Contains(query)).ToList();
        }
    }
}
