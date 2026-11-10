
namespace RaceDayMVC.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string? EventBannerUrl { get; set; }

        public int OrganiserId { get; set; }
    }
}
