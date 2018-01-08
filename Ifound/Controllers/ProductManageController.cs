using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Ifound.Models;
using Ifound.Extensions;
using System.Collections.Generic;
using System;
using Ifound.Services;

namespace Ifound.Controllers
{
    //该控制器中的方法为用户作为卖家时的功能
    public class ProductManageController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();
        private readonly IProductService _productService;
        private readonly ICommonService _commonService;

        public ProductManageController(IProductService productService, ICommonService commonService)
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

        //用户添加自己要卖的二手商品信息，发布
        public JsonpResult AddProduct(int userid, string pdtitle, int pdtype, string pdprice, string pdmsg)
        {
            try
            {
                db.Products.Add(new Product()
                {
                    PdTitle = pdtitle,
                    PdType = (GoodsType)pdtype,
                    PdPrice = decimal.Parse(pdprice),
                    PdMsg = pdmsg,
                    LaunchTime=DateTime.Now.ToString(),
                    IsOnShelves=true
                });
                db.SaveChanges();
                //找到最后一个product的id号，填入关联表/
                var pdid = db.Products.AsEnumerable().Last().Id;
                db.UserProducts.Add(new UserProduct()
                {
                    UserId = userid,
                    PdId = pdid
                });
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey(pdid));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("add product failed"));
            }
        }

        //添加商品图片，也用于修改商品图片
        public JsonpResult AddProPicture(int pdid, string base64)
        {
            try
            {
                var product = db.Products.Find(pdid);
                string pathUType = Server.MapPath("~/Image/Products/");
                string imageName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + pdid + "_" + (int)product.PdType + ".jpg";
                if (product.PdImage != null)
                {
                    string savepath = product.PdImage;
                    string newpath = savepath.Replace('/', '\\');
                    string path = Server.MapPath("~/") + newpath.Substring(newpath.IndexOf("\\") + 1);
                    System.IO.File.Delete(path);
                }
                product.PdImage = _commonService.SaveImage(base64, pathUType, imageName, "Products");
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("ERROR"));
            }
        }

        //显示自己发布的所有商品
        //还没做：查看有无人购买->改成查看被收藏数量？
        public JsonpResult ShowOwnProducts(int userid)
        {
            try
            {
                var judge = db.UserProducts.Where(x => x.UserId == userid);
                string ipv4 = _commonService.GetIPV4();
                if (judge == null || judge.Count() < 1)
                    return this.Jsonp(this.WrapNoKey("no records"));
                else
                {
                    var userpd = judge.ToList();
                    List<Product> products = new List<Product>();
                    foreach (UserProduct j in userpd)
                    {
                        Product product = db.Products.Find(j.PdId);
                        product.PdImage = ipv4 + product.PdImage;
                        products.Add(product);
                    }
                    return this.Jsonp(this.WrapNoKey(products.AsEnumerable<Product>()));
                }
                
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("error"));
            }
        }

        //获取单个商品信息
        public JsonpResult GetProduct(int userid,int pdid)
        {
            var judge = db.UserProducts.Where(x => x.UserId == userid && x.PdId == pdid);
            string ipv4 = _commonService.GetIPV4();
            if (judge == null || judge.Count() < 1)
            {
                return this.Jsonp(this.WrapNoKey("user and product are not matched"));
            }
            else
            {
                Product product = db.Products.Find(pdid);
                product.PdImage = ipv4 + product.PdImage;
                return this.Jsonp(this.WrapNoKey(product));
            }
        }

        //修改单个商品的信息
        //需不需要判断用户和商品匹配？
        public JsonpResult EditProduct(int pdid,string title,int type,decimal price,string msg)
        {
            var product = db.Products.Find(pdid);
            product.PdTitle = title;
            product.PdType = (GoodsType)type;
            product.PdPrice = price;
            product.PdMsg = msg;
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey(product.Id));
            }
            return this.Jsonp(this.WrapNoKey("Error"));
        }

        public void DeleteProductFromTable(int pdid)
        {
            var product = db.Products.Find(pdid);
            var userp = db.UserProducts.Single(x => x.PdId == pdid);
            if (product.PdImage != null)
            {
                string savepath = product.PdImage;
                string newpath = savepath.Replace('/', '\\');
                string path = Server.MapPath("~/") + newpath.Substring(newpath.IndexOf("\\") + 1);
                System.IO.File.Delete(path);
            }
            db.Products.Attach(product);
            db.Products.Remove(product);
            db.UserProducts.Attach(userp);
            db.UserProducts.Remove(userp);
            db.SaveChanges();
        }

        //删除发布的商品
        public JsonpResult DeleteProduct(int pdid)
        {
            try 
            {
                var product = db.Products.Find(pdid);
                product.IsOnShelves = false;
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }

        //判断是否有买家按下了购买键（间接使商品下架了），buyerid!=0时，有买家，通知卖家处理
        public JsonpResult HaveBuyer(int sailerid)
        {
            try
            {
                var userp = db.UserProducts.Where(x => x.UserId == sailerid).ToList();
                List<Product> products = new List<Product>();
                foreach (UserProduct u in userp)
                {
                    if (u.BuyerId != 0)
                    {
                        Product product = db.Products.Find(u.PdId);
                        products.Add(product);
                    }
                }
                if (products.Count <= 0)
                {
                    return this.Jsonp(this.WrapNoKey("No buyer"));
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

        //卖家在通知栏中点击“有买家购买**商品”的链接后，打开具体信息页面
        public JsonpResult GetBuyerDetails(int pdid)
        {
            try
            {
                var uid = db.UserProducts.Single(x => x.PdId == pdid).BuyerId;
                User user = db.Users.Find(uid);
                Product product = db.Products.Find(pdid);
                List<object> list = new List<object>();
                list.Add(user);
                list.Add(product);
                return this.Jsonp(this.WrapNoKey(list.AsEnumerable<object>()));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }

        //买家确定购买后，当卖家也确定售出，交易达成
        public JsonpResult SailerConfirmSold(int sailerid, int pdid)
        {
            try
            {
                int buyerid = db.UserProducts.Single(x => x.PdId == pdid).BuyerId;
                if (buyerid == 0 || sailerid == 0 || pdid == 0)
                {
                    return this.Jsonp(this.WrapNoKey("exit null id"));
                }
                else
                {
                    var userp = db.UserProducts.Single(x => x.UserId == sailerid && x.PdId == pdid);
                    //用于买家判断，交易成功了才进入评价
                    db.ProductWishlists.Single(x => x.UserId == buyerid && x.PdId == pdid).IsTransactionSuccessful = true;
                    userp.CompleteDealTime = DateTime.Now;
                    db.SaveChanges();
                    return this.Jsonp(this.WrapNoKey("successful transaction"));
                }
                    
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("confirm sold error"));
            }
        }

        //线下交易失败
        public JsonpResult TransactionFail(int sailerid, int pdid)
        {
            try
            {
                int buyerid = db.UserProducts.Single(x => x.PdId == pdid).BuyerId;
                if (buyerid == 0 || sailerid == 0 || pdid == 0)
                {
                    return this.Jsonp(this.WrapNoKey("exit null id"));
                }
                else
                {
                    var sailer = db.UserProducts.Single(x => x.UserId == sailerid && x.PdId == pdid);
                    sailer.BuyerId = 0;
                    //卖家认为交易没有发生，交易失败，使商品重新上架
                    db.Products.Single(x => x.Id == pdid).IsOnShelves = true;
                    //用于买家判断，交易失败了提示买家
                    db.ProductWishlists.Single(x => x.UserId == buyerid && x.PdId == pdid).IsTransactionSuccessful=false;
                    db.SaveChanges();
                    return this.Jsonp(this.WrapNoKey("transaction fail"));
                }
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("deny sail error"));
            }
        }

        //定时访问，判断是否超过评价时间
        public JsonpResult OverEvaluationTime(int pdid)
        {
            try
            {
                DateTime now = DateTime.Now;
                var judge = db.UserProducts.Where(x => x.PdId == pdid);
                DateTime completeDealTime = (DateTime)db.UserProducts.Single(x => x.PdId == pdid).CompleteDealTime;
                if (judge == null || judge.Count() < 1)
                {
                    return this.Jsonp(this.WrapNoKey("not complete deal"));
                }
                else
                {
                    TimeSpan span = now - completeDealTime;
                    double day = span.TotalDays;
                    var userid = db.UserProducts.Single(x => x.PdId == pdid).UserId;
                    if (day >= 7)
                    {
                        var userp = db.UserProducts.Single(x => x.PdId == pdid);
                        userp.ProductScore = userp.ServiceScore = userp.OrderProcessingScore = userp.TotalPoints = 5;
                        _commonService.SailerAddPoint(userid, 5, db);
                        db.SaveChanges();
                        return this.Jsonp(this.WrapNoKey("over evaluate time"));
                    }
                    else
                    {
                        return this.Jsonp(this.WrapNoKey("in evaluate time"));
                    }
                }
                
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
            
        }
    }
}
