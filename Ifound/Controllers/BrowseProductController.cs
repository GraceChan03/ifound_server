using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Ifound.Models;
using Ifound.Extensions;
using System.Collections;
using System.Collections.Generic;
using System;
using Ifound.Services;

namespace Ifound.Controllers
{
    //该控制器中的方法为用户作为买家时的功能
    public class BrowseProductController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();
        private readonly IProductService _productService;
        private readonly ICommonService _commonService;

        public BrowseProductController(IProductService productService, ICommonService commonService)
        {
            _productService = productService;
            _commonService = commonService;
        }
        public class ProductObject
        {
            public bool Success { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }
        public ProductObject WrapNoKey(object data)
        {
            return new ProductObject { Data = data, Success = true };
        }

        public JsonpResult ViewAllProducts()
        {
            var products = db.Products.AsEnumerable();
            return this.Jsonp(this.WrapNoKey(products));
        }

        public JsonpResult ViewAllInTypes(int type)
        {
            return this.Jsonp(this.WrapNoKey(_productService.ViewProductsInTypes((GoodsType)type,db)));
        }

        //简单搜索
        public JsonpResult ViewByKeyWords(string key)
        {
            var list = (from p in db.Products
                        where p.PdTitle.Contains(key) || p.PdMsg.Contains(key)
                        select p).ToList();
            return this.Jsonp(this.WrapNoKey(list.AsEnumerable<Product>()));
        }

        //点击某个商品后可看到单个商品详情
        public JsonpResult ViewProduct(int pdid)
        {
            Product product = db.Products.Find(pdid);
            product.PdImage = _commonService.GetIPV4() + product.PdImage;
            return this.Jsonp(this.WrapNoKey(product));
        }

        //收藏商品
        public JsonpResult CollectProduct(int userid,int pdid)
        {
            var judge = db.ProductWishlists.Where(x => x.UserId == userid && x.PdId == pdid);
            if (judge == null || judge.Count() < 1)
            {
                db.ProductWishlists.Add(new ProductWishlist()
                            {
                                UserId = userid,
                                PdId = pdid
                            });
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));//要不要返回id之类的信息？
            }
            else
            {
                return this.Jsonp(this.WrapNoKey("Collected"));
            }
        }

        //查看商品收藏夹（全部商品）
        public JsonpResult ViewProductWishlist(int userid)
        {
            var judge = db.ProductWishlists.Where(x => x.UserId == userid);
            if (judge == null || judge.Count() < 1)
            {
                return this.Jsonp(this.WrapNoKey("no collection"));
            }
            else
            {
                List<Product> products=new List<Product>();
                var pdwishlist = judge.ToList();
                foreach (ProductWishlist w in pdwishlist)
                {
                    Product product = db.Products.Find(w.PdId);
                    products.Add(product);
                }
                return this.Jsonp(this.WrapNoKey(products.AsEnumerable<Product>()));
            }
        }

        //查看收藏夹中单个商品
        //缩减代码？
        public JsonpResult GetProductFromWishlist(int userid, int pdid)
        {
            var judge = db.ProductWishlists.Where(x => x.UserId == userid && x.PdId == pdid);
            if (judge == null || judge.Count() < 1)
            {
                return this.Jsonp(this.WrapNoKey("user and product are not matched"));
            }
            else
            {
                Product product = db.Products.Find(pdid);
                product.PdImage = _commonService.GetIPV4() + product.PdImage;
                return this.Jsonp(this.WrapNoKey(product));
            }
        }

        //删除收藏的商品
        public JsonpResult DeleteFromWishlist(int userid,int pdid)
        {
            try
            {
                var wish = db.ProductWishlists.Single(x => x.UserId == userid && x.PdId == pdid);
                db.ProductWishlists.Attach(wish);
                db.ProductWishlists.Remove(wish);
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }

        }

        //买者按下确认购买后，与卖家进行线下协商，发送提醒信息给卖家，线下成功则卖家承认交易
        public JsonpResult BuyerConfirmPurchase(int buyerid, int pdid)
        {
            try
            {
                var sailerid = db.UserProducts.Single(x => x.PdId == pdid).UserId;
                if (buyerid == 0 || sailerid == 0 || pdid == 0)
                    return this.Jsonp(this.WrapNoKey("exit null id"));
                //禁止购买自己发布的商品
                else if (buyerid == sailerid)
                    return this.Jsonp(this.WrapNoKey("can not buy own product"));
                else
                {
                    var productwish = db.ProductWishlists.Single(x => x.UserId == buyerid && x.PdId == pdid);
                    if (productwish == null)
                        return this.Jsonp(this.WrapNoKey("buyer is error"));//调试客户端用
                    else
                    {
                        productwish.ConfirmPurchase = true;
                        //buyerid不为零说明有买家，提示卖家处理交易
                        db.UserProducts.Single(x => x.UserId == sailerid && x.PdId == pdid).BuyerId = buyerid;
                        //IsOnShelves=false
                        db.Products.Single(x => x.Id == pdid).IsOnShelves = false;
                        db.SaveChanges();
                        return this.Jsonp(this.WrapNoKey("buyer confirm purchase"));
                    }
                }
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("confirm purchase error"));
            }
        }

        //卖者确认售出后，交易达成，才让买者写评论？？
        //获得success信息，提示客户端评价
        public JsonpResult CompleteDeals(int buyerid)
        {
            try
            {
                var wish = db.ProductWishlists.Where(x => x.UserId == buyerid).ToList();
                List<Product> products = new List<Product>();
                foreach(ProductWishlist p in wish)
                {
                    if (p.IsTransactionSuccessful == true)
                    {
                        Product product = db.Products.Find(p.PdId);
                        products.Add(product);
                    }
                }
                if (products.Count <= 0)
                {
                    return this.Jsonp(this.WrapNoKey("no finished deal"));
                }
                else
                {
                    return this.Jsonp(this.WrapNoKey(products.AsEnumerable<Product>()));
                }
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }

        //客户评价，分三部分分数进行评分

        public JsonpResult Evaluate(int buyerid, int pdid, float score1, float score2, float score3)
        {
            try
            {
                var product = db.UserProducts.Single(x => x.PdId == pdid);
                product.ProductScore = score1;
                product.ServiceScore = score2;
                product.OrderProcessingScore = score3;
                var total = (score1 + score2 + score3) / 3;
                product.TotalPoints = total;

                _commonService.SailerAddPoint(product.UserId,total,db);
                //给买家加分
                _commonService.BuyerAddPoint(buyerid, db);
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