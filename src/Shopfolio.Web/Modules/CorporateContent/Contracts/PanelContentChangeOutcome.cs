namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>What became of a publish status move (T534; 07 UC-159).</summary>
public enum PanelContentChangeOutcome
{
    /// <summary>The status was written, the trail row was written and the version rose.</summary>
    Changed,

    /// <summary>The record was already in the asked status: nothing was written (07 2.5.1).</summary>
    Unchanged,

    /// <summary>The gate is not met: 422, the fields are <see cref="PanelContentChangeResult.Missing"/> (K-584).</summary>
    GateIncomplete,

    NotFound,
}
