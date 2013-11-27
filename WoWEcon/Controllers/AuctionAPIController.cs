using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WoWEcon.Models;

namespace WoWEcon.Controllers
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
                            // I work with buyouts, not concerned with bid only operations
                            && auc.Buyout > 0
                         select auc).ToList();

            // stop if there is no results
            if (auctions.Count() == 0)
            {
                return result;
            }
            // Finds distinct auctions
            GenericEqualityComparer<Auction> comparer = new GenericEqualityComparer<Auction>(
                (a, b) => a.AucID == b.AucID,
                (a) => a.GetHashCode()
            );
            var buyouts = auctions.Distinct(comparer).SelectMany(a =>
                {
                    return Enumerable.Range(0, a.Quanity).Select(buy => a.Buyout / a.Quanity * 1.0);
                }
            ).ToList();

            // Calculates the average number of items found per TimeStamp
            result.AvgSeen = auctions.Count() * 1.0 / auctions.GroupBy(a => a.TimeStamp).Count();

            // buyouts in a certain percentile
            var culledBuyouts = buyouts.OrderBy(a => a).Take((int)Math.Ceiling(buyouts.Count() * percentile));
            result.Mean = culledBuyouts.Average();
            // Min of latest scan
            result.CurrMin = auctions.Where(a => a.TimeStamp == auctions.Select(ts => ts.TimeStamp).Max()).Select(auc => auc.Buyout / auc.Quanity).Min();

            Int64 sumOfDeviation = (Int64)Math.Floor(culledBuyouts.Sum(d => (d - result.Mean) * (d - result.Mean)));
            result.StdDev = Math.Sqrt(sumOfDeviation / culledBuyouts.Count());

            result.ZValue = (result.CurrMin - result.Mean) / result.StdDev;

            return result;
        }

        [ActionName("HistoricalData")]
        public IEnumerable<HistoricalResults> HistoricalData(String realm, String faction, Int32 itemID, DateTime startTime, DateTime endTime, double percentile = .25)
        {
            var auctions = (from auc in wac.Auctions
                            where auc.MyAuctionHouse.Realm == realm
                               && auc.MyAuctionHouse.Faction == faction
                               && auc.MyItem.ID == itemID
                               && auc.TimeStamp >= startTime
                               && auc.TimeStamp < endTime
                                // I work with buyouts, not concerned with bid only operations
                               && auc.Buyout > 0
                            group auc by auc.TimeStamp into t
                            select t).ToList();

            ConcurrentBag<HistoricalResults> bag = new ConcurrentBag<HistoricalResults>();
            Parallel.ForEach(auctions, (iTimeStamp) =>
            {
                HistoricalResults hr = new HistoricalResults();
                hr.TimeStamp = iTimeStamp.Key;
                hr.Count = iTimeStamp.Count();
                var buyouts = iTimeStamp.SelectMany(a =>
                    {
                        return Enumerable.Range(0, a.Quanity).Select(b => a.Buyout);
                    }
                );
                var culledAuctions = buyouts.OrderBy(buy => buy).Take((int)Math.Ceiling(hr.Count * percentile));

                hr.Min = culledAuctions.Min();
                hr.Mean = culledAuctions.Average();

                bag.Add(hr);
            });

            return bag.OrderBy(a => a.TimeStamp).ToList();
        }
    }
    public class ZStatsResults
    {
        public Double Mean { get; set; }
        public long CurrMin { get; set; }
        public Double StdDev { get; set; }
        public Double ZValue { get; set; }
        public Double AvgSeen { get; set; }
    }
}
