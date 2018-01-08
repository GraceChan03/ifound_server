using Ifound.Models;
using Ifound.Extensions;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System;
using Ifound.Services;

namespace Ifound.Controllers
{
    public class AuctionsManageController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();
        public IAuctionService _auctionService;
        private readonly ICommonService _commonService;

        public AuctionsManageController(IAuctionService auctionService, ICommonService commonService)
        {
            _auctionService = auctionService;
            _commonService = commonService;
        }
        public class AuctionObject
        {
            public bool Success { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }
        public AuctionObject WrapNoKey(object data)
        {
            return new AuctionObject { Data = data, Success = true };
        }

        //duration:持续时间(单位：天)
        public JsonpResult AddAuction(int userid, string auctitle, int auctype, 
            int isnew, decimal startingprice, decimal range, string aucmsg, int duration)
        {
            bool NewOrOld;
            if (isnew == 0) NewOrOld = false;//非新
            else NewOrOld = true;//全新
            try 
            {
                db.Auctions.Add(new Auction()
                {
                    AucTitle = auctitle,
                    AucType = (GoodsType)auctype,
                    IsNew = NewOrOld,
                    StartingPrice = startingprice,
                    PriceIncreaseRange = range,
                    AucMsg = aucmsg,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddDays(duration).ToString("yyyy/MM/dd HH:mm:ss"),
                    NewPrice = startingprice,
                    IsOnShelves = true
                });
                db.SaveChanges();
                var aucid = db.Auctions.AsEnumerable().Last().Id;
                db.UserAuctions.Add(new UserAuction()
                {
                    UserId=userid,
                    AucId=aucid
                });
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey(aucid));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("add auction failed"));
            }
        }

        public JsonpResult AddAucPicture(int aucid, string base64)
        {
            try
            {
                var auction = db.Auctions.Find(aucid);
                string pathUType = Server.MapPath("~/Image/Auctions/");
                string imageName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + aucid + "_" + (int)auction.AucType + ".jpg";
                if (auction.AucImage != null)
                {
                    string savepath = auction.AucImage;
                    string newpath = savepath.Replace('/', '\\');
                    string path = Server.MapPath("~/") + newpath.Substring(newpath.IndexOf("\\") + 1);
                    System.IO.File.Delete(path);
                }
                auction.AucImage = _commonService.SaveImage(base64, pathUType, imageName, "Auctions");
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("ERROR"));
            }
        }

        //显示自己发布的所有拍卖品
        public JsonpResult ShowOwnAuctions(int userid)
        {
            var judge = db.UserAuctions.Where(x => x.UserId == userid);
            string ipv4 = _commonService.GetIPV4();
            if (judge == null || judge.Count() < 1)
                return this.Jsonp(this.WrapNoKey("no records"));
            else
            {
                var userauc = judge.ToList();
                List<Auction> auctions = new List<Auction>();
                foreach (UserAuction a in userauc)
                {
                    Auction auction = db.Auctions.Find(a.AucId);
                    auction.AucImage = ipv4 + auction.AucImage;
                    auctions.Add(auction);
                }
                return this.Jsonp(this.WrapNoKey(auctions.AsEnumerable<Auction>()));
            }
            
        }

        //获取单个拍卖品信息
        public JsonpResult GetAuction(int userid, int aucid)
        {
            var judge = db.UserAuctions.Where(x => x.UserId == userid && x.AucId == aucid);
            string ipv4 = _commonService.GetIPV4();
            if (judge == null || judge.Count() < 1)
            {
                return this.Jsonp(this.WrapNoKey("user and auction are not matched"));
            }
            else
            {
                Auction auction = db.Auctions.Find(aucid);
                auction.AucImage = ipv4 + auction.AucImage;
                return this.Jsonp(this.WrapNoKey(auction));
            }
        }

        //删除拍卖品
        //实际是将拍卖品设为下架
        public JsonpResult DeleteAuction(int aucid)
        {
            try
            {
                var auction = db.Auctions.Find(aucid);
                auction.IsOnShelves = false;
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("Delete Auction OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Delete Auction Error"));
            }
        }

        //将拍卖品从表中删除
        public string DeleteAuctionFromTable(int aucid)
        {
            try
            {
                var auction = db.Auctions.Find(aucid);
                db.Auctions.Attach(auction);
                db.Auctions.Remove(auction);
                var usera = db.UserAuctions.Single(x => x.AucId == aucid);
                db.UserAuctions.Attach(usera);
                db.UserAuctions.Remove(usera);
                db.SaveChanges();
                return "OK";
            }
            catch
            {
                return "ERROR";
            }
        }

        class SailerAuction
        {
            public int AucId { get; set; }
            public string AucTitle { get; set; }
            public decimal AucPrice { get; set; }
            public string Option { get; set; }
        }

        //拍卖结束后，判断是否有买家
        //首先判断是否超时，再判断是否有买家
        //0表示没有买家，1表示有买家
        public JsonpResult HaveBuyer(int sailerid)
        {
            var usera = db.UserAuctions.Where(x => x.UserId == sailerid).ToList();
            List<SailerAuction> salist = new List<SailerAuction>();
            foreach (UserAuction ua in usera)
            {
                Auction auc = db.Auctions.Find(ua.AucId);
                SailerAuction sa = new SailerAuction()
                {
                    AucId = auc.Id,
                    AucTitle = auc.AucTitle,
                    AucPrice = auc.NewPrice
                };
                if (_auctionService.IsOverTime(auc.EndTime))
                {
                    auc.IsOnShelves = false;
                    db.SaveChanges();
                    if (auc.Buyerid == 0)
                    {
                        sa.Option = "0";
                        salist.Add(sa);
                    }
                    else
                    {
                        sa.Option = "1";
                        salist.Add(sa);
                    }
                }
            }
            if (salist.Count == 0)
            {
                return this.Jsonp(this.WrapNoKey("no auction overtime"));
            }
            else
            {
                //返回的拍卖品信息中，buyerid=0则无人加价，拍卖失败，无链接
                //buyerid!=0，则拍卖生效，通知栏显示拍卖品被以**价格拍下，点击（返回拍卖品id）可显示买家信息
                return this.Jsonp(this.WrapNoKey(salist.AsEnumerable<SailerAuction>()));
            }
        }

        public class BuyerAuction
        {
            public string AucTitle { get; set; }
            public decimal AucPrice { get; set; }
            public string UserName { get; set; }
            public string UserTel { get; set; }
            public bool? IsTranSuccess { get; set; }
        }

        //获取买家信息
        //返回拍卖品信息（显示拍卖标题,最终价格）和买家信息（买家姓名、联系方式）
        public JsonpResult GetBuyerMsg(int aucid)
        {
            var auction=db.Auctions.Find(aucid);
            var buyerid = auction.Buyerid;
            var buyer = db.Users.Find(buyerid);
            var judge = db.AuctionWishlists.Single(x => x.AucId == aucid && x.UserId == buyerid).IsTransactionSuccessful;
            BuyerAuction ba = new BuyerAuction()
            {
                AucTitle = auction.AucTitle,
                AucPrice = auction.NewPrice,
                UserName = buyer.UserName,
                UserTel = buyer.Tel,
                IsTranSuccess = judge
            };
            return this.Jsonp(this.WrapNoKey(ba));
            //return this.Jsonp(this.WrapNoKey(buyer));
        }

        //线下交易完成后，卖家确认交易，交易达成
        public JsonpResult SailerConfirmAuctionSold(int aucid)
        {
            try
            {
                int buyerid = db.Auctions.Find(aucid).Buyerid;
                db.AuctionWishlists.Single(x => x.UserId == buyerid && x.AucId == aucid).IsTransactionSuccessful = true;
                var usera = db.UserAuctions.Single(x => x.AucId == aucid);
                //用于判断是否超过评价时间（从完成交易后七天）
                usera.CompleteDealTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("successful transaction"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("confirm sold error")); 
            }
        }

        public JsonpResult AuctionTransactionFail(int aucid)
        {
            try
            {
                int buyerid = db.Auctions.Find(aucid).Buyerid;
                db.AuctionWishlists.Single(x => x.UserId == buyerid && x.AucId == aucid).IsTransactionSuccessful = false;
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("transaction fail"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("fail sail error"));
            }
        }

        
    }
}