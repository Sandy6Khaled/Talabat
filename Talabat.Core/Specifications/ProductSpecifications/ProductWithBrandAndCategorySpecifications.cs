using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifications.ProductSpecifications
{
    public class ProductWithBrandAndCategorySpecifications : BaseSpecifications<Product>
    {
        // This Constractor Will be used for Creating an Object, That will be used to Get All Products
        public ProductWithBrandAndCategorySpecifications(ProductSpecsParams specsParams) : 
            base(P => (string.IsNullOrEmpty(specsParams.Search) || P.Name.ToLower().Contains(specsParams.Search)) && (!specsParams.BrandId.HasValue || P.BrandId == specsParams.BrandId.Value) && (!specsParams.CategoryId.HasValue || P.CategoryId == specsParams.CategoryId.Value))
        {
            AddIncludes();
            if (!string.IsNullOrEmpty(specsParams.Sort))
            {
                switch (specsParams.Sort)
                {
                    case "priceAsc":
                        //OrderBy = P => P.Price;
                        AddOrderBy(P => P.Price);
                        break;
                    case "priceDesc":
                        //OrderByDesc = P => P.Price;
                        AddOrderByDesc(P => P.Price);
                        break;
                    default:
                        AddOrderBy(P => P.Name);
                        break;
                }
            }
            else
                AddOrderBy(P => P.Name);

            // totalProducts = 18
            // pageSize = 5
            // pageIndex = 3

            ApplyPagination((specsParams.PageIndex - 1) * specsParams.PageSize, specsParams.PageSize);
        }
        // This Constractor Will be used for Creating an Object, That will be used to Get a specific Product with Id
        public ProductWithBrandAndCategorySpecifications(int id) : base(P => P.Id == id)
        {
            AddIncludes();
        }
        private void AddIncludes()
        {
            Includes.Add(P => P.Brand);
            Includes.Add(P => P.Category);
        }
    }
}
