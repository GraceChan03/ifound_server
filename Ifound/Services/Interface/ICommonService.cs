using Ifound.Models;
using System;
namespace Ifound.Services
{
    public interface ICommonService
    {
        string SaveImage(string base64, string pathUType, string imageName, string imageType);
        string GetIPV4();
        void SailerAddPoint(int sailerid, float total, IfoundDbContext db);
        void BuyerAddPoint(int buyerid, IfoundDbContext db);
    }
}
