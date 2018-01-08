using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ifound.Models;
using Ifound.Services;
using Ifound.Extensions;

namespace Ifound.Controllers
{
    public class BrowseTerritoryController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();
        private readonly ICommonService _commonService;

        public BrowseTerritoryController(ICommonService commonService)
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
        public TerritoryObject Wrap(object data, string message)
        {
            return new TerritoryObject { Data = data, Success = true, Message = message };
        }
        //显示所有,不管是自己的，还是别人发布的
        public JsonpResult ViewAllTerritories()
        {
            try
            {
                var territories = db.Territories.AsEnumerable().ToList();
                var ipv4 = _commonService.GetIPV4();
                for (int i = 0; i < territories.Count; i++)
                {
                    territories[i].Image = ipv4 + territories[i].Image;
                }
                return this.Jsonp(this.WrapNoKey(territories.AsEnumerable<Territory>()));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("Error"));
            }
        }
        //显示单个，展示某条记录的细节
        //msg用于表示id为userid的用户是否点了赞，0代表没点赞，1代表点了赞
        public JsonpResult ViewTerritory(int tid, int userid)
        {
            var territory = db.Territories.Find(tid);
            territory.Image = _commonService.GetIPV4() + territory.Image;
            var judge = db.Praises.Where(x => x.TId == tid && x.UserId == userid);
            string msg;
            if (judge == null || judge.Count() < 1)
                msg = "0";
            else
                msg = "1";
            return this.Jsonp(this.Wrap(territory,msg));
        }
        //点赞、取消赞
        //userid为点赞人/取消赞人的ID号
        public JsonpResult OptIn(int tid, int userid, int point)
        {
            try
            {
                var territory = db.Territories.Find(tid);
                //territory.PraiseTimes += point;
                if (point == 1)
                {
                    db.Praises.Add(new Praise()
                    {
                        TId = tid,
                        UserId = userid
                    });
                    db.SaveChanges();
                }
                else if (point == -1)
                {
                    var praise = db.Praises.Single(x => x.TId == tid && x.UserId == userid);
                    db.Praises.Attach(praise);
                    db.Praises.Remove(praise);
                    db.SaveChanges();
                }
                territory.PraiseTimes = db.Praises.Where(x => x.TId == tid).ToList().Count;
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
