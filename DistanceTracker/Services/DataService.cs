using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Serializers.NewtonsoftJson;
using MonkeyCache.SQLite;
using RestSharp.Authenticators;

namespace DistanceTracker
{
    public static class DataService
    {
        //If running a local service...?
        //static string Baseurl = DeviceInfo.Platform == DevicePlatform.Android ?
        //                                    "http://10.0.2.2:5000" : "http://localhost:5000";


        static RestClient client;
        static string BaseUrl = Endpoints.DistTrackURLBase;


        static DataService()
        {
            client = new RestClient(BaseUrl);
            client.DefaultParameters.Clear();

            client.UseNewtonsoftJson(new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Ignore,
            });
            client.ThrowOnAnyError = true;
            
        }

        public static Task<IEnumerable<LapRecord>> GetLapRecords(bool forceRefresh = true, string raceEventId = "") =>
            GetAsync<IEnumerable<LapRecord>>(Endpoints.LapRecords, Endpoints.Settings, forceRefresh: forceRefresh, raceEvent: raceEventId);

        public static Task<IEnumerable<Runner>> GetRunners(bool forceRefresh = true, string raceEventId = "") =>
            GetAsync<IEnumerable<Runner>>(Endpoints.Runners, Endpoints.Runners, forceRefresh: forceRefresh, raceEvent: raceEventId);

        public static Task<IEnumerable<RaceEvent>> GetRaces(bool forceRefresh = true) =>
            GetAsync<IEnumerable<RaceEvent>>(Endpoints.RaceEvents, Endpoints.RaceEvents, forceRefresh: forceRefresh);

        public static Task<RaceEvent> GetEvent(bool forceRefresh = true, string raceEventId = "") =>
            GetAsync<RaceEvent>(Endpoints.RaceEvent, Endpoints.RaceEvent, forceRefresh: forceRefresh, raceEvent: raceEventId);



