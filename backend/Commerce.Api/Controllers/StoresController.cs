using Commerce.Api.Extensions;
using Commerce.Infrastructure.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Controllers;

[ApiController]
[Route("api/stores")]
[Authorize(Roles = "Merchant")]
public class StoresController : ControllerBase
{
    private readonly StoreService _storeService;

    public StoresController(StoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStores()
    {
        var merchantId = User.GetMerchantId();

        var stores = await _storeService.GetAllAsync(merchantId);

        return Ok(stores);
    }

    [HttpGet("{storeId:guid}")]
    public async Task<IActionResult> GetStore(Guid storeId)
    {
        var merchantId = User.GetMerchantId();

        var store = await _storeService.GetByIdAsync(
            storeId,
            merchantId);

        if (store == null)
        {
            return NotFound();
        }

        return Ok(store);
    }
}

/*
API endpoints that allow a logged-in merchant to retrieve their stores,
while making sure they can only access stores that belong to them
 */
