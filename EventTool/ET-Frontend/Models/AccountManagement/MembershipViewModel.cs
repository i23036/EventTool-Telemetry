namespace ET_Frontend.Models.AccountManagement;

public class MembershipViewModel
{
    public int    AccountId        { get; set; }
    public string OrganisationName { get; set; } = "";
    public string Email            { get; set; } = "";
    public int    OrganisationId   { get; set; }
}