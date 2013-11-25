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

        [HttpGet]
        public ZStatsResults SingleZStats(String realm, String faction, Int32 itemID, DateTime startTime, DateTime endTime, double percentile = .25, Int32 minPrice = 250)
        {
            ZStatsResults result = new ZStatsResults();
            var auctions = (from auc in wac.Auctions
                         where auc.MyAuctionHouse.Realm == realm
                            && auc.MyAuctionHouse.Faction == faction
                            && auc.MyItem.ID == itemID
                            && auc.TimeStamp >= startTime
                            && auc.TimeStamp < endTime
                         select auc);

            var buyouts = auctions.SelectMany(a =>
                Enumerable.Range(0, a.Quanity).Select(buy => a.Buyout / a.Quanity * 1.0)
            );

            var culledBuyouts = buyouts.OrderBy(a => a).Take((int)Math.Ceiling(buyouts.Count() * percentile));
            result.Mean = culledBuyouts.Average();
            var currentMin = auctions.Where(a => a.TimeStamp == auctions.Select(ts => ts.TimeStamp).Max()).Select(auc => auc.Buyout / auc.Quanity).Min();

            Double sumOfDeviation = culledBuyouts.Sum(d => (d - result.Mean) * (d - result.Mean));
            result.StdDev = Math.Sqrt(sumOfDeviation / culledBuyouts.Count());

            Double ZValue = (result.Mean - currentMin) / result.StdDev;

            return result;
        }

    }
    public class ZStatsResults
    {
        public Double Mean { get; set; }
        public Double CurrMin { get; set; }
        public Double StdDev { get; set; }
        public Double ZValue { get; set; }
    }
}
