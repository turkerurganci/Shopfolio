namespace Shopfolio.Infrastructure.DataModel;

/// <summary>
/// Marks a record whose writing never raises the order's version (06 2.6.4, 4.17 "Sürüm"): the internal note, the dispatch record and its
/// outcome, the transition record, the audit trail, the job row and the inbound notification record. The order's version guard reads the mark
/// from the model instead of naming another module's type (09 6 item 2, 10.1 item 1); the closed list is the set of the types that carry it and
/// a test holds it equal to 06 2.6.4. It has no members.
/// </summary>
internal interface IOrderVersionExempt;
