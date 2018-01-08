using Ifound.Models;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Ifound.Services
{
    public class CommonService : Ifound.Services.ICommonService
    {
        //返回图片存储在数据库中的路径名(string)
        //pathUType:一直到图片类别的路径,如Server.MapPath("~/Image/Users/")
        //imageName:图片名
        //imageType:图片类别（Users/Products/Auctions...)
        public string SaveImage(string base64, string pathUType, string imageName, string imageType)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream memStream = new MemoryStream(bytes);
            Bitmap bmp = new Bitmap(memStream);
            
            string date = DateTime.Now.ToString("yyyyMMdd");
            string upperPath = pathUType + date + "\\";
            //如果不存在，就新建文件夹
            if (Directory.Exists(upperPath) == false)
                Directory.CreateDirectory(upperPath);
            string path = upperPath + imageName;
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);//保存图片到指定路径
            memStream.Close();
            string savePath = "/Image/" + imageType + "/" + date + "/" + imageName;
            return savePath;
        }

        public string GetIPV4()
        {
            string ipv4 = "";
            System.Net.IPAddress[] addresslist = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (IPAddress ip in addresslist)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ipv4 = ip.ToString();
            }
            return "http://" + ipv4;
        }

        public void SailerAddPoint(int sailerid, float total, IfoundDbContext db)
        {
            var sailer = db.Users.Find(sailerid);
            sailer.SailerPoint = (sailer.SailerPoint + total) / (sailer.CompleteOrderTimes + 1);
            sailer.CompleteOrderTimes++;
        }
        public void BuyerAddPoint(int buyerid, IfoundDbContext db)
        {
            var buyer = db.Users.Find(buyerid);
            buyer.EvaluateTimes++;
            buyer.BuyerPoint++;
        }
    }
}