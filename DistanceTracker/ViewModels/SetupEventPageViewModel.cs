using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DistanceTracker
{
    public class SetupEventPageViewModel : ViewModel
    {
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public string EventPassword { get; set; }

        public string EventYear;

        public bool ShowLoading { get; set; }

        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        public ICommand SaveEventCommand => new Command(SaveEvent);

        public SetupEventPageViewModel(BaseServices services) : base(services)
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

        public async void SaveEvent()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                if (string.IsNullOrWhiteSpace(EventPassword) || string.IsNullOrWhiteSpace(EventName))
                {
                    await _dialogService.Alert("Fill Out all Details", "All details must be filled out in order to save the race event.", "OK");
                    IsBusy = false;
                    return;
                }
                else if (!string.IsNullOrWhiteSpace(EventPassword))
                {
                    var confirmationPass = await _dialogService.Input("Confirm the event passcode: ", "Password Confirmation", "OK", "Cancel");
                    if (!string.IsNullOrWhiteSpace(confirmationPass?.ToString()))
                    {
                        if (confirmationPass.ToString() != EventPassword)
                        {
                            await _dialogService.Alert("No Match", "Passcodes do not match. Please click save and try again.", "OK");
                            IsBusy = false;
                            return;
                        }
                        else
                        {
                            //all is well, continue
                        }

                    }
                    else
                    {
                        await _dialogService.Snackbar("You must supply a password confirmation");
                        IsBusy = false;
                        return;
                    }
                }


                if (ConnectivityService.IsConnected())
                {
                    var newEvent = new RaceEvent()
                    {
                        EventName = EventName.Trim(),
                        EventDate = EventDate,
                        EventType = EventType,
                        EventYear = EventDate.Year.ToString(),
                        EventPassCode = EventPassword,
                    };

                    var race = await DataService.PostRaceEventAsync(newEvent);
                    if (race == null)
                    {

                        await _dialogService.Alert("Create Race Event Failed",
                            "An error occured while creating the race event record. Please try again.",
                            "OK");
                    }
                    else
                    {
                        //show a success toast?

                        ShowLoading = false;

                        await _dialogService.Snackbar("Event Saved Successfully!");

                        var result = await _dialogService.Confirm($"Set as Default?", $"Would you like to set this event ({race.EventName}) as your default?", "YES", "NO");
                        if (result)
                        {
                            Preferences.Set("currenteventname", race.EventName);
                            Preferences.Set("currenteventcode", race.EventPassCode);

                            await _dialogService.Snackbar($"{race.EventName} set as Default!");

                            await _navigationService.GoBackAsync();
                        }
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

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            EventDate = DateTime.Now;

            base.OnNavigatedTo(parameters);
        }

       


        [Reactive] public string Property { get; set; }
    }
}
