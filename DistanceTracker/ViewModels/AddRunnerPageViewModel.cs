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
