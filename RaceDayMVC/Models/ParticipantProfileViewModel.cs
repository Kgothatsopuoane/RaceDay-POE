
namespace RaceDayMVC.Models
{
    public class ParticipantProfileViewModel
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Role { get; set; } = "";

        public string? ProfilePictureUrl { get; set; }
    }
}
