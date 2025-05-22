namespace ET_Frontend.Models;

public class OrganizationMemberViewModel
{
    public string Email { get; set; }
    public string LastName { get; set; }
    public int Role { get; set; } // kann auch Enum sein (siehe unten)

    public OrganizationMemberViewModel() { }

    public OrganizationMemberViewModel(string email, string lastName, int role)
    {
        Email = email;
        LastName = lastName;
        Role = role;
    }
}