using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ifound.Models;

namespace Ifound.Services
{
    public class AuctionService : Ifound.Services.IAuctionService
    {
        //获取剩余时间
        public string GetRemainingTime(string endtime)
        {
            DateTime now = DateTime.Now;
            DateTime end = Convert.ToDateTime(endtime);
            TimeSpan span = end - now;
            string day, hour, minute, second;
            day = span.TotalDays.ToString();
            hour = span.TotalHours.ToString();
            minute = span.TotalMinutes.ToString();
            second = span.TotalSeconds.ToString();
            return day + ":" + hour + ":" + minute + ":" + second;
        }

        //判断是否已达拍卖结束时间
        public bool IsOverTime(string endtime)
        {
            DateTime end = Convert.ToDateTime(endtime);
            DateTime now = DateTime.Now;
            if (now <= end)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public IEnumerable<Auction> ViewProductsInTypes(GoodsType type, IfoundDbContext db)
        {
            var auctions = db.Auctions.Where(x => x.AucType == type).AsEnumerable();
            return auctions;
        }
    }
}