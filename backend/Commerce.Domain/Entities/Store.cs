namespace Commerce.Domain.Entities;

public class Store
{
    public Guid Id { get; set; }

    public string ShopifyDomain { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
