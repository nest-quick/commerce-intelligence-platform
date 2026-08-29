using System.Security.Claims;

namespace Commerce.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetMerchantId(this ClaimsPrincipal user)
    {
        var merchantIdClaim = user.FindFirst("merchantId");

        if (merchantIdClaim == null)
        {
            throw new UnauthorizedAccessException(
                "Merchant ID claim is missing.");
        }

        if (!Guid.TryParse(merchantIdClaim.Value, out var merchantId))
        {
            throw new UnauthorizedAccessException(
                "Merchant ID claim is invalid.");
        }

        return merchantId;
    }
}

/*
This gets the logged-in user’s merchant ID from their token
and returns it. If it’s missing or invalid, it rejects the request.
 */
