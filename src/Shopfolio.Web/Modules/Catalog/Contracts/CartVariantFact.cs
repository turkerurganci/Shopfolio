namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// What a cart item reads of its variant, live (02 3.16.5, 3.16.6, 3.16.7): the product's name, the price that applies now - the discounted price while the product's discount runs -,
/// whether the variant and its product stand on the showcase (both Published) and whether the variant is sold out (stock minus the reserved units is not above zero; a variant
/// without a tracked count never is). Carries no stock number (10 KD-33).
/// </summary>
/// <param name="VariantId">The variant.</param>
/// <param name="ProductId">The variant's product.</param>
/// <param name="ProductName">The product's name, as the cart line and the once-told messages show it.</param>
/// <param name="ProductType">The product's type.</param>
/// <param name="IsOnShowcase">True when both the product and the variant are Published.</param>
/// <param name="IsSoldOut">True when the variant cannot be bought now.</param>
/// <param name="UnitPrice">The unit price that applies now, VAT included.</param>
public sealed record CartVariantFact(Guid VariantId, Guid ProductId, string ProductName, ProductType ProductType, bool IsOnShowcase, bool IsSoldOut, decimal UnitPrice);
