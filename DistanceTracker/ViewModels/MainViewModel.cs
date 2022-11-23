

namespace DistanceTracker
{
    public class MainViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        [Reactive] public string CurrentEventName { get; set; } = "Event Name: NOT SET";
        [Reactive] public string CurrentDistances { get; set; } = "Distances: NOT SET";
        [Reactive] public string EventStartTime { get; set; } = "Started At: NOT STARTED YET";
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
            CurrentEventName = $"Event Name: {curEvent}";

            var curDistance = Preferences.Get(Keys.Distances, "NOT SET");
            CurrentDistances = $"Distances: {curDistance}";

            StartTime = Preferences.Get(Keys.CurrentEventTimestamp, "NOT STARTED YET");
            EventStartTime = $"Started At: {StartTime}";

            base.OnNavigatedTo(parameters);
        }

        public override Task InitializeAsync(INavigationParameters parameters)
        {
            var curEvent = Preferences.Get(Keys.CurrentEventName, "NOT SET");
            CurrentEventName = $"Event Name: {curEvent}";

            var curDistance = Preferences.Get(Keys.Distances, "NOT SET");
            CurrentDistances = $"Distances: {curDistance}";

            StartTime = Preferences.Get(Keys.CurrentEventTimestamp, "NOT STARTED YET");
            EventStartTime = $"Started At: {StartTime}";

            return base.InitializeAsync(parameters);
        }


        [Reactive] public string Property { get; set; }
    }
}