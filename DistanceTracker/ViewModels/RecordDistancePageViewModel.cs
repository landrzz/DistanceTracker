
using Shiny;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class RecordDistancePageViewModel : ViewModel
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

        public RecordDistancePageViewModel(Shiny.BaseServices services) : base(services)
        {
            SelectionChangedCommand = new DelegateCommand<Runner>(RunnerSelected);

            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            var distancesSet = CheckAreDistancesSet();
            var raceEvent = CheckIsEventSet();

            if (!raceEvent || !distancesSet)
            {
                await _dialogService.Alert("You must specify both a default event and default event distances before recording distance!", "Set Event First!");
                await _navigationService.GoBackAsync();
            }

            var currentDistances = Preferences.Default.Get("distances", string.Empty);
            if (!string.IsNullOrWhiteSpace(currentDistances))
            {
                ////may have things like 13.1 or 5K - need to convert to mileage
                //DistancesMixed = currentDistances.Split(',').ToList();

                //foreach (var d in DistancesMixed)
                //{
                //    if (d.ToLower().Contains("k"))
                //    {
                //        var dClean = d.Replace("K", "");
                //    }
                //}

                //only mileage supported for right now
                DistancesMixed = currentDistances.Split(',').ToList();
                foreach (var d in DistancesMixed)
                {
                    var mileDbl = double.Parse(d);
                    DistancesMiles.Add(mileDbl);
                }
            }

            if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
            {
                IsRefreshing = true;                
                await GetRunners(EventName, forceRefresh: true);
                IsRefreshing = false;
            }
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
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
                    RunnersList = new ObservableCollection<Runner>(runnersList.OrderBy(x => int.Parse(x.BibNumber)));
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


                var result = await _dialogService.ActionSheet($"{runner.RunnerName}'s Lap Distance:",
                    null, "Cancel", distances);
                    
                if (result != null && result != "Cancel")
                {
                    //a distance was selected
                    var distance = result;

                    //create new lap record
                    var lapRecord = new LapRecord()
                    {
                        LapCompletedTime = DateTime.Now,
                        BibNumber = runner.BibNumber,
                        RunnerName = runner.RunnerName,
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

        public async void SyncSaveLapRecord(LapRecord lap)
        {           
            try
            {
                if (ConnectivityService.IsConnected())
                {
                    ShowLoading = true;
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
            var raceEvent = Preferences.Get("currenteventname", string.Empty);
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

        public bool CheckAreDistancesSet()
        {
            var distances = Preferences.Get("distances", string.Empty);
            if (string.IsNullOrWhiteSpace(distances))
            {
                return false;
            }
            else
                return true;
        }


        [Reactive] public string Property { get; set; }
    }
}
