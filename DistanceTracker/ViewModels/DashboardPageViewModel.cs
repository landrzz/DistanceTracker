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

        public DateTime EventStartTime;
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
        [Reactive] public ObservableCollection<Deltas> TopDeltasList { get; set; } = new ObservableCollection<Deltas>();

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

                var curEventId = Preferences.Default.Get(Keys.CurrentEventId, string.Empty);

                refreshTimer.Stop();
                var interval = Preferences.Default.Get(Keys.RefreshInterval, 60);
                refreshTimer.Interval = interval * 1000;

                if (parameters.GetNavigationMode() != Prism.Navigation.NavigationMode.Back)
                {
                    //no need to force a refresh of the class list
                    IsRefreshing = true;
                    await GetRunners(EventName, forceRefresh: true);
                    await GetLapRecords(EventName, forceRefresh: true);
                    await RefreshCurrentEventDetails(curEventId);

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
                    if (runr != null)
                    {
                        racer.Age = runr.Age;
                        racer.Sex = runr.Sex;
                        racer.TeamName = runr.TeamName;

                    }

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

                        var teamtotal = grp.Sum(x => x.CurrentMileageTotal);
                        raceTeam.CurrrentTeamMileageDistance = Math.Round(teamtotal, 1);

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

                // Group laps by runner (BibNumber) *AND* LapDistance
                var groupedLapRecordsList_Deltas = LapRecordsList
                    .GroupBy(l => new { l.BibNumber, l.LapDistance })
                    .Select(grp => grp.OrderBy(x => x.LapCompletedTimeLocal).ToList())
                    .ToList();

                CalculateTotalElapsedTimeForLaps(EventStartTime, groupedLapRecordsList_Deltas);

                BuildDeltasList(groupedLapRecordsList_Deltas);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "FormatData - Error formatting dashboard data");
            }
        }

        private void BuildDeltasList(List<List<LapRecord>> groupedLapRecordsList_Deltas)
        {
            try
            {
                var deltasList = new List<Deltas>();

                foreach (var lapGroup in groupedLapRecordsList_Deltas)
                {
                    var lapCount = lapGroup.Count;
                    if (lapCount == 0)
                        continue; // No laps? Nothing to do.

                    // Extract some basics from the group
                    var bibNumber = lapGroup[0].BibNumber;
                    var runnerName = lapGroup[0].RunnerName;

                    // Look up the runner to retrieve Sex (or other info)
                    var runnerInfo = RunnersList.FirstOrDefault(r => r.BibNumber == bibNumber);
                    var runnerSex = runnerInfo?.Sex ?? string.Empty;

                    // We’ll track the best (fastest) delta in seconds
                    double bestDeltaSeconds = double.MaxValue;
                    LapRecord bestDeltaLap = null;

                    // ----------------------------------
                    //  Handle SINGLE-LAP scenario
                    // ----------------------------------
                    if (lapCount == 1)
                    {
                        // Just compute the time from event start to first (and only) lap
                        var singleLap = lapGroup[0];
                        if (singleLap.LapCompletedTimeLocal.HasValue)
                        {
                            bestDeltaSeconds = (singleLap.LapCompletedTimeLocal.Value - EventStartTime).TotalSeconds;
                            bestDeltaLap = singleLap;
                        }
                    }
                    else
                    {
                        // ----------------------------------
                        //  Handle MULTI-LAP scenario
                        // ----------------------------------
                        for (int i = 1; i < lapCount; i++)
                        {
                            DateTime? currentLapTime = lapGroup[i].LapCompletedTimeLocal;
                            DateTime? previousLapTime = lapGroup[i - 1].LapCompletedTimeLocal;
                            if (!currentLapTime.HasValue || !previousLapTime.HasValue)
                                continue;

                            var deltaSeconds = (currentLapTime.Value - previousLapTime.Value).TotalSeconds;
                            if (deltaSeconds < bestDeltaSeconds)
                            {
                                bestDeltaSeconds = deltaSeconds;
                                bestDeltaLap = lapGroup[i];
                            }
                        }
                    }

                    // If we found a bestDeltaLap, build a Deltas object and add it
                    if (bestDeltaLap != null)
                    {
                        var bestDeltasObject = new Deltas
                        {
                            RacerName = runnerName,
                            BibNumber = bibNumber,
                            LapDistance = bestDeltaLap.LapDistance,
                            DeltaTimespan = TimeSpan.FromSeconds(bestDeltaSeconds),
                            Sex = runnerSex
                        };
                        deltasList.Add(bestDeltasObject);
                    }
                }

                // Sort them in ascending order (fastest = smallest timespan)
                var sortedDeltas = deltasList
                    .OrderBy(d => d.DeltaTimespan)
                    .ToList();

                // Assign them to the reactive property
                TopDeltasList = new ObservableCollection<Deltas>(sortedDeltas);
            }
            catch (Exception ex)
            {
                // In production, always log or handle exceptions
                System.Diagnostics.Debug.WriteLine($"{ex.Message} :: {ex.InnerException}");
                Logger.LogError(ex, "BuildDeltasList - Error computing deltas");
            }
        }


        //private void BuildDeltasList(List<List<LapRecord>> groupedLapRecordsList_Deltas)
        //{
        //    try
        //    {
        //        var deltasList = new List<Deltas>();

        //        foreach (var lapGroup in groupedLapRecordsList_Deltas)
        //        {
        //            // If the runner has fewer than 2 laps, we can’t compute a delta
        //            if (lapGroup.Count() < 2)
        //                continue;

        //            // Extract some basics from the group
        //            var bibNumber = lapGroup[0].BibNumber;
        //            var runnerName = lapGroup[0].RunnerName;

        //            // Look up the runner to retrieve Sex (or other info)
        //            var runnerInfo = RunnersList.FirstOrDefault(r => r.BibNumber == bibNumber);
        //            var runnerSex = runnerInfo?.Sex ?? string.Empty;

        //            // We’ll track the best (fastest) delta in seconds
        //            double bestDeltaSeconds = double.MaxValue;
        //            LapRecord bestDeltaLap = null;

        //            // Pairwise check consecutive laps
        //            for (int i = 1; i < lapGroup.Count(); i++)
        //            {
        //                DateTime? currentLapTime = lapGroup[i].LapCompletedTimeLocal;
        //                DateTime? previousLapTime = lapGroup[i - 1].LapCompletedTimeLocal;
        //                if (!currentLapTime.HasValue || !previousLapTime.HasValue)
        //                    continue;

        //                var deltaSeconds = (currentLapTime.Value - previousLapTime.Value).TotalSeconds;
        //                if (deltaSeconds < bestDeltaSeconds)
        //                {
        //                    bestDeltaSeconds = deltaSeconds;
        //                    bestDeltaLap = lapGroup[i];
        //                }
        //            }

        //            // If we found a bestDeltaLap, build a Deltas object and add it
        //            if (bestDeltaLap != null)
        //            {
        //                var bestDeltasObject = new Deltas
        //                {
        //                    RacerName = runnerName,
        //                    BibNumber = bibNumber,
        //                    LapDistance = bestDeltaLap.LapDistance,
        //                    DeltaTimespan = TimeSpan.FromSeconds(bestDeltaSeconds),
        //                    Sex = runnerSex
        //                };
        //                deltasList.Add(bestDeltasObject);
        //            }
        //        }

        //        // 4) Sort or filter them in any way you like
        //        var sortedDeltas = deltasList
        //            .OrderBy(d => d.DeltaTimespan)  // fastest first
        //            .ToList();

        //        // 5) Assign them to the reactive property
        //        TopDeltasList = new ObservableCollection<Deltas>(sortedDeltas);
        //    }
        //    catch
        //    {

        //    }

        //}


        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
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

        public async Task RefreshCurrentEventDetails(string curId, bool forceRefresh = true)
        {
            var raceEvent = new RaceEvent();

            try
            {
                var rEvent = await DataService.GetEvent(forceRefresh, curId);
                raceEvent = rEvent;
                if (raceEvent != null && !string.IsNullOrWhiteSpace(raceEvent.EventName))
                {
                    Preferences.Default.Set(Keys.CurrentEventTimestamp, raceEvent.EventStartTimestamp);
                    EventStartTime = DateTime.Parse(raceEvent.EventStartTimestamp);

                    await Task.Delay(2000);

                    //MessagingCenter.Send<DashboardPage>(null, "CheckStart");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "RefreshCurrentEventDetails - Error getting race event details");
            }
        }

        //////////////////////////////////////////////////
        ///
        public void CalculateTotalElapsedTimeForLaps(
        DateTime eventStartTime,
        List<List<LapRecord>> groupedLapRecordsList)
        {
            foreach (var lapGroup in groupedLapRecordsList)
            {
                // The "previous" time for the first lap is the event start
                DateTime previousLapTime = eventStartTime;

                // Running total of elapsed seconds from event start
                int totalElapsedSeconds = 0;

                foreach (var lap in lapGroup)
                {

                    var lapDateTime = lap.LapCompletedTimeLocal.Value;

                    // How many seconds between this lap's completion and the previous checkpoint?
                    var delta = (lapDateTime - previousLapTime).TotalSeconds;

                    // Accumulate total elapsed time
                    totalElapsedSeconds += Convert.ToInt32(delta);

                    // Assign that running total to LapTimeSpan (the "total time" from event start)
                    lap.LapTimeSpan = totalElapsedSeconds;

                    // Update "previous" time
                    previousLapTime = lapDateTime;
                }
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
            var tot = Math.Round(total, 1);
            return tot;
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

    public class Deltas
    {
        public string RacerName { get; set; }
        public string BibNumber { get; set; }
        public string LapDistance { get; set; }
        public TimeSpan DeltaTimespan { get; set; }
        public string Sex { get; set; }

        public string DelaTimeSpanFormatted => DeltaTimespan.ToString(@"hh\:mm\:ss");

        public string DeltaAndDistance => $"{DeltaTimespan} ({LapDistance})";
    }
}
