using System;
using Ifound.Models;
namespace Ifound.Services
{
    public interface IAuctionService
    {
        string GetRemainingTime(string endtime);
        bool IsOverTime(string endtime);
        System.Collections.Generic.IEnumerable<Auction> ViewProductsInTypes(GoodsType type, IfoundDbContext db);
    }
}
