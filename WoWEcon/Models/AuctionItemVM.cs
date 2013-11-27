using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WoWEcon.Models
{
    public class AuctionItemVM
    {
        public String ItemName { get; set; }
        public String Faction
        {
            get
            {
                return Data.PrimaryName;
            }
            set
            {
                // Construct data sub-object
                if (Data == null)
                    Data = new ItemData();

                // Set Names
                Data.PrimaryName = value;
                Data.OffName = value != "horde" ? "horde" : "alliance";

                // Set Colors
                Data.PrimaryColor = value == "horde" ? "red" : "blue";
                Data.OffColor = value != "horde" ? "red" : "blue";
            }
        }
        public String Realm { get; set; }
        public ItemData Data { get; set; }
        public IEnumerable<HistoricalResults> HistoryData { get; set; }
        
        public String MinData()
        {
            return DateDataMatcher(new Func<HistoricalResults, long>((hr) => hr.Min));
        }
        public String MarketData()
        {
            return DateDataMatcher(new Func<HistoricalResults, long>((hr) => (Int64)Math.Round(hr.Mean, 0)));
        }
        private String DateDataMatcher(Func<HistoricalResults,Int64> selector)
        {
            String[] dataItems = new String[HistoryData.Count()];

            Int32 iterCounter = 0;
            foreach(var iHitory in HistoryData)
            {
                var value = selector(iHitory);
                dataItems[iterCounter] = String.Format(
                    "[Date.UTC({0},{1},{2},{3},{4}), {5}]",
                    iHitory.TimeStamp.Year, 
                    // -1 because Javascript starts at 0 for january
                    iHitory.TimeStamp.Month - 1,
                    iHitory.TimeStamp.Day,
                    iHitory.TimeStamp.Hour,
                    iHitory.TimeStamp.Minute,
                    value);

                 ++iterCounter;
            }

            return String.Join(",", dataItems);
        }

    }

    public class HistoricalResults
    {
        public DateTime TimeStamp { get; set; }
        public Int64 Min { get; set; }
        public Double Mean { get; set; }
        public Int32 Count { get; set; }
    }

    public class ItemData
    {
        public KeyValuePair<Int64, Int32>[] Primary { get; set; }
        public String PrimaryName { get; set; }
        public String PrimaryColor { get; set; }
        public KeyValuePair<Int64, Int32>[] Off { get; set; }
        public String OffName { get; set; }
        public String OffColor { get; set; }
        public Int64[] Categories
        {
            get
            {
                return (Primary.Select(a => a.Key).Union(Off.Select(o => o.Key))).ToArray();
            }
        }
    }
}