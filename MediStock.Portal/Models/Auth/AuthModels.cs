// ============================================================
//  MediStock.Portal — Auth Models
//  Place in: Models/Auth/AuthModels.cs
// ============================================================

using System.Text.Json.Serialization;

namespace MediStock.Portal.Models.Auth
{
    // ── Login ─────────────────────────────────────────────────────────────────
    public sealed class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        [JsonPropertyName("userid")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("mobile")]
        public string Mobile { get; set; } = string.Empty;

        [JsonPropertyName("role_type")]
        public string RoleType { get; set; } = string.Empty;

        [JsonPropertyName("profile_id")]
        public string ProfileId { get; set; } = string.Empty;

        [JsonPropertyName("pharmacy_id")]
        public string PharmacyId { get; set; } = string.Empty;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("otp")]
        public string? Otp { get; set; }
    }

    // ── OTP ───────────────────────────────────────────────────────────────────
    public sealed class OtpRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("otp")]
        public string Otp { get; set; } = string.Empty;
    }

    public sealed class OtpResponse
    {
        [JsonPropertyName("userid")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("mobile")]
        public string Mobile { get; set; } = string.Empty;

        [JsonPropertyName("role_type")]
        public string RoleType { get; set; } = string.Empty;

        [JsonPropertyName("profile_id")]
        public string ProfileId { get; set; } = string.Empty;

        [JsonPropertyName("pharmacy_id")]
        public string PharmacyId { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("change_password")]
        public bool ChangePassword { get; set; }
    }

    // ── View Models ───────────────────────────────────────────────────────────
    public sealed class OtpViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? DevOtpHint { get; set; }
    }
}
