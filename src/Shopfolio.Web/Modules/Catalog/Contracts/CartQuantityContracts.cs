namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>A variant of a cart and the quantity it holds (or would hold). Carries no stock number (10 KD-33).</summary>
public sealed record CartQuantityLine(Guid VariantId, int Quantity);

/// <summary>
/// The verdict on the quantity of one cart line (T347; 02 3.16.8-3.16.10). <see cref="OrderLimit"/> is the product's limit and is filled exactly when <see cref="Problem"/> is
/// <see cref="CartQuantityProblem.OverOrderLimit"/>: the limit is told as a number, the stock never is.
/// </summary>
public sealed record CartQuantityVerdict(Guid VariantId, CartQuantityProblem Problem, int? OrderLimit);
