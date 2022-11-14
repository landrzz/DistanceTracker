using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class AddRunnerPageViewModel : ViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }

        public string Age;
        public string SelectedSex { get; set; }
        public string BibNumber { get; set; }

        public bool ShowLoading { get; set; }

        public ICommand SaveRunnerCommand => new Command(SaveNewRunner);
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }
        public DelegateCommand<string> NavigateCommand { get; }

        public AddRunnerPageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
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
                    var curEventName = Preferences.Default.Get("currenteventname", string.Empty);

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
                        return;
                    }

                    var newRunner = new Runner()
                    {
                        FirstName = FirstName.Trim(),
                        LastName = LastName.Trim(),
                        BirthDate = BirthDate,
                        Sex = SelectedSex,
                        BibNumber = BibNumber,
                        EventName = curEventName,                       
                    };

                    var user = await DataService.PostRunnerAsync(newRunner);
                    if (user == null)
                    {                    
                        
                        await _dialogService.Alert("Create Runner Failed",
                            "An error occured while creating the runner record. Please try again.",
                            "OK");
                    }
                    else
                    {
                        //show a success toast?

                        //ShowLoading = false;
                        //var navP = new NavigationParameters();
                        //navP.Add(Keys.SuccessPopup_Message, AppStrings.AccountCreated);
                        //await _navigationService.NavigateAsync("SuccessPopupPage", navP, false, true);
                        //await Task.Delay(1300);
                        //await _navigationService.ClearPopupStackAsync();


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
            var raceEvent = Preferences.Get("currenteventname", string.Empty);
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
