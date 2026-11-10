
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "Participant";
    }
}
