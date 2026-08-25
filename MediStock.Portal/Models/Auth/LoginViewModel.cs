namespace MediStock.Portal.Models.Auth
{
    public sealed class LoginViewModel
    {
        public string pharmacySlug { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string? errormessage { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string newpassword { get; set; } = string.Empty;
        public string confirmpassword { get; set; } = string.Empty;
        public string? errormessage { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string currentpassword { get; set; } = string.Empty;
        public string newpassword { get; set; } = string.Empty;
        public string confirmpassword { get; set; } = string.Empty;
        public string? errormessage { get; set; }
    }
}
