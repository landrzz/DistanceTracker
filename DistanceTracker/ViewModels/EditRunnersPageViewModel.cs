using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class EditRunnersPageViewModel : DTViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }
        public string EventName { get; set; }

        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public Runner SelectedRunner { get; set; }
        public List<Runner> RunnersList { get; set; } = new List<Runner>();
        [Reactive] public ObservableCollection<Runner> Runners { get; set; } = new ObservableCollection<Runner>();
        [Reactive] public string TotalNumberOfRunners { get; set; }
        public DelegateCommand<Runner> RunnerSelectedCommand { get; }
        public string EventId { get; set; }
        public EditRunnersPageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
            RunnerSelectedCommand = new DelegateCommand<Runner>(RunnerSelected);
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
                await GetRunners(EventName, forceRefresh: true);
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
                    Runners = new ObservableCollection<Runner>(RunnersList.OrderByDescending(x => x.LastName));
                    TotalNumberOfRunners = $"Runners: {RunnersList.Count}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}  {ex.InnerException}");
                Logger.LogError(ex, "GetRunnerRecords - Error getting runner records");
            }

            return runnersList;
        }

        public async void RunnerSelected(Runner runner)
        {
            IsBusy = true;

            try
            {
                var result = await _dialogService.ActionSheet($"Edit {runner.RunnerName}'s Runner Record:",
                    null, "Cancel", "DELETE RUNNER");

                if (result != null && result != "Cancel")
                {
                    if (result == "DELETE RUNNER")
                    {
                        var res = await _dialogService.Confirm("Are you sure you want to delete this runner from the event? This will also remove any of their previously recorded lap records. This cannot be undone.",
                            "Delete Runner?", "YES", "CANCEL");

                        if (res)
                        {
                            //delete the runner record
                            DeleteRunnerRecord(runner);
                        }
                    }
                }


                SelectedRunner = null;
            }
            catch (Exception ex)
            {
                IsBusy = false;
            }

            IsBusy = false;
        }

        public async void DeleteRunnerRecord(Runner runner)
        {
            try
            {
                var deleteResult = await DataService.DeleteRunner(runner);
                if (deleteResult != null)
                {
                    await _dialogService.Snackbar("Runner deleted successfully");
                }

                await GetRunners(EventName, forceRefresh: true);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
