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

            // Get a set of Item:IE<Auction>
            var ByItems = wac.Auctions
                        .Where(aucCheck => aucCheck.TimeStamp >= StartDate
                            // Get the right AH
                            && aucCheck.MyAuctionHouse.Faction == faction
                            && aucCheck.MyAuctionHouse.Realm == realm)
                        // Teh grouping
                        .GroupBy(itemGroup => itemGroup.MyItem)
                        // Sum of all Quantity / Number of times we got data = Average listing quantity
                        .Where(itemAucs => 
                            itemAucs.Sum(a => a.Quanity) 
                            / itemAucs.Select(a => a.TimeStamp).Distinct().Count() > count)
                        .Where(buyoutFloor => buyoutFloor.Select(a => a.Buyout).Min() > buymin);

            List<AuctionSummary> items = new List<AuctionSummary>();
            ConcurrentBag<AuctionSummary> bag = new ConcurrentBag<AuctionSummary>();
            Parallel.ForEach(ByItems, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (itemAuctions) =>
                {
                    AuctionSummary ret = new AuctionSummary();
                    ret.ItemID = itemAuctions.Key.ID;
                    ret.ItemName = itemAuctions.First().MyItem.Name;
                    ret.MinBuyout = itemAuctions.Where(a => a.TimeStamp == itemAuctions.Select(auc => auc.TimeStamp).Max()).Min(a => a.Buyout / a.Quanity);
                    var accountCompare = new GenericEqualityComparer<Auction>(
                        (Auction x, Auction y) => x.AucID == y.AucID,
                        (Auction x) => x.GetHashCode()
                    );
                    var auctions = itemAuctions.Distinct(accountCompare);
                    var bigCount = auctions.Count();
                    auctions = auctions.OrderBy(a => a.Buyout).Take((int)Math.Ceiling(bigCount * .25));

                    var buyouts = auctions.SelectMany(a =>
                        {
                            return Enumerable.Range(0, a.Quanity).Select(v => a.Buyout / a.Quanity * 1.0);
                        }
                    );

                    ret.StdDev = Math.Round(Extensions.StdDev(buyouts), 4);
                    // If there is no variance, there is no volatility
                    if (ret.StdDev != 0.0)
                    {
                        ret.Mean = Math.Round(buyouts.Average() , 4);
                        ret.ZValue = Math.Round((ret.MinBuyout - ret.Mean) / ret.StdDev, 4);
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
