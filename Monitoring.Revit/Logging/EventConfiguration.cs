namespace Monitoring.Revit.Logging
{
    public class EventConfiguration
    {
        public bool Initialized { get; set; }
        public bool Opening { get; set; }
        public bool Saving { get; set; }
        public bool Synchronizing { get; set; }
        public bool Printing { get; set; }
        public bool Exporting { get; set; }
        public bool Importing { get; set; }
        public bool FamilyLoading { get; set; }
        public bool ViewChanged { get; set; }
        public bool Changes { get; set; }
        public bool UiClicks { get; set; }
        public bool TimeSpent { get; set; }
    }
}