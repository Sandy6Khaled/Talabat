using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifications.ProductSpecifications
{
    public class ProductsWithFilterationForCountSpecifications : BaseSpecifications<Product>
    {
        public ProductsWithFilterationForCountSpecifications(ProductSpecsParams specsParams) 
            :base(P => (string.IsNullOrEmpty(specsParams.Search) || P.Name.ToLower().Contains(specsParams.Search)) && (!specsParams.BrandId.HasValue || P.BrandId == specsParams.BrandId.Value) && (!specsParams.CategoryId.HasValue || P.CategoryId == specsParams.CategoryId.Value))
        {
            
        }
    }
}
