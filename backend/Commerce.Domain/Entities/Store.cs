namespace Commerce.Domain.Entities;

public class Store
{
    public Guid Id { get; set; }
    public Guid MerchantId{  get; set; }
    public Merchant Merchant { get; set; } = null!;

    public string ShopifyDomain { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
