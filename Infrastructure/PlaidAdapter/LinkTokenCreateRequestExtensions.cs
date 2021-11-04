using Going.Plaid.Entity;
using Going.Plaid.Link;

namespace PlaidAdapter;

public static class LinkTokenCreateRequestExtensions
{
  public static LinkTokenCreateRequest WithAccessToken(this LinkTokenCreateRequest ltcr, string accessToken)
  {
    ltcr.AccessToken = accessToken;
    return ltcr;
  }

  public static LinkTokenCreateRequest WithoutProducts(this LinkTokenCreateRequest ltcr)
  {
    ltcr.Products = new List<Products>();
    return ltcr;
  }
}
