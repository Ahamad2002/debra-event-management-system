using debra_man.Controllers;

namespace debra_man
{
    public class Event
    {

        public int EventID { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public string ImagePath { get; set; }
        public int UserID { get; set; }
    }
}
