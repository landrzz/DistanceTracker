

namespace DistanceTracker
{
    public class NewLapPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public bool ShowLoading { get; set; }

        [Reactive] public ObservableCollection<Runner> RunnersList { get; set; }
        public List<string> DistancesMixed { get; set; } = new List<string>();
        public List<double> DistancesMiles { get; set; } = new List<double>();
        public string EventName { get; set; }

        public DelegateCommand<Runner> SelectionChangedCommand { get; }

        [Reactive] public Runner SelectedRunner { get; set; }
        [Reactive] public string ItemCount { get; set; }
        public string EventTimeStamp { get; set; }
        public string EventId { get; set; }

        public NewLapPageViewModel(Shiny.BaseServices services) : base(services)
        {
            SelectionChangedCommand = new DelegateCommand<Runner>(RunnerSelected);

            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            var currentDistances = Preferences.Default.Get(Keys.Distances, string.Empty);
            var currentEventId = Preferences.Default.Get(Keys.CurrentEventId, string.Empty);

            if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
            {
                CheckIsEventIdSet();
                var distancesSet = CheckAreDistancesSet();
                var raceEvent = CheckIsEventSet();
                if (!raceEvent || !distancesSet)
                {
                    await _dialogService.Alert("You must specify both a default event and default event distances before recording distance!", "Set Event First!");
                    await _navigationService.GoBackAsync();
                }
                CheckIsEventTimeStampSet();

                if (!string.IsNullOrWhiteSpace(currentDistances))
                {
                    //only mileage supported for right now
                    DistancesMixed = currentDistances.Split(',').ToList();
                    foreach (var d in DistancesMixed)
                    {
                        var mileDbl = double.Parse(d);
                        DistancesMiles.Add(mileDbl);
                    }
                }
            }

            IsRefreshing = true;
            await GetEventDetails(currentEventId, forceRefresh: true);
            await GetRunners(EventName, forceRefresh: true);
            IsRefreshing = false;

            base.OnNavigatedTo(parameters);
        }

        public override Task InitializeAsync(INavigationParameters parameters)
        {
            //code go here

            return base.InitializeAsync(parameters);
        }

        public async Task<List<Runner>> GetEventDetails(string currentEventId, bool forceRefresh = true)
        {
            var runnersList = new List<Runner>();

            try
            {
                var eventDetails = await DataService.GetEvent(forceRefresh, currentEventId);
                if (eventDetails != null)
                {
                    var currentTimestamp = eventDetails.EventStartTimestamp ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(currentTimestamp))
                    {
                        await _dialogService.Alert("Event time clock has not been started yet. You may still add distances but no time deltas will be recorded.", "Time Clock Not Yet Started", "OK");
                    }
                    else
                    {
                        Preferences.Default.Set(Keys.CurrentEventTimestamp, currentTimestamp);
                        CheckIsEventTimeStampSet();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetRunners - Error getting runners");
            }

            return runnersList;
        }

        public async Task<List<Runner>> GetRunners(string curRaceEvent, bool forceRefresh = true)
        {
            var runnersList = new List<Runner>();

            try
            {
                var runnersListResult = await DataService.GetRunners(forceRefresh, curRaceEvent);
                runnersList = runnersListResult.ToList();
                if (runnersList != null)
                {

                    RunnersList = new ObservableCollection<Runner>(runnersList.OrderBy(x => double.Parse(x.BibNumber)));
                    ItemCount = RunnersList.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetRunners - Error getting runners");
            }

            return runnersList;
        }

        public async void RunnerSelected(Runner runn)
        {
            //SelectedRunner = null;
            var runner = runn as Runner;

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                //set to one of the distances
                //await _dialogService.Alert($"{runner.RunnerName}");

                var distances = DistancesMixed.ToArray();

                var result = await _dialogService.ActionSheet($"{runner.RunnerName}'s Lap Distance\nWhich distance are they planning to run?",
                    null, "Cancel", distances);

                if (result != null && result != "Cancel")
                {
                    //a distance was selected
                    var distance = result;

                    //TODO: create new timed lap record
                    //blank with no start time or end time

                    //create new lap record
                    var lapRecord = new TimedLapRecord()
                    {
                        LapCompletedTime = string.Empty,
                        LapStartedTime = string.Empty,
                        BibNumber = runner.BibNumber,
                        RunnerName = runner.RunnerName,
                        RunnerId = runner.Id,
                        LapDistance = distance,
                        RaceEventName = EventName,                        
                    };

                    //save
                    SyncSaveLapRecord(lapRecord);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "RunnerSelected - Error selecting a runner from list view or saving lap record");
            }
            IsBusy = false;
        }

        public async void SyncSaveLapRecord(TimedLapRecord lap)
        {
            try
            {
                if (ConnectivityService.IsConnected())
                {
                    ShowLoading = true;
                    var _lap = await DataService.PostTimedLapRecordAsync(lap);
                    if (_lap == null)
                    {
                        await _dialogService.Alert("Create Timed Lap Record Failed",
                            "An error occured while creating the timed lap record. Please try again.",
                            "OK");
                    }
                    else
                    {
                        await _dialogService.Snackbar("New Timed Lap Created!");

                        var navP = new NavigationParameters
                        {
                            { Keys.NeedsRefresh, true }
                        };

                        await _navigationService.GoBackAsync(navP);
                    }
                }
                else
                {
                    ShowLoading = false;
                    await _dialogService.Alert("No Internet Connection", "Please ensure you have an active network connection and try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                ShowLoading = false;
            }
            ShowLoading = false;
        }

        public bool CheckIsEventSet()
        {
            var raceEvent = Preferences.Default.Get(Keys.CurrentEventName, string.Empty);
            if (string.IsNullOrWhiteSpace(raceEvent))
            {
                return false;
            }
            else
            {
                EventName = raceEvent;
                return true;
            }
        }

        public string CurrentEventTimestamp { get; set; }
        public bool CheckIsEventTimeStampSet()
        {
            EventTimeStamp = Preferences.Default.Get(Keys.CurrentEventTimestamp, string.Empty);
            if (string.IsNullOrWhiteSpace(EventTimeStamp))
            {
                return false;
            }
            else
            {
                CurrentEventTimestamp = EventTimeStamp;
                return true;
            }
        }

        public bool CheckAreDistancesSet()
        {
            var distances = Preferences.Default.Get(Keys.Distances, string.Empty);
            if (string.IsNullOrWhiteSpace(distances))
            {
                return false;
            }
            else
                return true;
        }

        public bool CheckIsEventIdSet()
        {
            var raceEventId = Preferences.Default.Get(Keys.CurrentEventId, string.Empty);
            if (string.IsNullOrWhiteSpace(raceEventId))
            {
                return false;
            }
            else
            {
                EventId = raceEventId;
                return true;
            }
        }


        [Reactive] public string Property { get; set; }
    }
}