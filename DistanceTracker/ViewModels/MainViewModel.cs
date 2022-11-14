

namespace DistanceTracker
{
    public class MainViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        [Reactive] public string CurrentEventName { get; set; } = "Current Event Name: NOT SET";
        [Reactive] public string CurrentDistances { get; set; } = "Current Distances: NOT SET";

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
            var curEvent = Preferences.Get("currenteventname", "NOT SET");
            CurrentEventName = $"Current Event Name: {curEvent}";

            var curDistance = Preferences.Get("distances", "NOT SET");
            CurrentDistances = $"Current Distances: {curDistance}";


            base.OnNavigatedTo(parameters);
        }

        public override Task InitializeAsync(INavigationParameters parameters)
        {
            var curEvent = Preferences.Get("currenteventname", "NOT SET");
            CurrentEventName = $"Current Event Name: {curEvent}";

            var curDistance = Preferences.Get("distances", "NOT SET");
            CurrentDistances = $"Current Distances: {curDistance}";

            return base.InitializeAsync(parameters);
        }


        [Reactive] public string Property { get; set; }
    }
}