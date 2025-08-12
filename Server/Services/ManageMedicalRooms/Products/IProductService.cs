using AquaSolution.Shared.ManageMedicalRooms.Products;

namespace AquaSolution.Server.Services.ManageMedicalRooms.Products
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetListProductByImport();
        Task<List<ProductExportDto>> GetListProductByExport();

        Task<bool> DeleteProduct(Guid productId);
        Task<bool> CreatedProduct(ProductDto productDto);
        Task<bool> UpdateProduct(ProductDto productDto);

    }
}