        static async Task<T> GetAsync<T>(string url, string keybase, int mins = 20, bool forceRefresh = false, string raceEvent = "")
        {
            var json = string.Empty;
            var user = string.Empty;
            var pass = string.Empty;
            var dataRetrieved = string.Empty;
            T dataObject = default(T);

            try
            {
                //user = AppSettingsService.FirstName;
                //pass = await SecureStorage.GetAsync(Keys.PrivateKey) ?? string.Empty;

                //Determine if we should get results from cache
                if (!ConnectivityService.IsConnected())
                    json = Barrel.Current.Get<string>($"{keybase}");
                else if (!forceRefresh && !Barrel.Current.IsExpired($"{keybase}"))
                    json = Barrel.Current.Get<string>($"{keybase}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get data from cache {ex}");
            }


            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.WriteLine($"RACE EVENT ID: {raceEvent}");

                    url = $"{url}?code={Endpoints.code}";

      
                    if (!string.IsNullOrWhiteSpace(raceEvent))
                        url = $"{url}&raceeventid={raceEvent}";

                    Debug.WriteLine($"URL -- {url}");

                    var request = new RestRequest(url, DataFormat.Json);
                    dataObject = await client.GetAsync<T>(request);

                    try
                    {
                        //AddToBarrel(key: $"{keybase}",
                        //        data: JsonConvert.SerializeObject(dataObject),
                        //        expireIn: TimeSpan.FromMinutes(mins),
                        //        dataObject);
                    }
                    catch (Exception exx)
                    {
                    }

                    dataRetrieved = $"Retrieved {dataObject.GetType()} from SERVER";
                }
                else
                {
                    dataObject = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, });
                    dataRetrieved = $"Retrieved {dataObject.GetType()} from CACHE";
                }

                Debug.WriteLine($"{dataRetrieved}");
                return dataObject;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get information from server {ex}");
                throw ex;
            }
        }

        public static async Task<Runner> PostRunnerAsync(Runner _runner)
        {
            Debug.WriteLine("Creating a new Runner Item...");
            Runner retrieved_runner = null;
            var jsonObject = JsonConvert.SerializeObject(_runner);

            var id = Preferences.Default.Get(Keys.CurrentEventId, string.Empty);

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.AddRunner}/{id}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var savedCode = Preferences.Default.Get(Keys.CurrentEventCode, string.Empty);
            client.Authenticator = new HttpBasicAuthenticator("distancetrackerapp", savedCode);

            var restRequest = new RestRequest(url, Method.POST).AddJsonBody(_runner, "application/json");
            var response = await client.PostAsync<Runner>(restRequest);
            if (response != null)
            {
                if (response.Id != null)
                {
                    retrieved_runner = response;
                }                
            }

            return retrieved_runner;
        }

        public static async Task<LapRecord> PostLapRecordAsync(LapRecord _laprecord)
        {
            Debug.WriteLine("Creating a new Lap Record Item...");
            LapRecord retrieved_laprecord = null;
            var jsonObject = JsonConvert.SerializeObject(_laprecord);

            var id = Preferences.Default.Get(Keys.CurrentEventName, string.Empty);
            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.AddLapRecord}/{id}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var savedCode = Preferences.Default.Get(Keys.CurrentEventCode, string.Empty);
            client.Authenticator = new HttpBasicAuthenticator("distancetrackerapp", savedCode);

            var restRequest = new RestRequest(url, Method.POST).AddJsonBody(_laprecord, "application/json");
            var response = await client.PostAsync<LapRecord>(restRequest);
            if (response != null)
            {
                if (response.Id != null)
                {
                    retrieved_laprecord = response;
                }
            }

            return retrieved_laprecord;
        }

        public static async Task<RaceEvent> PostRaceEventAsync(RaceEvent _race)
        {
            Debug.WriteLine("Creating a new Race Event Item...");
            RaceEvent retrieved_race = null;
            var jsonObject = JsonConvert.SerializeObject(_race);

            var id = Preferences.Default.Get(Keys.CurrentEventId, string.Empty);

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.AddRaceEvent}/{id}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var restRequest = new RestRequest(url, Method.POST).AddJsonBody(_race, "application/json");
            var response = await client.PostAsync<RaceEvent>(restRequest);
            if (response != null)
            {
                if (response.Id != null)
                {
                    retrieved_race = response;
                }
            }

            return retrieved_race;
        }

        public static async Task<Object> DeleteLapRecord(LapRecord _lap)
        {
            Debug.WriteLine("Deleting a lap record item...");
            Object retrieved_result = null;
            var jsonObject = JsonConvert.SerializeObject(_lap);

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.DeleteLap}/{_lap.Id}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var savedCode = Preferences.Default.Get(Keys.CurrentEventCode, string.Empty);
            client.Authenticator = new HttpBasicAuthenticator("distancetrackerapp", savedCode);

            var restRequest = new RestRequest(url, Method.DELETE).AddJsonBody(_lap, "application/json");
            var response = await client.DeleteAsync<Object>(restRequest);  
            if (response != null)
            {
                retrieved_result = response;
            }

            return retrieved_result;
        }

        public static async Task<Object> DeleteRunner(Runner _runner)
        {
            Debug.WriteLine("Deleting a runner...");
            Object retrieved_result = null;
            var jsonObject = JsonConvert.SerializeObject(_runner);

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.DeleteRunner}/{_runner.Id}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var savedCode = Preferences.Default.Get(Keys.CurrentEventCode, string.Empty);
            client.Authenticator = new HttpBasicAuthenticator("distancetrackerapp", savedCode);

            var restRequest = new RestRequest(url, Method.DELETE).AddJsonBody(_runner, "application/json");
            var response = await client.DeleteAsync<Object>(restRequest);
            if (response != null)
            {
                retrieved_result = response;
            }

            return retrieved_result;
        }

        public static async Task<RaceEvent> PutEventTimeClock(string now)
        {
            Debug.WriteLine("Starting the event time clock...");
            RaceEvent retrieved_result = null;
            Object result_obj = null;

            //var nowObject = new { EventStartTimestamp = now };
            var nowTimeCode = new RaceEventTimeCode()
            {
                EventStartTimestamp = now,
            };
            var jsonObject = JsonConvert.SerializeObject(nowTimeCode);
            //var jsonObject = JsonConvert.SerializeObject(nowObject);

            var id = Preferences.Default.Get(Keys.CurrentEventId, string.Empty);

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.UpdateRaceEvent}/{id}?code={Endpoints.code}";
            Debug.WriteLine(url);
            //url = $"http://localhost:7071/api/Update-RaceEvent/{id}?code={Endpoints.code}";

            var savedCode = Preferences.Default.Get(Keys.CurrentEventCode, string.Empty);
            client.Authenticator = new HttpBasicAuthenticator("distancetrackerapp", savedCode);

            var restRequest = new RestRequest(url, Method.PUT).AddJsonBody(nowTimeCode, "application/json");
            var response = await client.PutAsync<RaceEvent>(restRequest);
            if (response != null)
            {
                if (response.EventName != null)
                    retrieved_result = response;
                //result_obj = response;
            }

            return retrieved_result;
        }

        public static void AddToBarrel(string key, string data, TimeSpan expireIn, object dataObject)
        {
            //if User type, and user type has square brackets (array)... remove them
            //Commenting this out until I can find a way to support the Enumerable AND single object
            // at the same time
            //if (dataObject is IEnumerable<User> ||
            //    dataObject is IEnumerable<Agency>)
            //{
            //    data = data.Trim('[');
            //    data = data.Trim(']');
            //}

            Barrel.Current.Add(key, data, expireIn);
        }

    }

    public class RaceEventTimeCode
    {
        public string EventStartTimestamp { get; set; } 
    }

    public static class Endpoints
    {
        //SS Conf
        public static string DistTrackURLBase = "https://distancetrackerfunctions2022.azurewebsites.net/api";

        public static string AddUser = "Post-CreateUser";
        public static string Users = "Get-Users";
        public static string GetUser = "Get-AUser";
    
        public static string AddLapRecord = "Post-LapRecord";
        public static string LapRecords = "Get-LapRecords";
        public static string DeleteLap = "Delete-LapRecord";

        public static string AddRaceEvent = "Post-RaceEvent";
        public static string RaceEvents = "Get-RaceEvents";
        public static string RaceEvent = "Get-RaceEvent";
        public static string UpdateRaceEvent = "Update-RaceEvent";
                
        public static string AddRunner = "Post-Runner";
        public static string Runners = "Get-Runners";
        public static string DeleteRunner = "Delete-Runner";

        public static string Settings = "Get-Settings";

        public static string code = "pZ0O29mHHLRgXE8fMikc7qO130RyLWXw16BaAyZsYRuEAzFuNznU9A==";       
    }
}
