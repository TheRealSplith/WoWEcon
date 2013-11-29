using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WoWEcon.Models;

namespace WoWEcon.Controllers
{
    public class AuctionController : Controller
    {
        WoWAuctionContext wac = new WoWAuctionContext();
        //
        // GET: /Auction/

        public ActionResult Item(Int32 id, String faction = "horde", String realm = "bonechewer", Int32 numPriceCat = 10)
        {
            AuctionAPIController api = new AuctionAPIController();
            String offFaction = faction == "horde" ? "alliance" : "horde";

            AuctionItemVM vm = new AuctionItemVM();

            var item = wac.Items.Find(id);
            if (item == null)
                return new HttpNotFoundResult("No Items found!");

            vm.ItemName = item.Name;
            vm.Faction = faction;
            vm.Realm = realm;

            // we get the most recent time so that we can get a cross-section of
            // the most recent scan
            DateTime startDate = wac.Auctions.Select(a => a.TimeStamp).Max();
            var factionItems = (from auc in wac.Auctions
                               where auc.MyAuctionHouse.Realm == realm
                                    && auc.MyAuctionHouse.Faction == faction
                                    && auc.ItemID == id
                                    && auc.TimeStamp == startDate
                               select auc).ToList();
            var offFactionItems = (from auc in wac.Auctions
                                   where auc.MyAuctionHouse.Realm == realm
                                        && auc.MyAuctionHouse.Faction == offFaction
                                        && auc.ItemID == id
                                        && auc.TimeStamp == startDate
                                   select auc).ToList();

            // If we don't have items we need to throw a tantrume
            if (factionItems.Count() == 0)
                return new HttpNotFoundResult("No records found");

            IEnumerable<long> allBuyoutPrices = factionItems.Union(offFactionItems).Select(a => a.Buyout / a.Quanity).Distinct();
            Int32 uniquePrices = allBuyoutPrices.Count();
            // Note Cats refers to categories
            Int32 numberOfCats = uniquePrices < numPriceCat ? uniquePrices : numPriceCat;
            Int64[] catPriceBreakdown = new Int64[numberOfCats];

            // Used to calculate our category ranges
            var bottom = allBuyoutPrices.Min();
            var top = allBuyoutPrices.Max();
            var diff = (top - bottom) / numberOfCats;

            // Actual category range calculation
            for (int i = 0; i < catPriceBreakdown.Length; ++i)
            {
                    catPriceBreakdown[i] = bottom + (diff * i);
            }

            // Category structure
            vm.Data.Primary = new KeyValuePair<long, int>[numberOfCats];
            vm.Data.Off = new KeyValuePair<long, int>[numberOfCats];
            // here we get the number of items in certain price ranges to get a
            // histogram of a pricing cross-section
            for (int i = 0; i < vm.Data.Primary.Length; ++i )
            {
                if(i != vm.Data.Primary.Length -1)
                {
                    vm.Data.Primary[i] = new KeyValuePair<long, int>(
                        // the bottom price for the histogram category
                        bottom + (i * diff),
                        (from a in factionItems
                         where (a.Buyout / a.Quanity) >= bottom + (i * diff)
                            && (a.Buyout / a.Quanity) < bottom + ((i + 1) * diff)
                         select (int?)a.Quanity).Sum() ?? 0
                    );

                    vm.Data.Off[i] = new KeyValuePair<long, int>(
                        // the bottom price for the histogram category
                        bottom + (i * diff),
                        (from a in offFactionItems
                         where (a.Buyout / a.Quanity) >= bottom + (i * diff)
                            && (a.Buyout / a.Quanity) < bottom + ((i + 1) * diff)
                         select (int?)a.Quanity).Sum() ?? 0
                    );

                }
                else
                {
                    vm.Data.Primary[i] = new KeyValuePair<long,int>(
                        // the bottom price for category
                        bottom + (i * diff),
                        // number of items in range
                        factionItems.Where(a => a.Buyout >= bottom + (i * diff)).Sum(a => (int?)a.Quanity) ?? 0
                    );

                    vm.Data.Off[i] = new KeyValuePair<long, int>(
                        // the bottom price for category
                        bottom + (i * diff),
                        // number of items in range
                        offFactionItems.Where(a => a.Buyout >= bottom + (i * diff)).Sum(a => (int?)a.Quanity) ?? 0
                    );
                }
            }

            DateTime historicalStart = DateTime.Today.Subtract(new TimeSpan(30,0,0,0,0));
            vm.HistoryData = api.HistoricalData(realm, faction, id, historicalStart, DateTime.UtcNow);

            ViewData["faction"] = faction;
            ViewData["realm"] = realm;

            return View(vm);
        }
    }
}
