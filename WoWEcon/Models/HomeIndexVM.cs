using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WoWEcon.Models
{
    public class HomeIndexVM
    {
        public String Faction;
        public String Realm;
        public IList<AuctionSummary> Items = new List<AuctionSummary>();
    }

    public class AuctionSummary
    {
        public Int32 ItemID { get; set; }
        public String ItemName { get; set; }
        public Int64 MinBuyout { get; set; }
        public Double Mean { get; set; }
        public Double StdDev { get; set; }
        public Double ZValue { get; set; }
    }
}