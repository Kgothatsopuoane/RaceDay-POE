
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateResultViewModel
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        [Range(1, 10000)]
        public int FinishingPosition { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        public List<ResultEnrolmentOptionViewModel> Enrolments { get; set; } = new();
    }

    public class ResultEnrolmentOptionViewModel
    {
        public int EnrolmentId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string ParticipantName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
    }
}
