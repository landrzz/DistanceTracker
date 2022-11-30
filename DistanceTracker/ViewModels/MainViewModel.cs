

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

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            var curEvent = Preferences.Default.Get(Keys.CurrentEventName, "NOT SET");
            CurrentEventName = $"Event Name: {curEvent}";

            var curEventId = Preferences.Default.Get(Keys.CurrentEventId, string.Empty);
            if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
            {
                //check for updated details
                await RefreshCurrentEventDetails(curEventId);
            }

            var curDistance = Preferences.Default.Get(Keys.Distances, "NOT SET");
            CurrentDistances = $"Distances: {curDistance}";

            StartTime = Preferences.Default.Get(Keys.CurrentEventTimestamp, "NOT STARTED YET");
            EventStartTime = $"Started At: {StartTime}";

            

            base.OnNavigatedTo(parameters);
        }

        public override Task InitializeAsync(INavigationParameters parameters)
        {
            var curEvent = Preferences.Default.Get(Keys.CurrentEventName, "NOT SET");
            CurrentEventName = $"Event Name: {curEvent}";

            var curDistance = Preferences.Default.Get(Keys.Distances, "NOT SET");
            CurrentDistances = $"Distances: {curDistance}";

            StartTime = Preferences.Default.Get(Keys.CurrentEventTimestamp, "NOT STARTED YET");
            EventStartTime = $"Started At: {StartTime}";

            return base.InitializeAsync(parameters);
        }

        public async Task RefreshCurrentEventDetails(string curId, bool forceRefresh = true)
        {
            var raceEvent = new RaceEvent();

            try
            {
                var rEvent = await DataService.GetEvent(forceRefresh, curId);
                raceEvent = rEvent;
                if (raceEvent != null && !string.IsNullOrWhiteSpace(raceEvent.EventName))
                {
                    Preferences.Default.Set(Keys.CurrentEventTimestamp, raceEvent.EventStartTimestamp);

                    await Task.Delay(2000);

                    StartTime = Preferences.Default.Get(Keys.CurrentEventTimestamp, "NOT STARTED YET");
                    EventStartTime = $"Started At: {StartTime}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "RefreshCurrentEventDetails - Error getting race event details");
            }
        }


        [Reactive] public string Property { get; set; }
    }
}