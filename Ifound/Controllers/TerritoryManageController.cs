using System.Web.Http;
using System;
using System.Web.Mvc;
using System.Data.Entity;
using System.Linq;
using Ifound.Models;
using Ifound.Extensions;
using Ifound.Services;

namespace Ifound.Controllers
{
    public class TerritoryManageController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();
        private readonly ICommonService _commonService;

        public TerritoryManageController(ICommonService commonService)
        {
            _commonService = commonService;
        }
        public class TerritoryObject
        {
            public bool Success { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }
        public TerritoryObject WrapNoKey(object data)
        {
            return new TerritoryObject { Data = data, Success = true };
        }
        //添加地盘里的新纪录
        public JsonpResult AddTerritory(int userid, string site, string remark)
        {
            try
            {
                db.Territories.Add(new Territory()
                {
                    UserId = userid,
                    Site = site,
                    Remark = remark,
                    Time = DateTime.Now.ToString("yyyy/MM/dd")
                });
                db.SaveChanges();
                var tid = db.Territories.AsEnumerable().Last().Id;
                return this.Jsonp(this.WrapNoKey(tid));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("add territory fail"));
            }
        }
        //添加图片
        public JsonpResult AddTerPicture(int tid, string base64)
        {
            try
            {
                var territory = db.Territories.Find(tid);
                string pathUType = Server.MapPath("~/Image/Territories/");
                string imageName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + tid + ".jpg";
                territory.Image = _commonService.SaveImage(base64, pathUType, imageName, "Territories");
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("ERROR"));
            }
        }
        //显示自己
        public JsonpResult ShowOwnTerritories(int userid)
        {
            try
            {
                var judge = db.Territories.Where(x => x.UserId == userid);
                string ipv4 = _commonService.GetIPV4();
                if (judge == null || judge.Count() < 1)
                    return this.Jsonp(this.WrapNoKey("no records"));
                else
                {
                    var territories = judge.ToList();
                    for (int i = 0; i < territories.Count; i++)
                    {
                        territories[i].Image = ipv4 + territories[i].Image;
                    }
                    return this.Jsonp(this.WrapNoKey(territories.AsEnumerable<Territory>()));
                }
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("error"));
            }
        }
        //获取单个
        public JsonpResult GetTerritory(int tid)
        {
            var judge = db.Territories.Where(x => x.Id == tid);
            if (judge == null || judge.Count() < 1)
                return this.Jsonp(this.WrapNoKey("no territory"));
            else
            {
                var territory = db.Territories.Find(tid);
                string ipv4 = _commonService.GetIPV4();
                territory.Image = ipv4 + territory.Image;
                return this.Jsonp(this.WrapNoKey(territory));
            }
        }

        public JsonpResult DeleteTerritory(int tid)
        {
            try
            {
                var territory = db.Territories.Find(tid);
                db.Territories.Attach(territory);
                db.Territories.Remove(territory);
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("ERROR"));
            }
        }
    }
}