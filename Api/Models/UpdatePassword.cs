
namespace Api.Models;
public class UpdatePassword {
    public string UserName { get; set; } = String.Empty;
    public string OldPassword { get; set; } = String.Empty;
    public string NewPassword { get; set; } = String.Empty;
}