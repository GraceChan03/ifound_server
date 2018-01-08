using System;
namespace Ifound.Services
{
    public interface IProductService
    {
        System.Collections.Generic.IEnumerable<Ifound.Models.Product> ViewProductsInTypes(Ifound.Models.GoodsType type, Ifound.Models.IfoundDbContext db);
    }
}
