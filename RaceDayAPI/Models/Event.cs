
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        [Column("Decription")]
        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        [Column("RouteDistance")]
        public decimal DistanceKm { get; set; }

        public string EventType { get; set; } = string.Empty;

        // Azure event banner image URL
        public string? EventBannerUrl { get; set; }

        public int OrganiserId { get; set; }

        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}
