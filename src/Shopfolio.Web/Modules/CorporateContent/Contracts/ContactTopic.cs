namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>The closed values of ContactTopic (06 5.3.10). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum ContactTopic
{
    GeneralQuestion,
    AboutOrder,
    AboutProduct,
    PersonalDataRequest,
    Other,
}
