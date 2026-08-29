using Commerce.Domain.Entities;
using Commerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Infrastructure.Stores;

public class StoreService
{
    private readonly ApplicationDbContext _dbContext;

    public StoreService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    //Give me one store owned by this merchant
    public async Task<Store?> GetByIdAsync(
        Guid storeId,
        Guid merchantId)
    {
        return await _dbContext.Stores
            .SingleOrDefaultAsync(store =>
                store.Id == storeId &&
                store.MerchantId == merchantId);
    }

    //Give me all stores owned by this merchant
    public async Task<List<Store>> GetAllAsync(Guid merchantId)
    {
        return await _dbContext.Stores
            .Where(store => store.MerchantId == merchantId)
            .ToListAsync();
    }
}
