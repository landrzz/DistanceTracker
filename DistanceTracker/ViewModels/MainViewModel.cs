

namespace DistanceTracker
{
    public class MainViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        [Reactive] public string CurrentEventName { get; set; } = "Current Event Name: NOT SET";
        [Reactive] public string CurrentDistances { get; set; } = "Current Distances: NOT SET";
        [Reactive] public string EventStartTime { get; set; } = "Event Started: NOT STARTED YET";
        public string StartTime { get; set; }

        public MainViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            var curEvent = Preferences.Get(Keys.CurrentEventName, "NOT SET");
            CurrentEventName = $"Current Event Name: {curEvent}";

            var curDistance = Preferences.Get(Keys.Distances, "NOT SET");
            CurrentDistances = $"Current Distances: {curDistance}";

            StartTime = Preferences.Get(Keys.CurrentEventTimestamp, "NOT STARTED YET");
            EventStartTime = $"Event Started: {StartTime}";

            base.OnNavigatedTo(parameters);
        }

        public override Task InitializeAsync(INavigationParameters parameters)
        {
            var curEvent = Preferences.Get(Keys.CurrentEventName, "NOT SET");
            CurrentEventName = $"Current Event Name: {curEvent}";

            var curDistance = Preferences.Get(Keys.Distances, "NOT SET");
            CurrentDistances = $"Current Distances: {curDistance}";

            StartTime = Preferences.Get(Keys.CurrentEventTimestamp, "NOT STARTED YET");
            EventStartTime = $"Event Started: {StartTime}";

            return base.InitializeAsync(parameters);
        }


        [Reactive] public string Property { get; set; }
    }
}