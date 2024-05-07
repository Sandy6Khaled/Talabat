using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Services;
using Talabat.Core.Specifications.ProductSpecifications;

namespace Talabat.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IReadOnlyList<ProductBrand>> GetBrandsAsync()
            => await _unitOfWork.Repository<ProductBrand>().GetAllAsync();

        public async Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync()
            => await _unitOfWork.Repository<ProductCategory>().GetAllAsync();

        public async Task<int> GetCountAsync(ProductSpecsParams specParams)
        {
            var countSpec = new ProductsWithFilterationForCountSpecifications(specParams);

            var count = await _unitOfWork.Repository<Product>().GetCountAsync(countSpec);

            return count;
        }

        public async Task<Product?> GetProductAsync(int productId)
        {
            var spec = new ProductWithBrandAndCategorySpecifications(productId);

            var product = await _unitOfWork.Repository<Product>().GetWithSpecAsync(spec);

            return product;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync([FromQuery] ProductSpecsParams specParams)
        {

            var spec = new ProductWithBrandAndCategorySpecifications(specParams);

            var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
            
            return products;
        }
    }
}
