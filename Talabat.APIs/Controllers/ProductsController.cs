using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services;
using Talabat.Core.Specifications;
using Talabat.Core.Specifications.ProductSpecifications;

namespace Talabat.APIs.Controllers
{
    public class ProductsController : ApiBaseController
    {
        //private readonly IGenericRepository<Product> _productRepo;
        //private readonly IGenericRepository<ProductBrand> _brandsRepo;
        //private readonly IGenericRepository<ProductCategory> _categoryRepo;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductsController(
            IProductService productService,
            //IGenericRepository<Product> productRepo,
            //IGenericRepository<ProductBrand> brandsRepo,
            //IGenericRepository<ProductCategory> categoryRepo, 
            IMapper mapper)
        {
            //_productRepo = productRepo;
            //_brandsRepo = brandsRepo;
            //_categoryRepo = categoryRepo;
            _productService = productService;
            _mapper = mapper;
        }
        //[Authorize]
        [CachedAttribute(600)]
        [HttpGet] // GET : /api/Products
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecsParams specParams)
        {

            var products = await _productService.GetProductsAsync(specParams);

            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            var count = await _productService.GetCountAsync(specParams);

            return Ok(new Pagination<ProductToReturnDto>(specParams.PageIndex,specParams.PageSize,count,data));
        }
        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [CachedAttribute(600)]
        [HttpGet("{id}")]
        public  async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductAsync(id);

            if (product == null) 
                return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        [CachedAttribute(600)]
        [HttpGet("brands")] // GET: /api/products/brands
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetBrands()
        {
            var brands = await _productService.GetBrandsAsync();
            return Ok(brands);
        }
        [CachedAttribute(600)]
        [HttpGet("categories")] // GET: /api/products/categories
        public async Task<ActionResult<IReadOnlyList<ProductCategory>>> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }
    }
}
