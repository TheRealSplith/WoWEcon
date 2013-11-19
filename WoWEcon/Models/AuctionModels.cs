using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WoWEcon.Models
{
    public class WoWAuctionContext : DbContext
    {
        public WoWAuctionContext()
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<AuctionHouse> AuctionHouse { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuctionHouse>()
                .HasMany(ah => ah.Auctions)
                .WithOptional()
                .HasForeignKey(a => a.AuctionHouseID);

            modelBuilder.Entity<Auction>()
                .HasOptional(a => a.MyAuctionHouse)
                .WithMany()
                .HasForeignKey(a => a.AuctionHouseID);

            modelBuilder.Entity<Auction>()
                .HasOptional(a => a.MyItem)
                .WithMany()
                .HasForeignKey(a => a.ItemID);

            modelBuilder.Entity<Item>().Property(i => i.ID)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
        }
    }
    public class Auction
    {
        [Key]
        public Int32 ID { get; set; }
        public Int64 AucID { get; set; }
        public DateTime TimeStamp { get; set; }
        public Int32? ItemID { get; set; }
        public virtual Item MyItem { get; set; }
        public Int32? AuctionHouseID { get; set; }
        public virtual AuctionHouse MyAuctionHouse { get; set; }
        public Int64 Bid { get; set; }
        public Int64 Buyout { get; set; }
        public Int32 Quanity { get; set; }
    }

    public class AuctionHouse
    {
        public Int32 ID { get; set; }
        public String Realm { get; set; }
        public String Faction { get; set; }
        public virtual IList<Auction> Auctions { get; set; }
    }

    public class Item
    {
        [Key]
        public Int32 ID { get; set; }
        public String Name { get; set; }
    }
}