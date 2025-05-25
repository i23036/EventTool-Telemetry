using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace ET_Frontend.Models;

public class OrgaChangeModel
{
    public int OrganizationId { get; set; }

    [Required(ErrorMessage = "Organisationsname ist erforderlich.")]
    public string orgaName { get; set; }

    public string description { get; set; }

    [Required(ErrorMessage = "Domain ist erforderlich.")]
    public string domain { get; set; }

    public IBrowserFile orgaPic { get; set; }

    public string orgaPicBase64 { get; set; }
}