using DynamicData.Aggregation;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DistanceTracker
{
    public class DashboardPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        System.Timers.Timer refreshTimer;
        public string EventId { get; set; }
        public ElapsedEventHandler refreshHandler;
        [Reactive] public bool ShowLoading { get; set; }
        [Reactive] public string EventName { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public string LeadingFemaleName { get; set; }
        [Reactive] public string LeadingFemaleDistance { get; set; }
        [Reactive] public string LeadingMaleName { get; set; }
        [Reactive] public string LeadingMaleDistance { get; set; }
        [Reactive] public string OverallDistanceTotal { get; set; }

        //Teams
        [Reactive] public string LeadingTeamNameMale { get; set; }
        [Reactive] public string LeadingTeamDistanceMale { get; set; }
        [Reactive] public string LeadingTeamNameFemale { get; set; }
        [Reactive] public string LeadingTeamDistanceFemale { get; set; }
        [Reactive] public string LeadingTeamNameCoed { get; set; }
        [Reactive] public string LeadingTeamDistanceCoed { get; set; }

        public List<Runner> RunnersList { get; set; } = new List<Runner>();
        public List<LapRecord> LapRecordsList { get; set; } = new List<LapRecord>();

        [Reactive] public ObservableCollection<Racer> RacersList { get; set; } = new ObservableCollection<Racer>();
        [Reactive] public ObservableCollection<RaceTeam> RaceTeamsList { get; set; } = new ObservableCollection<RaceTeam>();

        public DashboardPageViewModel(BaseServices services) : base(services)
        {           
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;

            refreshTimer = new System.Timers.Timer(60000);

            refreshHandler = new ElapsedEventHandler(OnTimedEvent);

            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                CheckIsEventIdSet();
                var raceEvent = CheckIsEventSet();
                if (!raceEvent)
                {
                    await _dialogService.Alert("You must a default event before viewing the dashboard!", "Set Event First!");
                    await _navigationService.GoBackAsync();
                }

                refreshTimer.Stop();
                var interval = Preferences.Get(Keys.RefreshInterval, 60);
                refreshTimer.Interval = interval * 1000;

                if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
                {
                    //no need to force a refresh of the class list
                    IsRefreshing = true;
                    await GetRunners(EventName, forceRefresh: true);
                    await GetLapRecords(EventName, forceRefresh: true);

                    FormatData();
                    IsRefreshing = false;
                }

                refreshTimer.Elapsed += refreshHandler;
                refreshTimer.Enabled = true;
                refreshTimer.Start();
            }
            catch (Exception)
            {

                throw;
            }

            base.OnNavigatedTo(parameters);
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            try
            {
                refreshTimer.Elapsed -= refreshHandler;
                refreshTimer.Enabled = false;
                refreshTimer.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "OnNavigatedFrom");
            }

            base.OnNavigatedFrom(parameters);
        }

        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    IsRefreshing = true;
                    await GetRunners(EventName, forceRefresh: true);
                    await GetLapRecords(EventName, forceRefresh: true);
                    FormatData();
                    IsRefreshing = false;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "OnTimedEvent - Error on timer elapsed");
            }

            //var startedWhen = GetTimeLeftToRun(dtStarted);
            //Debug.WriteLine(startedWhen);
            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    TimeLeftLabel.Text = $"{startedWhen}";
            //});
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
                RacersList = new ObservableCollection<Racer>(RacersList.OrderByDescending(x => x.CurrentMileageTotal));

                if (RacersList.Any())
                {
                    var groupedRunnerTeams = RacersList
                        .Where(x => x.TeamName != "SOLO")
                        .GroupBy(u => u.TeamName)
                        .Select(grp => grp.ToList())
                        .ToList();


                    //var teamsDetails = groupedRunnerTeams.Select(x => new
                    //{
                    //    list = x,
                    //    total = x.Sum(m => m.CurrentMileageTotal),
                    //    teamName = x.Select(n => n.TeamName).FirstOrDefault()
                    //}).OrderByDescending(x => x.total);
                    //var leadingTeamDetails = teamsDetails.FirstOrDefault();

                    var _raceTeamsList = new List<RaceTeam>();
                    foreach (var grp in groupedRunnerTeams)
                    {
                        var grpItm = grp.FirstOrDefault();
                        var raceTeam = new RaceTeam()
                        {
                            TeamName = grpItm.TeamName,
                            RaceEventName = grpItm.RaceEventName,
                        };

                        raceTeam.CurrrentTeamMileageDistance = grp.Sum(x => x.CurrentMileageTotal);
                        raceTeam.BibNumbers = string.Join("|", grp.Select(x => x.BibNumber));
                        raceTeam.TeamMemberNames = string.Join("|", grp.Select(x => x.RacerName));

                        if (grp.All(s => s.Sex == "Male"))
                        {
                            raceTeam.TeamType = "Male";
                        }
                        else if (grp.All(s => s.Sex == "Female"))
                        {
                            raceTeam.TeamType = "Female";
                        }
                        else
                        {
                            raceTeam.TeamType = "Coed";
                        }

                        _raceTeamsList.Add(raceTeam);
                    }
                    RaceTeamsList = new ObservableCollection<RaceTeam>(_raceTeamsList.OrderByDescending(x => x.CurrrentTeamMileageDistance));

                    var raceTeamsOrderedMale = _raceTeamsList.Where(s => s.TeamType == "Male").OrderByDescending(x => x.CurrrentTeamMileageDistance).ToList();
                    var raceTeamsOrderedFemale = _raceTeamsList.Where(s => s.TeamType == "Female").OrderByDescending(x => x.CurrrentTeamMileageDistance).ToList();
                    var raceTeamsOrderedCoed = _raceTeamsList.Where(s => s.TeamType == "Coed").OrderByDescending(x => x.CurrrentTeamMileageDistance).ToList();

                    var leadingTeamM = raceTeamsOrderedMale.FirstOrDefault();
                    var leadingTeamF = raceTeamsOrderedFemale.FirstOrDefault();
                    var leadingTeamCoed = raceTeamsOrderedCoed.FirstOrDefault();



                    var topDistanceRunnerMale = RacersList.Where(x => x.Sex == "Male").MaxBy(x => x.CurrentMileageTotal);
                    var topDistanceRunnerFemale = RacersList.Where(x => x.Sex == "Female").MaxBy(x => x.CurrentMileageTotal);

                    LeadingFemaleDistance = topDistanceRunnerFemale?.CurrentMileageTotal.ToString() ?? "0";
                    LeadingMaleDistance = topDistanceRunnerMale?.CurrentMileageTotal.ToString() ?? "0";
                    LeadingMaleName = topDistanceRunnerMale?.RacerName ?? "John Doe";
                    LeadingFemaleName = topDistanceRunnerFemale?.RacerName ?? "Jane Doe";
                    LeadingTeamNameMale = leadingTeamM?.TeamName ?? "Team";
                    LeadingTeamNameFemale = leadingTeamF?.TeamName ?? "Team";
                    LeadingTeamNameCoed = leadingTeamCoed?.TeamName ?? "Team";
                    LeadingTeamDistanceMale = leadingTeamM?.CurrrentTeamMileageDistance.ToString() ?? "0";
                    LeadingTeamDistanceFemale = leadingTeamF?.CurrrentTeamMileageDistance.ToString() ?? "0";
                    LeadingTeamDistanceCoed = leadingTeamCoed?.CurrrentTeamMileageDistance.ToString() ?? "0";

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
            var raceEvent = Preferences.Get(Keys.CurrentEventName, string.Empty);
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

        public bool CheckIsEventIdSet()
        {
            var raceEventId = Preferences.Get(Keys.CurrentEventId, string.Empty);
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

    public class RaceTeam
    {
        public string BibNumbers { get; set; }
        public string TeamMemberNames { get; set; }
        public string TeamName { get; set; }
        public string RaceEventName { get; set;}
        public double CurrrentTeamMileageDistance { get; set; }
        public string TeamType { get; set; }
    }

}
