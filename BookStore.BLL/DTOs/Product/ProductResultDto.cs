using ShpoNest.Models.Enums;

namespace ShopNest.BLL.DTOs.Product
{
    public class ProductResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        

        // Book Specs
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? ISBN { get; set; }
        public int? PublicationYear { get; set; }
        public int? Pages { get; set; }
        public string? Language { get; set; }
        public string? Edition { get; set; }
        public BookFormat? Format { get; set; }

        public string? MainImagePath { get; set; }
        public List<ProductImageResultDto> Images { get; set; } = new();
    }
}