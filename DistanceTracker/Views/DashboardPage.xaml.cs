using Humanizer;
using System.Diagnostics;
using System.Timers;

namespace DistanceTracker;

public partial class DashboardPage : ContentPage
{
    System.Timers.Timer myTimer;
    System.Timers.Timer myScrollerTimer;

    DateTime dtStarted;
    double totalTimeLimitHours;
    public DashboardPageViewModel _vm;
    public bool scrolling;

    public DashboardPage()
	{
		InitializeComponent();

        myTimer = new System.Timers.Timer(1000);
        totalTimeLimitHours = new TimeSpan(12, 0, 0).TotalMilliseconds;

        //MessagingCenter.Subscribe<DashboardPage>(this, "CheckStatus", (sender) =>
        //{
        //    CheckIfEventHasStarted();
        //});

    }

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        var startedWhen = GetTimeLeftToRun(dtStarted);
        Debug.WriteLine(startedWhen);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TimeLeftLabel.Text = $"{startedWhen}";
        });
    }


    private async void BeginScrollingRacersSlowly()
    {
        await Task.Delay(15000);

        var shouldAutoScroll = Preferences.Default.Get("AutoScrollDashboard", false);
        int count = _vm.RacersList.Count;
        int i = 0;
        
        if (shouldAutoScroll) 
        {
            while (scrolling)
            {
                if (_vm.RacersList.Any() && count > 3)
                {
                    for (i = 3; i < count; i++)
                    {
                        if (!_vm.IsRefreshing)
                        {
                            var target = _vm.RacersList[i];
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                RunnersListView.ScrollTo(target, ScrollToPosition.MakeVisible, true);
                            });
                        }

                        await Task.Delay(4000);
                    }
                    i = 0;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        RunnersListView.ScrollTo(_vm.RacersList[0], ScrollToPosition.Start, true);
                    });
                    await Task.Delay(8000);
                }
            }
        }           
    }

    //private async void BeginScrollingTeamsSlowly()
    //{
    //    var shouldAutoScroll = Preferences.Default.Get("AutoScrollDashboard", false);
    //    int count = _vm.RaceTeamsList.Count;
    //    int i = 0;

    //    if (shouldAutoScroll)
    //    {
    //        while (true)
    //        {
    //            if (_vm.RaceTeamsList.Any() && count > 3)
    //            {
    //                for (i = 3; i < count; i++)
    //                {
    //                    if (!_vm.IsRefreshing)
    //                    {
    //                        var target = _vm.RaceTeamsList[i];
    //                        MainThread.BeginInvokeOnMainThread(() =>
    //                        {
    //                            TeamsListView.ScrollTo(target, ScrollToPosition.MakeVisible, true);
    //                        });
    //                    }

    //                    await Task.Delay(4000);
    //                }
    //                i = 0;
    //                MainThread.BeginInvokeOnMainThread(() =>
    //                {
    //                    TeamsListView.ScrollTo(_vm.RaceTeamsList[0], ScrollToPosition.Start, true);
    //                });
    //                await Task.Delay(8000);
    //            }
    //        }
    //    }
    //}

    protected override async void OnAppearing()
    {
        try
        {
            _vm = GetViewModel();

            var refreshInterval = Preferences.Default.Get(Keys.RefreshInterval, 60);
            this.Title = $"Live Stats (Refresh Interval: {refreshInterval}s)";

            CheckIfEventHasStarted();

            
            scrolling = true;
            BeginScrollingRacersSlowly();
            //BeginScrollingTeamsSlowly();
        }
        catch (Exception ex)
        {
        }
        base.OnAppearing();
    }

    public void CheckIfEventHasStarted()
    {
        var timeStarted = Preferences.Default.Get(Keys.CurrentEventTimestamp, string.Empty);
        if (!string.IsNullOrWhiteSpace(timeStarted))
        {
            //convert to DT               
            var dtValid = DateTime.TryParse(timeStarted, out dtStarted);
            if (dtValid)
            {
                TimeLeftLabel.IsVisible = true;

                myTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                myTimer.Enabled = true;
                myTimer.Start();
            }
        }
    }

    protected override void OnDisappearing()
    {
        myTimer.Stop();

        scrolling = false;

        base.OnDisappearing();
    }

    public string GetTimeSinceEventTimerStarted(DateTime dtStarted)
    {
        var tsElapsed = (DateTime.Now - dtStarted).TotalMilliseconds;
        return TimeSpan.FromMilliseconds(tsElapsed).Humanize(3, countEmptyUnits: true, minUnit: Humanizer.Localisation.TimeUnit.Second);
    }

    public string GetTimeLeftToRun(DateTime dtStarted)
    {
        var tsElapsed = (DateTime.Now - dtStarted).TotalMilliseconds;        
        var tsLeft = totalTimeLimitHours - tsElapsed;
        return TimeSpan.FromMilliseconds(tsLeft).Humanize(3, countEmptyUnits: true, minUnit: Humanizer.Localisation.TimeUnit.Second);
    }

    public DashboardPageViewModel GetViewModel()
    {
        return BindingContext as DashboardPageViewModel;
    }
}