using Newtonsoft.Json;

namespace DistanceTracker;

public partial class AddRunnerPage : ContentPage
{
    AddRunnerPageViewModel _vm;
    public AddRunnerPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        _vm = GetViewModelInstance();
        base.OnAppearing();
    }

    private AddRunnerPageViewModel GetViewModelInstance()
    {
        return BindingContext as AddRunnerPageViewModel;
    }

    private void SaveRunner_Clicked(object sender, EventArgs e)
    {

    }

    private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        var birthdate = BirthDatePicker.Date;

        // Save today's date.
        var today = DateTime.Today;

        // Calculate the age.
        var age = today.Year - birthdate.Year;

        // Go back to the year in which the person was born in case of a leap year
        if (birthdate.Date > today.AddYears(-age)) age--;

        AgeEntry.Text = age.ToString();
    }

    async void ImportRunnersButton_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            var res = string.Empty;

            //FirstName - as string
            //LastName - as string  
            //BibNumber - as string
            //Sex - as string (M/F)
            //BirthDate
            //TeamName
            string jsonSample = @"[
                                          {
                                            ""FirstName"": ""John"",
                                            ""LastName"": ""Doe"",
                                            ""BibNumber"": ""101"",
                                            ""Sex"": ""M"",
                                            ""BirthDate"": ""4/1/1995"",
                                            ""TeamName"": ""Too Legit""
                                          }
                                      ]";
            res = await this.DisplayPromptAsync($"Paste Runner Import JSON",
                                                $"The JSON should be formatted as follows: \n\n {jsonSample}",
                                                "OK",
                                                "Cancel",
                                                keyboard: Keyboard.Default,
                                                initialValue: string.Empty);



            if (!string.IsNullOrWhiteSpace(res))
            {
                string result = res;
                List<Runner> RunnersToImport = new List<Runner>();
                RunnersToImport = JsonConvert.DeserializeObject<List<Runner>>(result);

                _vm.SaveImportedRunners(RunnersToImport);
            }
            System.Diagnostics.Debug.WriteLine(res);
        }
        catch (Exception ex)
        {
        }
    }
}