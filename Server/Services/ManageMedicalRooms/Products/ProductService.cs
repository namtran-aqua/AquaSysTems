using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.ManageMedicalRooms.Products;

namespace AquaSolution.Server.Services.ManageMedicalRooms.Products
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Inventories> _inventoryRepo;

        public ProductService(IRepository<Product> productRepo, IRepository<Inventories> inventoryRepo)
        {
            _productRepo = productRepo;
            _inventoryRepo = inventoryRepo;
        }
        public async Task<bool> CreatedProduct(ProductDto productDto)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Code = productDto.Code,
                Name = productDto.Name,
                Note = productDto.Note,
                ProductType = productDto.ProductType,
                CreatedBy = productDto.CreatedBy,
                CreatedDate = productDto.CreatedDate,
                Unit = productDto.Unit,
                IsHide = false,
            };
            await _productRepo.InsertAsync(product);
            var saveChange = await _productRepo.SaveChangesAsync();
            if (saveChange > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteProduct(Guid productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
            {
                return false;
            }
            return await _productRepo.DeleteAsync(product);
        }

        public async Task<List<ProductDto>> GetListProductByImport()
        {
            var query = from product in await _productRepo.GetQueryableAsync()
                        orderby product.Name
                        select new ProductDto
                        {
                            Id = product.Id,
                            Code = product.Code,
                            Name = product.Name,
                            Note = product.Note,
                            ProductType = product.ProductType,
                            CreatedBy = product.CreatedBy,
                            CreatedDate = product.CreatedDate,
                            UpdatedDate = product.UpdatedDate,
                            UpdateBy = product.UpdateBy,
                            Unit = product.Unit,
                            IsHide = product.IsHide,
                        };
            if (query.Any())
            {
                return query.ToList();
            }
            return new List<ProductDto>();
        }
        public async Task<List<ProductExportDto>> GetListProductByExport()
        {
            var query = from product in await _productRepo.GetQueryableAsync()
                        join inventory in await _inventoryRepo.GetQueryableAsync()
                        on product.Id equals inventory.ProductId
                        orderby product.Name
                        select new ProductExportDto
                        {
                            Id = product.Id,
                            Code = product.Code,
                            Name = product.Name,
                            Note = product.Note,
                            ProductType = product.ProductType,
                            CreatedBy = product.CreatedBy,
                            CreatedDate = product.CreatedDate,
                            UpdatedDate = product.UpdatedDate,
                            UpdateBy = product.UpdateBy,
                            Unit = product.Unit,
                            IsHide = product.IsHide,
                            ExpirationDate = inventory.ExpirationDate,
                            ManufacturingDate =inventory.ManufacturingDate,
                            Quantity = inventory.Quantity,
                            
                        };
            if (query.Any())
            {
                return query.ToList();
            }
            return new List<ProductExportDto>();
        }

        public async Task<bool> UpdateProduct(ProductDto productDto)
        {
            var product = await _productRepo.GetByIdAsync(productDto.Id);
            if (product == null) { return false; }
            product.Code = productDto.Code;
            product.Name = productDto.Name;
            product.Note = productDto.Note;
            product.ProductType = productDto.ProductType;
            product.UpdatedDate = productDto.UpdatedDate;
            product.UpdateBy = productDto.UpdateBy;
            product.Unit = productDto.Unit;
            product.IsHide = productDto.IsHide;
            var update = await _productRepo.UpdateAsync(product);
            return update;
        }
    }
}
