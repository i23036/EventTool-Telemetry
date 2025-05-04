namespace ET_Backend.Models;

public class Organization
{
    public String Name { set; get; }

    public String Description { set; get; }

    public String Domain { set; get; }

    public List<Account> Members { set; get; } = new List<Account>();
}