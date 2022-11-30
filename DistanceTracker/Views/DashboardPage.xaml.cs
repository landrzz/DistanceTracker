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

    public DashboardPage()
	{
		InitializeComponent();

        myTimer = new System.Timers.Timer(1000);
        myScrollerTimer = new System.Timers.Timer(2000);

        totalTimeLimitHours = new TimeSpan(12, 0, 0).TotalMilliseconds;
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

    private void OnScrollerTimerEvent(object source, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RunnersListView.ScrollTo(_vm.RunnersList[_vm.RunnersList.Count - 1], ScrollToPosition.End, true);
        });

        
    }

    protected override void OnAppearing()
    {
        try
        {
            _vm = GetViewModel();

            var refreshInterval = Preferences.Default.Get(Keys.RefreshInterval, 60);
            this.Title = $"Live Stats (Refresh Interval: {refreshInterval}s)";

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

            myScrollerTimer.Elapsed += new ElapsedEventHandler(OnScrollerTimerEvent);
            myScrollerTimer.Enabled = true;
            myScrollerTimer.Start();
        }
        catch (Exception ex)
        {
        }
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        myTimer.Stop();

        myScrollerTimer.Stop();

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