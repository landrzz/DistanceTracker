

using Prism.Events;

namespace DistanceTracker
{
    public class TimedLapsPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public TimedLapRecord SelectedTimedLap { get; set; }
        public List<TimedLapRecord> TimedLapRecordsList { get; set; } = new List<TimedLapRecord>();
        [Reactive] public ObservableCollection<TimedLapRecord> TimedLapRecords { get; set; } = new ObservableCollection<TimedLapRecord>();
        [Reactive] public string TotalNumberOfTimedLaps { get; set; }
        public DelegateCommand<TimedLapRecord> TimedLapRecordStartCommand { get; }
        public DelegateCommand<TimedLapRecord> TimedLapRecordStopCommand { get; }

        public string EventName { get; set; }
        public string EventId { get; set; }

        public DelegateCommand<string> NavigateCommand { get; }
        public ICommand RefreshCommand => new Command(RefreshTimedLapsOnCommand);

        public TimedLapsPageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
            TimedLapRecordStartCommand = new DelegateCommand<TimedLapRecord>(TimedLapRecordStart);
            TimedLapRecordStopCommand = new DelegateCommand<TimedLapRecord>(TimedLapRecordStop);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            EventName = Preferences.Default.Get(Keys.CurrentEventName, string.Empty);

            //code go here
            if (parameters.GetNavigationMode() == Prism.Navigation.NavigationMode.Back)
            {
                if (parameters.ContainsKey(Keys.NeedsRefresh))
                {
                    var shouldRefresh = parameters.GetValue<bool>(Keys.NeedsRefresh);

                    if (shouldRefresh)
                    {
                        IsRefreshing = true;
                        await Task.Delay(1500);
                        await GetCurrentTimedLaps(EventName, forceRefresh: true);
                        IsRefreshing = false;
                    }
                }               
            }
            else
            {
                IsRefreshing = true;
                await Task.Delay(1000);
                await GetCurrentTimedLaps(EventName, forceRefresh: true);
                IsRefreshing = false;
            }

            base.OnNavigatedTo(parameters);
        }

        public async Task RefreshTimedLaps()
        {
            await GetCurrentTimedLaps(EventName, forceRefresh: true);
        }

        public async void RefreshTimedLapsOnCommand()
        {
            await GetCurrentTimedLaps(EventName, forceRefresh: true);
        }

        public async Task<List<TimedLapRecord>> GetCurrentTimedLaps(string curRaceEvent, bool forceRefresh = true)
        {
            var timedlaprecordList = new List<TimedLapRecord>();

            try
            {
                var laprecordListResult = await DataService.GetTimedLapRecords(forceRefresh, curRaceEvent);
                timedlaprecordList = laprecordListResult.ToList();
                if (timedlaprecordList != null)
                {
                    TimedLapRecordsList = timedlaprecordList;
                    TimedLapRecords = new ObservableCollection<TimedLapRecord>(TimedLapRecordsList.OrderByDescending(x => x.CreatedTime));
                    
                    TotalNumberOfTimedLaps = $"Laps: {TimedLapRecordsList.Count}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetLapRecords - Error getting lap records");
            }

            return timedlaprecordList;
        }

        public async void TimedLapRecordStart(TimedLapRecord lap)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                //start the timer
                var result = await DataService.PutLapStart(DateTime.Now.ToString(), lap.Id);
                if (result != null)
                {
                    foreach (var lapp in TimedLapRecords.Where(x => x.BibNumber == result.BibNumber))
                    {
                        lapp.LapStartedTime = result.LapStartedTime;
                        await RefreshTimedLaps();
                    }
                }
                else
                    await _dialogService.Alert("Could not start lap! Please try again.");

                SelectedTimedLap = null;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await _dialogService.Alert("Could not start lap! Please try again.");
            }
            IsBusy = false;
        }

        public async void TimedLapRecordStop(TimedLapRecord lap)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                //stop the timer
                var result = await DataService.PutLapStop(DateTime.Now.ToString(), lap.Id);
                if (result != null)
                {
                    foreach (var lapp in TimedLapRecords.Where(x => x.BibNumber == result.BibNumber))
                    {
                        lapp.LapCompletedTime = result.LapCompletedTime;
                        await RefreshTimedLaps();
                    }
                }
                else
                    await _dialogService.Alert("Could not stop lap! Make note of the time (write it down) and try again.");

                SelectedTimedLap = null;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await _dialogService.Alert("Could not stop lap! Start time must be set! Please try again.");
            }
            IsBusy = false;
        }

        public override Task InitializeAsync(INavigationParameters parameters)
        {
            //code go here

            return base.InitializeAsync(parameters);
        }

        [Reactive] public string Property { get; set; }
    }
}