using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class DashboardPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        [Reactive] public bool ShowLoading { get; set; }
        [Reactive] public string EventName { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public string LeadingFemaleName { get; set; }
        [Reactive] public string LeadingFemaleDistance { get; set; }
        [Reactive] public string LeadingMaleName { get; set; }
        [Reactive] public string LeadingMaleDistance { get; set; }
        [Reactive] public string OverallDistanceTotal { get; set; }
        [Reactive] public string LeadingTeam { get; set; }
        [Reactive] public string LeadingTeamDistance { get; set; }

        public List<Runner> RunnersList { get; set; } = new List<Runner>();
        public List<LapRecord> LapRecordsList { get; set; } = new List<LapRecord>();

        public ObservableCollection<Racer> RacersList { get; set; } = new ObservableCollection<Racer>();

        public DashboardPageViewModel(BaseServices services) : base(services)
        {           
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            var raceEvent = CheckIsEventSet();
            if (!raceEvent)
            {
                await _dialogService.Alert("You must a default event before viewing the dashboard!", "Set Event First!");
                await _navigationService.GoBackAsync();
            }

            if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
            {
                //no need to force a refresh of the class list
                IsRefreshing = true;
                await GetRunners(EventName, forceRefresh: true);
                await GetLapRecords(EventName, forceRefresh: true);

                FormatData();
                IsRefreshing = false;
            }

            base.OnNavigatedTo(parameters);
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
                    RunnersList = runnersList;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetRunners - Error getting runners");
            }

            return runnersList;
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetLapRecords - Error getting lap records");
            }

            return laprecordList;
        }

        public void FormatData()
        {
            try
            {
                RacersList.Clear();
                var groupedLapRecordsList = LapRecordsList.GroupBy(u => u.BibNumber)
                        .Select(grp => grp.ToList())
                        .ToList();

                
               
                foreach (var grp in groupedLapRecordsList)
                {
                    var grpItem = grp.FirstOrDefault();
                    var racer = new Racer()
                    {
                        RacerName = grpItem?.RunnerName,
                        RaceEventName = grpItem?.RaceEventName,
                        BibNumber = grpItem?.BibNumber, 
                    };

                    var runr = RunnersList.FirstOrDefault(x => x.BibNumber == racer.BibNumber);
                    racer.Age = runr.Age;
                    racer.Sex = runr.Sex;
                    racer.TeamName = runr.TeamName;
                    racer.CompletedLaps = grp;

                    RacersList.Add(racer);
                }

                if (RacersList.Any())
                {
                    var groupedRunnerTeams = RacersList
                        .Where(x => x.TeamName != "SOLO")
                        .GroupBy(u => u.TeamName)
                        .Select(grp => grp.ToList())
                        .ToList();

                    var topDistanceRunnerMale = RacersList.Where(x => x.Sex == "Male").MaxBy(x => x.CurrentMileageTotal);

                    var topDistanceRunnerFemale = RacersList.Where(x => x.Sex == "Female").MaxBy(x => x.CurrentMileageTotal);

                    LeadingFemaleDistance = topDistanceRunnerFemale?.CurrentMileageTotal.ToString() ?? "0";
                    LeadingMaleDistance = topDistanceRunnerMale?.CurrentMileageTotal.ToString() ?? "0";

                    LeadingMaleName = topDistanceRunnerMale?.RacerName ?? "John Doe";
                    LeadingFemaleName = topDistanceRunnerFemale?.RacerName ?? "Jane Doe";
                }

                OverallDistanceTotal = LapRecordsList.Sum(x => double.Parse(x.LapDistance)).ToString("N2");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "FormatData - Error formatting dashboard data");
            }
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
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
    }

    public class Racer
    {        
        public string BibNumber { get; set; }
        public string RacerName { get; set; }
        public string RaceEventName { get; set; }

        public string Age { get; set; }
        public string Sex { get; set; }
        public string TeamName { get; set; }

        public List<LapRecord> CompletedLaps { get; set; }

        public double CurrentMileageTotal => GetCurrentMileageTotal(CompletedLaps);
        
        public double GetCurrentMileageTotal (List<LapRecord> CompletedLaps)
        {
            double total = CompletedLaps.Sum(item => double.Parse(item.LapDistance));
            return total;
        }
    }

}
