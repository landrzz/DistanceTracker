using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class EditLapsPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }
        public string EventName { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public LapRecord SelectedLap { get; set; }
        public List<LapRecord> LapRecordsList { get; set; } = new List<LapRecord>();
        [Reactive] public ObservableCollection<LapRecord> LapRecords { get; set; } = new ObservableCollection<LapRecord>();
        [Reactive] public string TotalNumberOfLaps { get; set; }
        public DelegateCommand<LapRecord> LapRecordSelectedCommand { get; }
        public string EventId { get; set; }
        public EditLapsPageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
            LapRecordSelectedCommand = new DelegateCommand<LapRecord>(LapRecordSelected);
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
                await GetLapRecords(EventName, forceRefresh: true);
                IsRefreshing = false;
            }

            base.OnNavigatedTo(parameters);
        }

        public async Task<List<LapRecord>> GetLapRecords(string curRaceEvent, bool forceRefresh = true)
        {
            var laprecordList = new List<LapRecord>();

            try
            {
                var laprecordListResult = await DataService.GetLapRecords(forceRefresh, curRaceEvent);
                laprecordList = laprecordListResult.ToList();
                if (laprecordList != null)
                {
                    LapRecordsList = laprecordList;
                    LapRecords = new ObservableCollection<LapRecord>(LapRecordsList.OrderByDescending(x => x.LapCompletedTime));
                    TotalNumberOfLaps = $"Laps: {LapRecordsList.Count}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetLapRecords - Error getting lap records");
            }

            return laprecordList;
        }

        public async void LapRecordSelected(LapRecord lap)
        {
            IsBusy = true;

            try
            {
                var result = await _dialogService.ActionSheet($"Edit {lap.RunnerName}'s Lap Record:",
                    null, "Cancel", "DELETE LAP");

                if (result != null && result != "Cancel")
                {
                    if (result == "DELETE LAP") 
                    {
                        var res = await _dialogService.Confirm("Are you sure you want to delete this lap record? This cannot be undone.",
                            "Delete Lap Record?", "YES", "CANCEL");

                        if (res)
                        {
                            //delete the lap record
                            DeleteLapRecord(lap);
                        }
                    }
                }


                SelectedLap = null;
            }
            catch (Exception ex)
            {
                IsBusy = false;
            }

            IsBusy = false;
        }

        public async void DeleteLapRecord(LapRecord lap)
        {
            try
            {
                var deleteResult = await DataService.DeleteLapRecord(lap);
                if (deleteResult != null)
                {
                    await _dialogService.Snackbar("Lap record deleted successfully");
                }

                await GetLapRecords(EventName, forceRefresh: true);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
