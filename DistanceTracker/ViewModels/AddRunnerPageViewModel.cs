using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class AddRunnerPageViewModel : ViewModel
    {
        [Reactive] public string FirstName { get; set; }
        [Reactive] public string LastName { get; set; }
        [Reactive] public DateTime BirthDate { get; set; }

        [Reactive] public string Age { get; set; }
        [Reactive] public string SelectedSex { get; set; }
        [Reactive] public string BibNumber { get; set; }
        [Reactive] public string TeamName { get; set; }

        [Reactive] public bool ShowLoading { get; set; }

        public ICommand SaveRunnerCommand => new Command(SaveNewRunner);
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }
        public DelegateCommand<string> NavigateCommand { get; }

        public AddRunnerPageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);

            BirthDate = new DateTime(1990, 6, 15);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            //check event is set
            var eventSet = CheckIsEventSet();
            if (!eventSet)
            {
                await _dialogService.Alert("You must specify a default event before adding a runner!", "Set Event First!");
                await _navigationService.GoBackAsync();
            }

            base.OnNavigatedTo(parameters);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }

        public async void SaveNewRunner()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                if (ConnectivityService.IsConnected())
                {
                    ShowLoading = true;
                    var curEventName = Preferences.Default.Get(Keys.CurrentEventName, string.Empty);

                    if (string.IsNullOrWhiteSpace(curEventName))
                    {
                        //cannot add a runner without attaching them to a current event
                        Debug.WriteLine("Default event not yet set. Try adding an event first.");
                    }

                    int i = 0;                   
                    bool result = int.TryParse(BibNumber, out i);
                    if (!result)
                    {
                        ShowLoading = false;
                        IsBusy = false;
                        await _dialogService.Snackbar("Bib Number is a required field!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(SelectedSex))
                    {
                        ShowLoading = false;
                        IsBusy = false;
                        await _dialogService.Snackbar("Sex is a required field!");
                        return;
                    }

                    var rand = new Random();
                    var randNum = rand.Next(100, 999);

                    if (string.IsNullOrWhiteSpace(FirstName))
                        FirstName = $"Racer{randNum}";

                    if (string.IsNullOrWhiteSpace(LastName))
                        LastName = $"Racer{randNum}";

                    if (string.IsNullOrWhiteSpace(TeamName))
                        TeamName = $"SOLO";

                    if (BirthDate.Year == 1900)
                        BirthDate = new DateTime(1970, 1, 1);

                    if (string.IsNullOrWhiteSpace(Age))
                        Age = "0";

                    //if (string.IsNullOrWhiteSpace(SelectedSex))
                    //    SelectedSex = "Not Specified";

                    var newRunner = new Runner()
                    {
                        FirstName = FirstName.Trim(),
                        LastName = LastName.Trim(),
                        BirthDate = BirthDate,
                        Sex = SelectedSex,
                        BibNumber = BibNumber,
                        EventName = curEventName, 
                        TeamName = TeamName,
                    };

                    var runner = await DataService.PostRunnerAsync(newRunner);
                    if (runner == null)
                    {                                           
                        await _dialogService.Alert("Create Runner Failed",
                            "An error occured while creating the runner record. Please try again.",
                            "OK");
                    }
                    else
                    {
                        await _dialogService.Snackbar("Runner Saved Successfully!");
                        BibNumber = string.Empty;
                        FirstName = string.Empty;
                        LastName = string.Empty;
                        SelectedSex = string.Empty;
                        Age = string.Empty;
                        BirthDate = new DateTime(1990, 6, 15);
                        TeamName = string.Empty;
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
            IsBusy = false;
            ShowLoading = false;
        }

        public async void SaveImportedRunners(List<Runner> runners)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                if (ConnectivityService.IsConnected())
                {
                    ShowLoading = true;
                    var curEventName = Preferences.Default.Get(Keys.CurrentEventName, string.Empty);

                    if (string.IsNullOrWhiteSpace(curEventName))
                    {
                        //cannot add a runner without attaching them to a current event
                        Debug.WriteLine("Default event not yet set. Try adding an event first.");
                    }

                    int failureCount = 0;
                    var failedNames = string.Empty;
                    foreach (var runnr in runners)
                    {
                        var _firstName = string.Empty;
                        var _lastName = string.Empty;
                        var _birthDate = new DateTime();
                        var _sex = string.Empty;
                        var _bibNumber = string.Empty;
                        var _eventName = string.Empty;
                        var _teamName = string.Empty;

                        //add a runner
                        var rand = new Random();
                        var randNum = rand.Next(100, 9999);
                        if (string.IsNullOrWhiteSpace(runnr.FirstName))
                            _firstName = $"Racer{randNum}";
                        else
                            _firstName = runnr.FirstName;

                        if (string.IsNullOrWhiteSpace(runnr.LastName))
                            _lastName = $"Racer{randNum}";
                        else
                            _lastName = runnr.LastName;

                        int i = 0;
                        bool result = int.TryParse(runnr.BibNumber.ToString(), out i);
                        if (!result)
                        {
                            failureCount++;
                            failedNames += $"{_firstName} {_lastName} - NO BIB #\n";
                            await _dialogService.Snackbar("Failed - Bib # is required!", 700);
                            continue;
                        }
                        _bibNumber = runnr.BibNumber;

                        if (string.IsNullOrWhiteSpace(runnr.Sex))
                        {
                            failureCount++;
                            failedNames += $"{_firstName} {_lastName} - SEX UNKNOWN\n";
                            await _dialogService.Snackbar("Failed - Sex required!", 700);
                            continue;
                        }
                        else
                        {
                            if (runnr.Sex == "M")
                                _sex = "Male";

                            if (runnr.Sex == "F")
                                _sex = "Female";
                        }

                        if (string.IsNullOrWhiteSpace(runnr.TeamName))
                            _teamName = $"SOLO";
                        else
                            _teamName = runnr.TeamName;

                        if (runnr.BirthDate.Year == 1900)
                            _birthDate = new DateTime(1970, 1, 1);
                        else
                            _birthDate = runnr.BirthDate;


                        var newRunner = new Runner()
                        {
                            FirstName = _firstName.Trim(),
                            LastName = _lastName.Trim(),
                            BirthDate = _birthDate,
                            Sex = _sex,
                            BibNumber = _bibNumber,
                            EventName = curEventName,
                            TeamName = _teamName,
                        };

                        var runner = await DataService.PostRunnerAsync(newRunner);
                        if (runner == null)
                        {
                            failureCount++;
                            failedNames += $"{_firstName} {_lastName} - ERR UNK\n";
                            await _dialogService.Snackbar("Failed - ERR UNK! (possible bib# duplicate)", 700);

                            await _dialogService.Alert(
                                $"An error occured while creating {newRunner.RunnerName}'s runner record. Please try again.",
                                "Create Runner Failed",
                                "OK");
                        }
                        else
                        {
                            await _dialogService.Snackbar("Runner Saved Successfully!", 700);
                            BibNumber = string.Empty;
                            FirstName = string.Empty;
                            LastName = string.Empty;
                            SelectedSex = string.Empty;
                            Age = string.Empty;
                            BirthDate = new DateTime(1990, 6, 15);
                            TeamName = string.Empty;
                        }

                        await Task.Delay(1700);
                    }

                    if (failureCount > 0)
                    {
                        await _dialogService.Alert(
                                $"Out of {runners.Count} runner records, there were {failureCount} failures.\n\n" +
                                $"Failed Imports are listed below:\n" +
                                $"{failedNames}",
                                "Import Complete!",
                                "OK");
                    }
                    else
                    {
                        await _dialogService.Alert(
                                $"Out of {runners.Count} runner records, there were {failureCount} failures.",
                                "Import Complete!",
                                "OK");
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
            finally
            {
                IsBusy = false;
                ShowLoading = false;
            }  
        }




        public bool CheckIsEventSet()
        {
            var raceEvent = Preferences.Default.Get(Keys.CurrentEventName, string.Empty);
            if (string.IsNullOrWhiteSpace(raceEvent))
            {
                return false;
            }
            else
                return true;
        }

        [Reactive] public string Property { get; set; }
    }
}
