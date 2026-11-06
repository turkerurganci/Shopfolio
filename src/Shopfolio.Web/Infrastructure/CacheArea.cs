namespace Shopfolio.Infrastructure;

/// <summary>
/// The areas of the in-memory cache (09 10.4 item 1; 05 7.10.1; MK-34): the storefront's often read parts and the sitemap. Each area holds one
/// value of one fixed type; there is no parameterized key. A new area enters 05 7.10 first and this enum after it. Price, stock, cart and the
/// state of the sales gate are not an area: they are read from the database and computed on every request (05 4.1.5; K-980).
/// </summary>
internal enum CacheArea
{
    Menu,
    CategoryTree,
    Settings,
    HomepageBlocks,
    Sitemap,
}
