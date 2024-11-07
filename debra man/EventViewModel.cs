using System.ComponentModel.DataAnnotations;

namespace debra_man
{
    public class EventViewModel
    {

        public string EventName { get; set; }

        public string EventDescription { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string Venue { get; set; }

        public string ImagePath { get; set; }

        [Required]
        public IFormFile Image { get; set; }
    }
}
