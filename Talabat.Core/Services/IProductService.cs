using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications.ProductSpecifications;

namespace Talabat.Core.Services
{
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetProductsAsync(ProductSpecsParams specParams);
        Task<Product?> GetProductAsync(int productId);
        Task<int> GetCountAsync(ProductSpecsParams specParams);
        Task<IReadOnlyList<ProductBrand>> GetBrandsAsync();
        Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync();
    }
}
