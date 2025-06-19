namespace ET_Frontend.Models;

public class OrganizationMemberViewModel
{
    public string Email { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public int Role { get; set; } // kann auch Enum sein (siehe unten)

    public OrganizationMemberViewModel() { }

    public OrganizationMemberViewModel(string email,string firstName, string lastName, int role)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }
}