using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ifound.Models;
using System.IO;
using System.Drawing;
using System.Net.Sockets;

namespace Ifound.Controllers
{
    public class TestPostController : Controller
    {
        private IfoundDbContext db = new IfoundDbContext();

        [HttpPost]
        public void Test(string base64)
        {
            int id = 9;
            byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream memStream = new MemoryStream(bytes);
            Bitmap bmp = new Bitmap(memStream);

            string date = DateTime.Now.ToString("yyyyMMdd");
            string uppath = Server.MapPath("~/Image/") + date + "\\";
            //如果不存在，就新建文件夹
            if (Directory.Exists(uppath) == false)
            {
                Directory.CreateDirectory(uppath);
            }
            //普通二手商品
            var product = db.Products.Find(id);
            //图片命名
            string image = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + id + product.PdType + ".jpg";
            //图片存储路径
            string path = uppath + image;
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            string ipv4 = "";
            System.Net.IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (IPAddress ip in addressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ipv4 = ip.ToString();
            }
            //URL获取图片时用的绝对路径，存储在数据库中
            string savepath = ipv4 + "/Image/" + date + "/" + image;
            product.PdImage = savepath;
            db.SaveChanges();
            memStream.Close();
        }

        // GET: /TestPost/
        public ActionResult Index()
        {
            return View(db.Students.ToList());
        }

        // GET: /TestPost/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: /TestPost/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /TestPost/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,StudentId,StudentClass,StudentName")] Student student)
        {
            if (ModelState.IsValid)
            {
                db.Students.Add(student);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(student);
        }

        // GET: /TestPost/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: /TestPost/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,StudentId,StudentClass,StudentName")] Student student)
        {
            if (ModelState.IsValid)
            {
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(student);
        }

        // GET: /TestPost/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: /TestPost/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Student student = db.Students.Find(id);
            db.Students.Remove(student);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
