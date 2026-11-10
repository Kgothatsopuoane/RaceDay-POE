
namespace RaceDayMVC.Models
{
    public class ResultViewModel
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public int? FinishTimeId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? FinishingPosition { get; set; }

        public ResultEnrolmentViewModel? Enrolment { get; set; }
    }

    public class ResultEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int EventId { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public ResultEventViewModel? Event { get; set; }

        public ResultParticipantViewModel? Participant { get; set; }

        public ResultCategoryViewModel? Category { get; set; }
    }

    public class ResultEventViewModel
    {
        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
    }

    public class ResultParticipantViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }

    public class ResultCategoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
    }
}
