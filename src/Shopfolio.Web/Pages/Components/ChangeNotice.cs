namespace Shopfolio.Pages.Components;

/// <summary>
/// The notice of a form whose value change sends the form by itself (T552, WCAG 2.2 3.2.2 On Input; <c>VAL-A784</c>): the cart's quantity, the payment step, the variant choice of a product and the sales summary's period. The sentence is
/// written here once; the <c>_ChangeNotice</c> partial draws it inside each such form and <c>common.js</c> shows it when the script that sends the change runs (without a script the form's own button sends it and nothing is announced).
/// The architecture test of the accessibility record reads every form that carries the script's marker and measures that the notice stands in it.
/// </summary>
public static class ChangeNotice
{
    /// <summary>The one sentence told before the change is made; it names no field, so it fits every such form.</summary>
    public const string Text = "Bu formdaki bir değeri değiştirmek sayfayı kendiliğinden günceller.";

    /// <summary>The marker of the notice's element.</summary>
    public const string Marker = "change-notice";
}
