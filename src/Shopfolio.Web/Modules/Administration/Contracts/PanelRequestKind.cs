namespace Shopfolio.Modules.Administration.Contracts;

// The two kinds of request the panel's requests list holds (T532; 07 5.3.2 RequestQuery requestType; 02 1.2): the names are the parameter's closed list.
public enum PanelRequestKind
{
    ContactRequest,
    DefectClaim,
}
