using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Ifound.Models;
using Ifound.Extensions;
using Ifound.Services;
using System;

namespace Ifound.Controllers
{
    public class UserController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();
        private readonly IUserService _userService;
        private readonly ICommonService _commonService;

        public UserController(IUserService userService, ICommonService commonService)
        {
            _userService = userService;
            _commonService = commonService;
        }

        public class UserObject
        {
            public bool Success { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }

        private UserObject WrapNoKey(object data)
        {
            return new UserObject { Data = data, Success = true };
        }
        private UserObject Wrap(object data, string msg)
        {
            return new UserObject { Data = data, Success = true, Message = msg };
        }

        public JsonpResult Login(string userno,string password)
        {
            var iq = db.Users.Where(x => x.UserNo == userno && x.Pswd == password);
            if (iq != null && iq.Count() >= 1)
            {
                var user = _userService.GetUserByUserNo(userno);
                return this.Jsonp(this.WrapNoKey(user.Id));
            }
            return this.Jsonp(this.Wrap("login failed","msg test"));
        }

        public JsonpResult Register(string username, string pswd, string userno, string tel, string sign, string usericon)
        {
            var judge = db.Students.Where(x => x.StudentId == userno && x.StudentName == username);
            var isUReg = db.Users.Where(x => x.UserNo == userno);
            ////判断学生表中是否有相应学生：必须保证姓名和学号信息与学生表中信息一致
            //if (judge == null || judge.Count() < 1)
            //{
            //    return this.Jsonp(this.WrapNoKey("Invalid userno or username"));
            //}
            //判断当前注册用学号是否已被注册
            if (isUReg != null && isUReg.Count() >= 1)
            {
                return this.Jsonp(this.WrapNoKey("Registered"));
            }
            else
            {
                db.Users.Add(new User() 
                {
                    UserName=username,
                    Pswd=pswd,
                    UserNo=userno,
                    Tel=tel,
                    Sign=sign,
                    UserIcon=usericon,
                    Sex=(Gender)1
                });
                db.SaveChanges();
                var user = _userService.GetUserByUserNo(userno);
                return this.Jsonp(this.WrapNoKey(user.Id));
            }
        }

        public JsonpResult EditIcon(int userid, string base64)
        {
            try
            {
                User user = db.Users.Find(userid);
                string pathUType = Server.MapPath("~/Image/Users/");
                string imageName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + userid + ".jpg";
                if (user.UserIcon != null)
                {
                    string savepath = user.UserIcon;
                    string newpath = savepath.Replace('/', '\\');
                    string path = Server.MapPath("~/") + newpath.Substring(newpath.IndexOf("\\") + 1);
                    System.IO.File.Delete(path);
                }
                user.UserIcon = _commonService.SaveImage(base64, pathUType, imageName, "Users");
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("OK"));
            }
            catch
            {
                return this.Jsonp(this.WrapNoKey("ERROR"));
            }
        }

        public JsonpResult GetUserMsg(int userid)
        {
            User user = db.Users.Single(x => x.Id == userid);
            return this.Jsonp(this.WrapNoKey(user));
        }

        public JsonpResult EditTel(int userid, string tel)
        {
            User user = db.Users.Find(userid);
            user.Tel = tel;
            db.SaveChanges();
            return this.Jsonp(this.WrapNoKey("1"));
        }

        public JsonpResult EditSign(int userid, string sign)
        {
            User user = db.Users.Find(userid);
            user.Sign = sign;
            db.SaveChanges();
            return this.Jsonp(this.WrapNoKey("1"));
        }

        public JsonpResult EditSex(int userid, int sex)
        {
            User user = db.Users.Find(userid);
            user.Sex = (Gender)sex;
            db.SaveChanges();
            return this.Jsonp(this.WrapNoKey("1"));
        }

        //改造自Edit
        public JsonpResult EditBasicMsg(int userid, string tel, string sign, string usericon, int sex)
        {
            var u = db.Users.Find(userid);
            User user = new User() 
            {
                Id=userid,
                UserName=u.UserName,
                Pswd=u.Pswd,
                UserNo=u.UserNo,
                Tel=tel,
                Sign=sign,
                UserIcon=usericon,
                BuyerPoint=u.BuyerPoint,
                Sex=(Gender)sex
            };
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey(user.Id));
            }
            return this.Jsonp(this.WrapNoKey("Error"));
        }

        public JsonpResult ChangePswd(int userid, string oldpswd, string newpswd1, string newpswd2)
        {
            User user = db.Users.Single(x => x.Id == userid);
            if (oldpswd != user.Pswd)
                return this.Jsonp(this.WrapNoKey("wrong old password"));
            else if (newpswd1 != newpswd2)
                return this.Jsonp(this.WrapNoKey("wrong new password"));
            else
            {
                user.Pswd = newpswd1;
                db.SaveChanges();
                return this.Jsonp(this.WrapNoKey("1"));
            }
        }

        public JsonpResult Delete(int id)
        {
            var user = db.Users.Find(id);
            db.Users.Attach(user);
            db.Users.Remove(user);
            db.SaveChanges();
            return this.Jsonp(this.WrapNoKey("ok"));
        }
    }
}
