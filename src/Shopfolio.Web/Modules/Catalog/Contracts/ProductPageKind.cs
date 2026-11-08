namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// What an address under <c>/urun/{slug}</c> answers (02 3.7.5, 3.7.6, 3.30.4). Only the code names live here; the behavior of a value lives in <see cref="ProductPage"/>.
/// NotFound: no page - the address is unknown, deleted or a draft seen by a visitor, one and the same "page not found". Published: the product page with its Published variants.
/// DraftPreview: a Draft product seen by a signed-in administrator, with the "Draft" band and every variant but the archived. Archived: the "Bu ürün artık satılmıyor" page - name,
/// image and status, no price and no cart.
/// </summary>
public enum ProductPageKind
{
    NotFound,
    Published,
    DraftPreview,
    Archived,
}
