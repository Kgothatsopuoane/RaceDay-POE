
namespace RaceDayAPI.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public int? FinishTimeId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? FinishingPosition { get; set; }

        public Enrolment? Enrolment { get; set; }
    }
}
