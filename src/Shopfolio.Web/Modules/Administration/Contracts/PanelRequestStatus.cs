namespace Shopfolio.Modules.Administration.Contracts;

// The statuses of the requests list's filter (T532; 07 5.3.2 requestStatus): Open is the open contact request and the open defect claim, Closed only a contact request and Resolved only a defect claim
// (04 9.10.3). The names are the parameter's closed list.
public enum PanelRequestStatus
{
    Open,
    Closed,
    Resolved,
}
