﻿using System;
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

        public ActionResult Item(String realm, String faction, Int32 id, Int32 numPriceCat = 10)
        {
            var item = wac.Items.Find(id);
            if (item == null)
                return new HttpNotFoundResult("No Items found!");

            // we get the most recent time so that we can get a cross-section of
            // the most recent scan
            DateTime startDate = wac.Auctions.Select(a => a.TimeStamp).Max();
            var currentItems = from auc in wac.Auctions
                               where auc.MyAuctionHouse.Realm == realm
                                    && auc.MyAuctionHouse.Faction == faction
                                    && auc.ItemID == id
                                    && auc.TimeStamp == startDate
                               select auc;

            Int32 uniquePrices = currentItems.Select(a => a.Buyout).Distinct().Count();
            // Note Cats refers to categories
            Int32 numberOfCats = uniquePrices < numPriceCat ? uniquePrices : numPriceCat;
            Int64[] catPriceBreakdown = new Int64[numberOfCats];

            // Used to calculate our category ranges
            var bottom = currentItems.Select(a => a.Buyout).Min();
            var top = currentItems.Select(a => a.Buyout).Max();
            var diff = (top - bottom) / numberOfCats;

            // Actual category range calculation
            for (int i = 0; i < catPriceBreakdown.Length; ++i)
            {
                    catPriceBreakdown[i] = bottom + (diff * i);
            }

            // Category structure
            KeyValuePair<Int64, Int32>[] histoData = new KeyValuePair<long, int>[numberOfCats];
            // here we get the number of items in certain price ranges to get a
            // histogram of a pricing cross-section
            for (int i = 0; i < histoData.Length; ++i )
            {
                if(i != histoData.Length)
                {
                    histoData[i] = new KeyValuePair<long, int>(
                        // the bottom price for the histogram category
                        bottom + (i * diff),
                        currentItems
                            .Where(
                            // bottom condition
                            a => a.Buyout >= bottom + (i * diff) 
                            // and top condition
                         && a.Buyout < bottom + ((i + 1) * diff))
                            // this is what we are here for, the number of items in the range
                            .Sum(a => a.Quanity)
                    );
                }
                else
                {
                    histoData[i] = new KeyValuePair<long,int>(
                        // the bottom price for category
                        bottom + (i * diff),
                        // number of items in range
                        currentItems.Where(a => a.Buyout >= bottom + (i * diff)).Sum(a => a.Quanity)
                    );
                }
            }

            return View(histoData);
        }

    }
}
