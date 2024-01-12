

namespace DistanceTracker
{
    public class ViewLapsResultsPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }
        public string EventName { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public TimedLapRecord SelectedTimedLap { get; set; }
        public List<TimedLapRecord> TimedLapRecordsList { get; set; } = new List<TimedLapRecord>();
        [Reactive] public ObservableCollection<TimedLapRecord> TimedLapRecords { get; set; } = new ObservableCollection<TimedLapRecord>();
        [Reactive] public string TotalNumberOfTimedLaps { get; set; }
        public DelegateCommand<TimedLapRecord> TimedLapRecordSelectedCommand { get; }
        public string EventId { get; set; }

        public ViewLapsResultsPageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
            TimedLapRecordSelectedCommand = new DelegateCommand<TimedLapRecord>(TimedLapRecordSelected);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            EventName = Preferences.Default.Get(Keys.CurrentEventName, string.Empty);

            if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
            {
                //no need to force a refresh of the class list
                IsRefreshing = true;
                await GetTimedLapRecords(EventName, forceRefresh: true);
                IsRefreshing = false;
            }

            base.OnNavigatedTo(parameters);
        }

        public async Task<List<TimedLapRecord>> GetTimedLapRecords(string curRaceEvent, bool forceRefresh = true)
        {
            var timedlaprecordList = new List<TimedLapRecord>();

            try
            {
                var timedlaprecordListResult = await DataService.GetTimedLapRecords(forceRefresh, curRaceEvent);
                timedlaprecordList = timedlaprecordListResult.ToList();
                if (timedlaprecordList != null)
                {
                    TimedLapRecordsList = timedlaprecordList;
                    TimedLapRecords = new ObservableCollection<TimedLapRecord>(TimedLapRecordsList.Where(y => y.IsLapFinished).OrderBy(x => x.TotalLapTime));
                    TotalNumberOfTimedLaps = $"Laps: {TimedLapRecordsList.Count}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetLapRecords - Error getting timed lap records");
            }

            return timedlaprecordList;
        }

        public async void TimedLapRecordSelected(TimedLapRecord lap)
        {
            IsBusy = true;

            try
            {
                var result = await _dialogService.ActionSheet($"Edit {lap.RunnerName}'s Timed Lap Record:",
                    null, "Cancel", "DELETE LAP");

                if (result != null && result != "Cancel")
                {
                    if (result == "DELETE LAP")
                    {
                        var res = await _dialogService.Confirm("Are you sure you want to delete this timed lap record? This cannot be undone.",
                            "Delete Timed Lap Record?", "YES", "CANCEL");

                        if (res)
                        {
                            //delete the lap record
                            DeleteTimedLapRecord(lap);
                        }
                    }
                }


                SelectedTimedLap = null;
            }
            catch (Exception ex)
            {
                IsBusy = false;
            }

            IsBusy = false;
        }

        public async void DeleteTimedLapRecord(TimedLapRecord lap)
        {
            try
            {
                var deleteResult = await DataService.DeleteTimedLapRecord(lap);
                if (deleteResult != null)
                {
                    await _dialogService.Snackbar("Lap record deleted successfully");
                }

                await GetTimedLapRecords(EventName, forceRefresh: true);

                await _dialogService.Alert("the TIMED LAP was deleted; however, the lap record for the overall distance total still remains. you may wish to go delete that now.", "REMINDER", "OK");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Reactive] public string Property { get; set; }
    }
}