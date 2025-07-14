namespace DTOs.ApiDtos
{
    public class ProductDTO
    {
        public Guid Id { get; init; }
        public string ProductName { get; init; } = null!;
        public string Description { get; init; } = null!;
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public string Category { get; init; } = null!;
    }
}
