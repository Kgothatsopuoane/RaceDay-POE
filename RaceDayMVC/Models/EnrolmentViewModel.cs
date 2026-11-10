
namespace RaceDayMVC.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; }

        public ParticipantViewModel? Participant { get; set; }
        public EnrolledEventViewModel? Event { get; set; }
        public EnrolledCategoryViewModel? Category { get; set; }
    }

    public class ParticipantViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class EnrolledEventViewModel
    {
        public string EventName { get; set; } = string.Empty;
        public int OrganiserId { get; set; }
    }

    public class EnrolledCategoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
    }
}
