namespace DistanceTracker.ViewModels
{
    public class EventsListPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }
        public DelegateCommand<RaceEvent> RaceEventSelectedCommand { get; }

        public RaceEvent SelectedEvent { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public ObservableCollection<RaceEvent> EventList { get; set; }

        //public ICommand FinalizeSetDistancesCommand => new Command(FinalizeSetDistances);

        public EventsListPageViewModel(BaseServices services) : base(services)
        {
            RaceEventSelectedCommand = new DelegateCommand<RaceEvent>(RaceEventSelected);

            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
            {
                //no need to force a refresh of the class list
                IsRefreshing = true;
                await GetEvents(forceRefresh: true);
                IsRefreshing = false;
            }

            base.OnNavigatedTo(parameters);
        }

        public async Task<List<RaceEvent>> GetEvents(bool forceRefresh = true)
        {
            var eventList = new List<RaceEvent>();

            try
            {
                var eventsListResult = await DataService.GetRaces(forceRefresh);
                eventList = eventsListResult.ToList();
                if (eventList != null)
                {
                    EventList = new ObservableCollection<RaceEvent>(eventList);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetEvents - Error getting events");
            }

            return eventList;
        }

        private async void RaceEventSelected(RaceEvent raceevent)
        {
            SelectedEvent = null;

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var res = await _dialogService.Confirm($"Would you like to set {raceevent.EventName} as your default event?", "Set as Default?", "Yes", "No");

                if (res)
                {
                    var eventPass = await _dialogService.Input("Please enter the event passcode", "Event Passcode", "OK", "Cancel");
                    if (eventPass == raceevent.EventPassCode)
                    {
                        Preferences.Set("currenteventname", raceevent.EventName);
                        Preferences.Set("currenteventcode", raceevent.EventPassCode);
                        await _dialogService.Snackbar("Default Event Set!");
                    }
                    else
                    {
                        await _dialogService.Snackbar("Passcode incorrect! Please try again.");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "RaceEventSelected - Error selecting a race event from list view");
            }
            IsBusy = false;
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }


    }
}
