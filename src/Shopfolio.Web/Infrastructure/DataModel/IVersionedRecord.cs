namespace Shopfolio.Infrastructure.DataModel;

/// <summary>
/// A record with one version column that its edit form carries (06 2.6.1, 2.6.4; 07 2.4; 09 10.2 items 1-3): the version is a plain integer, read
/// by anyone and raised only by <see cref="BumpVersion"/>. The form helpers of <c>Infrastructure/Forms</c> compare and raise it through this
/// interface so they name no module's type (09 6 item 2). A record with several version columns (the company settings, 06 2.6.3, 09 10.3) does
/// not carry it: each of its sections has its own column and its own conditional update.
/// </summary>
internal interface IVersionedRecord
{
    /// <summary>The version the record has now; the form carries the value it had when the form was opened.</summary>
    int Version { get; }

    /// <summary>Raises the version by one; every write of the record calls it (09 10.2 item 1).</summary>
    void BumpVersion();
}
