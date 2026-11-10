
using System.Text.Json.Serialization;

namespace RaceDayAPI.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int MinimumAge { get; set; }

        public int MaximumAge { get; set; }

        public decimal EntryFee { get; set; }

        [JsonIgnore]
        public Event? Event { get; set; }

        [JsonIgnore]
        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}