using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WoWEcon.Models;

namespace WoWEcon.Controllers
{
    public class HomeController : Controller
    {
        WoWAuctionContext wac = new WoWAuctionContext();

        [OutputCache(Duration=60 * 60, VaryByParam="*", Location=System.Web.UI.OutputCacheLocation.Server)]
        public ActionResult Index(String faction = "horde", String realm = "bonechewer", Int32 count = 20, Int32 results = 25, Int32 buymin = 250)
        {
            var StartDate = DateTime.Today.Subtract(new TimeSpan(3, 0, 0, 0));

            // EqulityComparer for Item
            var itemComparer = new GenericEqualityComparer<Item>(
                (a, b) => a.ID == b.ID,
                (a) => a.GetHashCode()
            );
            // Get a set of Item:IE<Auction>
            var items = (from a in wac.Auctions
                           where a.MyAuctionHouse.Realm == realm
                              && a.MyAuctionHouse.Faction == faction
                              && a.TimeStamp >= StartDate
                              && a.TimeStamp <= DateTime.Now
                              && a.Buyout > 0
                           select a.MyItem).ToList();

            items = items.Distinct(itemComparer).ToList();

            // result collection
            ConcurrentBag<AuctionSummary> bag = new ConcurrentBag<AuctionSummary>();
            Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (itemAuctions) =>
                {
                    AuctionSummary ret = new AuctionSummary();
                    AuctionAPIController api = new AuctionAPIController();

                    var result = api.SingleZStats(realm, faction, itemAuctions.ID, StartDate, DateTime.Now, .15, 250);

                    // If there is no variance, there is no volatility
                    if (result.StdDev != 0.0 && result.AvgSeen >= count)
                    {
                        ret.ItemID = itemAuctions.ID;
                        ret.ItemName = itemAuctions.Name;
                        ret.Mean = Math.Round(result.Mean, 4);
                        ret.MinBuyout = result.CurrMin;
                        ret.StdDev = Math.Round(result.StdDev, 4);
                        ret.ZValue = Math.Round(result.ZValue, 4);

                        bag.Add(ret);
                    }
                }
            );

            var vmIndex = new HomeIndexVM() { Faction = faction, Realm = realm };
            vmIndex.Items = bag.OrderBy(a => a.ZValue).Take(results).ToList();

            ViewData["faction"] = faction;
            ViewData["realm"] = realm;

            return View(vmIndex);
        }
    }
}
