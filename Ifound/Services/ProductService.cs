using System.Collections.Generic;
using System.Linq;
using Ifound.Models;

namespace Ifound.Services
{
    public class ProductService : Ifound.Services.IProductService
    {
        public IEnumerable<Product> ViewProductsInTypes(GoodsType type,IfoundDbContext db)
        {
            var products = db.Products.Where(x => x.PdType == type).AsEnumerable();
            return products;
        }
        
    }
}