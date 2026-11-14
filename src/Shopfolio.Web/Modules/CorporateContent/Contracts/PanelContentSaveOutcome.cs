namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>What became of the content form's save (T534; 07 UC-158).</summary>
public enum PanelContentSaveOutcome
{
    Saved,

    /// <summary>The record is not there (or is another type's): 404.</summary>
    NotFound,

    /// <summary>The record changed since the form was opened: 409 (K-800).</summary>
    VersionConflict,

    /// <summary>A second about page or announcement: 409; the form binds to <see cref="PanelContentSaveResult.ExistingId"/> (K-1193 (2)).</summary>
    AlreadyExists,

    /// <summary>A value is wrong: 422, the fields are <see cref="PanelContentSaveResult.Rejections"/>.</summary>
    Rejected,

    /// <summary>The edit of a published record would empty a gate field: 422, <see cref="PanelContentSaveResult.Missing"/> names them (K-812).</summary>
    PublishedGateBroken,
}
