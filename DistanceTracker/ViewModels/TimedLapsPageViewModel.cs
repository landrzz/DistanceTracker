

using System.Text;
using Prism.Events;
using Shiny;

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
        public string EventTimeStamp { get; set; }

        public DelegateCommand<string> NavigateCommand { get; }
        public ICommand RefreshCommand => new Command(RefreshTimedLapsOnCommand);

        public TimedLapsPageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
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
                    TimedLapRecords = new ObservableCollection<TimedLapRecord>(TimedLapRecordsList.Where(y => string.IsNullOrWhiteSpace(y.LapCompletedTime)).OrderByDescending(x => x.CreatedTime));
                    
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
                    }
                    await RefreshTimedLaps();
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
                    var newlapRecCount = 0;
                    foreach (var lapp in TimedLapRecords.Where(x => x.BibNumber == result.BibNumber).OrderByDescending(y => y.CreatedTime))
                    {
                        lapp.LapCompletedTime = result.LapCompletedTime;
                        await RefreshTimedLaps();

                        if (newlapRecCount == 0)
                        {
                            //don't even ask to add because this is for an older (lingering) timed lap record

                            var res = await _dialogService.Confirm("Would you like to add this to the distance total? (Probably yes)", "Add Distance to Laps", "YES", "NO");
                            if (res)
                            {
                                //add to laps
                                var elapsedTimeTicks = GetElapsedTicks();
                                var lapRec = new LapRecord()
                                {
                                    LapCompletedTime = DateTime.Now,
                                    BibNumber = lapp.BibNumber,
                                    RunnerName = lapp.RunnerName,
                                    RunnerId = lapp.RunnerId,
                                    LapDistance = lapp.LapDistance,
                                    RaceEventName = EventName,
                                    LapTimeSpan = elapsedTimeTicks,
                                };

                                await SyncSaveRegularLapRecord(lapRec);
                                newlapRecCount++;
                            }
                        }
                        
                    }
                }
                else
                    await _dialogService.Alert("Could not stop lap! Make note of the time (write it down) and try again. Lap may have already been stopped", "Error. Could not stop lap.");

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

        public async Task SyncSaveRegularLapRecord(LapRecord lap)
        {
            try
            {
                if (ConnectivityService.IsConnected())
                {
                    IsRefreshing = true;
                    var _lap = await DataService.PostLapRecordAsync(lap);
                    if (_lap == null)
                    {
                        await _dialogService.Alert("Create Lap Record Failed",
                            "An error occured while creating the lap distance record. Please try again.",
                            "OK");
                    }
                    else
                    {
                        await _dialogService.Snackbar("Distance Saved Successfully!");
                    }
                }
                else
                {
                    IsRefreshing = false;
                    await _dialogService.Alert("No Internet Connection", "Please ensure you have an active network connection and try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                IsRefreshing = false;
            }
            IsRefreshing = false;
        }

        public int GetElapsedTicks()
        {
            if (!string.IsNullOrWhiteSpace(EventTimeStamp))
            {
                //convert to DT
                DateTime dtStarted;
                var dtValid = DateTime.TryParse(EventTimeStamp, out dtStarted);

                if (dtValid)
                {
                    var tsElapsed = (DateTime.Now - dtStarted).TotalSeconds;
                    return (int)tsElapsed;
                }
                else
                {
                    return 0;
                }
            }
            else
                return 0;
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

        [Reactive] public string Property { get; set; }
    }
}