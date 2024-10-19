using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    [EmailAddress]
    public required string Email { get; set; }
}
