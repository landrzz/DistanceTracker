using Humanizer;
using System.Diagnostics;
using System.Timers;

namespace DistanceTracker;

public partial class DashboardPage : ContentPage
{
    System.Timers.Timer myTimer;
    DateTime dtStarted;
    double total12HourMiliseconds; 
    public DashboardPage()
	{
		InitializeComponent();

        myTimer = new System.Timers.Timer(1000);
        total12HourMiliseconds = new TimeSpan(12, 0, 0).TotalMilliseconds;
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

    protected override void OnAppearing()
    {
        try
        {
            var refreshInterval = Preferences.Get(Keys.RefreshInterval, 60);
            this.Title = $"Live Stats (refresh every {refreshInterval}s)";

            var timeStarted = Preferences.Get(Keys.CurrentEventTimestamp, string.Empty);
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
        catch (Exception ex)
        {
        }
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        myTimer.Stop();

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
        var tsLeft = total12HourMiliseconds - tsElapsed;
        return TimeSpan.FromMilliseconds(tsLeft).Humanize(3, countEmptyUnits: true, minUnit: Humanizer.Localisation.TimeUnit.Second);
    }
}