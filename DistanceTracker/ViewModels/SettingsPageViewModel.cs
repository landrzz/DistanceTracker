using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class SettingsPageViewModel : ViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        
        public ICommand FinalizeSetDistancesCommand => new Command(FinalizeSetDistances);
        public ICommand FinalizeSetRefreshIntervalCommand => new Command(FinalizeSetRefreshInterval);
        public ICommand FinalizeEventTimeLimitCommand => new Command(FinalizeEventTimeLimit);

        

        public ICommand StartEventTimeClockCommand => new Command(StartEventTimeClock);
        public ICommand ImportRunnersCommand => new Command(ImportRunners);



        public bool AutoScrollDashboard
        {
            get => Preferences.Default.Get(nameof(AutoScrollDashboard), false);
            set => Preferences.Default.Set(nameof(AutoScrollDashboard), value);
        }


        public SettingsPageViewModel(BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }
        public async void FinalizeSetDistances()
        {
            await _dialogService.Snackbar("Distances Set Successfully!");
        }

        public async void FinalizeSetRefreshInterval()
        {
            await _dialogService.Snackbar("Refresh Interval Set Successfully!");
        }

        public async void FinalizeEventTimeLimit()
        {
            await _dialogService.Snackbar("Event Time Limit Set Successfully!");
        }

        public async void StartEventTimeClock()
        {
            try
            {
                var answer = await _dialogService.Confirm("Would you like to start the event time clock? This should only be done once. If the event clock has already been started, this operation will fail.", "Start Event Time Clock", "YES", "CANCEL"); ;
                if (answer)
                {
                    //pass in the event id
                    var now = DateTime.Now.ToString();
                    var res = await DataService.PutEventTimeClock(now);

                    if (res != null)
                    {
                        Preferences.Default.Set(Keys.CurrentEventTimestamp, res.EventStartTimestamp);
                        await _dialogService.Snackbar("Time Clock Started!");
                    }
                    else
                    {
                        await _dialogService.Alert("Event time clock not started/set! This may be because the time clock has already been started by another race director.", "Clock was not started.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "StartEventTimeClock - Error setting event start clock");
            }
        }

        public async void ImportRunners()
        {
            try
            {
                var answer = await _dialogService.Confirm("Would you like to start the event time clock? This should only be done once. If the event clock has already been started, this operation will fail.", "Start Event Time Clock", "YES", "CANCEL"); ;
                if (answer)
                {
                    //pass in the event id
                    var now = DateTime.Now.ToString();
                    var res = await DataService.PutEventTimeClock(now);

                    if (res != null)
                    {
                        Preferences.Default.Set(Keys.CurrentEventTimestamp, res.EventStartTimestamp);
                        await _dialogService.Snackbar("Time Clock Started!");
                    }
                    else
                    {
                        await _dialogService.Alert("Event time clock not started/set! This may be because the time clock has already been started by another race director.", "Clock was not started.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "StartEventTimeClock - Error setting event start clock");
            }
        }

        [Reactive] public string Property { get; set; }
    }
}
