using System.ComponentModel.DataAnnotations;

namespace MediStock.Portal.Models.Auth
{
    public class RegisterViewModel
    {
        public string? errormessage { get; set; }

        [Required] public string PharmacyName { get; set; } = "";
        public string? PharmacyEmail { get; set; }
        public string? PharmacyPhone { get; set; }
        public string? PharmacyAddress { get; set; }

        [Required] public string FirstName { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        [Required, EmailAddress] public string AdminEmail { get; set; } = "";
        public string? AdminPhone { get; set; }

        [Required, MinLength(6)] public string Password { get; set; } = "";
        [Required] public string ConfirmPassword { get; set; } = "";
    }
}
