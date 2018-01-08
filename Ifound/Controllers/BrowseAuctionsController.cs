using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ifound.Models;
using Ifound.Extensions;
using Ifound.Services;

namespace Ifound.Controllers
{
    public class BrowseAuctionsController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();
        private readonly IAuctionService _auctionService;
        private readonly ICommonService _commonService;

        public BrowseAuctionsController() { }
        public BrowseAuctionsController(IAuctionService auctionService, ICommonService commonService)
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

        //显示所有拍卖品
        public JsonpResult ViewAllAuctions()
        {
            try
            {
                var auctions = db.Auctions.AsEnumerable();
                return this.Jsonp(this.WrapNoKey(auctions));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }

        //按关键字搜索拍卖品
        public JsonpResult ViewByKeyWords(string key)
        {
            var list = (from a in db.Auctions
                        where a.AucTitle.Contains(key) || a.AucMsg.Contains(key)
                        select a).ToList();
            return this.Jsonp(this.WrapNoKey(list.AsEnumerable<Auction>()));
        }

        //浏览单个拍卖品信息
        public JsonpResult ViewAuction(int aucid)
        {
            Auction auction = db.Auctions.Find(aucid);
            auction.AucImage = _commonService.GetIPV4() + auction.AucImage;
            return this.Jsonp(this.WrapNoKey(auction));
        }

        //买家加价
        //拍卖品表中最新价格、买家id更新
        //拍卖品被自动添加到买家收藏夹中
        public JsonpResult AddPrice(int buyerid, int aucid, decimal newprice)
        {
            var sailerid = db.UserAuctions.Single(x => x.AucId == aucid).UserId;
            if (buyerid == sailerid)
                return this.Jsonp(this.WrapNoKey("cannot buy own auction"));
            var auction = db.Auctions.Find(aucid);
            //判断买家加的价格是否大于拍卖品表中的newprice，若低于，说明有信息延时，提示买家重新加价
            if (newprice < auction.NewPrice)
                return this.Jsonp(this.WrapNoKey("newprice is lower"));
            else
            {
                auction.NewPrice = newprice;
                auction.Buyerid = buyerid;
                db.SaveChanges();
            }
            
            var isAuctionExist = db.AuctionWishlists.Where(x => x.UserId == buyerid && x.AucId == aucid);
            if (isAuctionExist == null || isAuctionExist.Count() < 1)
            {
                db.AuctionWishlists.Add(new AuctionWishlist()
                            {
                                UserId = buyerid,
                                AucId = aucid,
                                NewAucPrice = newprice
                            });
                db.SaveChanges();
            }
            else
            {
                db.AuctionWishlists.Single(x => x.UserId == buyerid && x.AucId == aucid).NewAucPrice = newprice;
                db.SaveChanges();
            }
            return this.Jsonp(this.WrapNoKey("OK"));
        }

        //主动将拍卖品加进收藏夹
        public JsonpResult CollectAuction(int userid, int aucid)
        {
            var sailerid = db.UserAuctions.Single(x => x.AucId == aucid).UserId;
            if (userid == sailerid)
                return this.Jsonp(this.WrapNoKey("cannot collect own auction"));
            else
            {
                var judge = db.AuctionWishlists.Where(x => x.UserId == userid && x.AucId == aucid);
                if (judge == null || judge.Count() < 1)
                {
                    db.AuctionWishlists.Add(new AuctionWishlist()
                                {
                                    UserId = userid,
                                    AucId = aucid
                                });
                    db.SaveChanges();
                }
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            
        }

        //浏览自己收藏夹中的所有拍卖品
        public JsonpResult ViewAuctionWishlist(int userid)
        {
            var judge = db.AuctionWishlists.Where(x => x.UserId == userid);
            if (judge == null || judge.Count() < 1)
            {
                return this.Jsonp(this.WrapNoKey("no auction"));
            }
            else
            {
                List<Auction> auctions = new List<Auction>();
                var aucwishlist = judge.ToList();
                foreach (AuctionWishlist w in aucwishlist)
                {
                    Auction auction = db.Auctions.Find(w.AucId);
                    auctions.Add(auction);
                }
                return this.Jsonp(this.WrapNoKey(auctions.AsEnumerable<Auction>()));
            }
        }

        //浏览收藏夹中的单个拍卖品详情
        public JsonpResult GetAuctionFromWishlist(int userid, int aucid)
        {
            var judge = db.AuctionWishlists.Where(x => x.UserId == userid && x.AucId == aucid);
            if (judge == null || judge.Count() < 1)
            {
                return this.Jsonp(this.WrapNoKey("not matched"));
            }
            else
            {
                Auction auction = db.Auctions.Find(aucid);
                auction.AucImage = _commonService.GetIPV4() + auction.AucImage;
                return this.Jsonp(this.WrapNoKey(auction));
            }
        }

        //删除条件？？
        //从收藏夹中删除拍卖品
        public JsonpResult DeleteFromWishlist(int userid, int aucid)
        {
            try
            {
                var auction = db.Auctions.Find(aucid);
                if (!_auctionService.IsOverTime(auction.EndTime))
                {
                    return this.Jsonp(this.WrapNoKey("not over endtime"));
                }
                else
                {
                    var wish = db.AuctionWishlists.Single(x => x.UserId == userid && x.AucId == aucid);
                    db.AuctionWishlists.Attach(wish);
                    db.AuctionWishlists.Remove(wish);
                    db.SaveChanges();
                    return this.Jsonp(this.WrapNoKey("OK"));
                }
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }

        public string DeleteAucWishFromTable(int id)
        {
            try
            {
                var aw = db.AuctionWishlists.Find(id);
                db.AuctionWishlists.Attach(aw);
                db.AuctionWishlists.Remove(aw);
                db.SaveChanges();
                return "OK";
            }
            catch
            {
                return "ERROR";
            }
        }

        public class BuyerAuction
        {
            public int AucId { get; set; }
            public string AucTitle { get; set; }
            public decimal AucPrice { get; set; }//目前拍卖品价格
            public decimal NewAucPrice { get; set; }//买家出的价
            public string Option { get; set; }
            public string CompleteDealTime { get; set; }
        }

        //处理基于拍卖时间、其他人加价、拍卖是否成功做的处理
        //会出现四种情况，三种用于显示：
        //1表示未到时间有其他人加价；2表示到时间拍中商品；3表示到时间没拍中；4表示到时间，卖家确认交易成功，待买家评价
        public JsonpResult BuyerAuctionEvent(int userid)
        {
            var awlist = db.AuctionWishlists.Where(x => x.UserId == userid).ToList();
            List<BuyerAuction> balist = new List<BuyerAuction>();
            foreach (AuctionWishlist aw in awlist)
            {
                var auction = db.Auctions.Find(aw.AucId);
                var userPrice = aw.NewAucPrice;
                var auctionPrice = auction.NewPrice;
                var ba = new BuyerAuction()
                {
                    AucId = auction.Id,
                    AucTitle = auction.AucTitle,
                    AucPrice = auctionPrice,
                    NewAucPrice = userPrice,
                    CompleteDealTime = db.UserAuctions.Single(x => x.AucId == aw.AucId).CompleteDealTime
                };
                if (!(_auctionService.IsOverTime(auction.EndTime)) && userPrice < auctionPrice)
                {
                    ba.Option = "1";
                    balist.Add(ba);
                }
                else if (_auctionService.IsOverTime(auction.EndTime))
                {
                    var buyerid = db.Auctions.Find(aw.AucId).Buyerid;
                    var ua=db.UserAuctions.Single(x=>x.AucId==aw.AucId);
                    if (userid == buyerid)
                    {
                        ba.Option = "2";
                        balist.Add(ba);
                    }
                    else
                    {
                        ba.Option = "3";
                        balist.Add(ba);
                    }
                    if (aw.IsTransactionSuccessful == true && ua.Evaluated == false)
                    {
                        ba.Option = "4";
                        balist.Add(ba);
                    }
                }
            }
            if (balist.Count() == 0)
                return this.Jsonp(this.WrapNoKey("no events"));
            else
                return this.Jsonp(this.WrapNoKey(balist.AsEnumerable<BuyerAuction>()));
        }

        

        public class SailerAuction
        {
            public string AucTitle { get; set; }
            public decimal AucPrice { get; set; }
            public decimal NewAucPrice { get; set; }
            public string UserName { get; set; }
            public string UserTel { get; set; }
        }

        public JsonpResult GetSailerDetails(int buyerid, int aucid)
        {
            try
            {
                var auction = db.Auctions.Find(aucid);
                var sailerid = db.UserAuctions.Single(x => x.AucId == aucid).UserId;
                var user = db.Users.Find(sailerid);
                var aw = db.AuctionWishlists.Single(x => x.UserId == buyerid && x.AucId == aucid);
                SailerAuction sa = new SailerAuction()
                {
                    AucTitle = auction.AucTitle,
                    AucPrice = auction.NewPrice,
                    NewAucPrice = aw.NewAucPrice,
                    UserName = user.UserName,
                    UserTel = user.Tel
                };
                return this.Jsonp(this.WrapNoKey(sa));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }


        //买家评价后进行的得分操作
        public JsonpResult Evaluate(int buyerid, int aucid, float score1, float score2, float score3)
        {
            try
            {
                var usera = db.UserAuctions.Single(x => x.AucId == aucid);
                usera.ProductScore = score1;
                usera.ServiceScore = score2;
                usera.OrderProcessingScore = score3;
                var total = (score1 + score2 + score3) / 3;
                usera.TotalPoints = total;

                _commonService.SailerAddPoint(usera.UserId, total, db);
                _commonService.BuyerAddPoint(buyerid, db);
                usera.Evaluated = true;
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }

        //超过评价时间做出处理
        public JsonpResult OverEvaluationTime(int aucid)
        {
            try
            {
                var userid = db.UserAuctions.Single(x => x.AucId == aucid).UserId;
                var usera = db.UserAuctions.Single(x => x.AucId == aucid);
                usera.ProductScore = usera.ServiceScore = usera.OrderProcessingScore = usera.TotalPoints = 5;
                usera.Evaluated = true;
                _commonService.SailerAddPoint(userid, 5, db);
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }
    }
}
