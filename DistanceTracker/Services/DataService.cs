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

        public static Task<IEnumerable<LapRecord>> GetLapRecords(bool forceRefresh = true, string raceEvent = "") =>
            GetAsync<IEnumerable<LapRecord>>(Endpoints.LapRecords, Endpoints.Settings, forceRefresh: forceRefresh, raceEvent: raceEvent);

        public static Task<IEnumerable<Runner>> GetRunners(bool forceRefresh = true, string raceEvent = "") =>
            GetAsync<IEnumerable<Runner>>(Endpoints.Runners, Endpoints.Runners, forceRefresh: forceRefresh, raceEvent: raceEvent);

        public static Task<IEnumerable<RaceEvent>> GetRaces(bool forceRefresh = true) =>
            GetAsync<IEnumerable<RaceEvent>>(Endpoints.RaceEvents, Endpoints.RaceEvents, forceRefresh: forceRefresh);



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
                    Debug.WriteLine($"RACE EVENT: {raceEvent}");

                    url = $"{url}?code={Endpoints.code}";

      
                    if (!string.IsNullOrWhiteSpace(raceEvent))
                        url = $"{url}&raceeventname={raceEvent}";

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

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.AddRunner}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var savedCode = Preferences.Get("currenteventcode", string.Empty);
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

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.AddLapRecord}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var savedCode = Preferences.Get("currenteventcode", string.Empty);
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

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.AddRaceEvent}?code={Endpoints.code}";
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

        public static async Task<LapRecord> DeleteLapRecord(LapRecord _lap)
        {
            Debug.WriteLine("Deleting a lap record item...");
            LapRecord retrieved_result = null;
            var jsonObject = JsonConvert.SerializeObject(_lap);

            var url = $"{Endpoints.DistTrackURLBase}/{Endpoints.DeleteLap}?code={Endpoints.code}";
            Debug.WriteLine(url);

            var savedCode = Preferences.Get("currenteventcode", string.Empty);
            client.Authenticator = new HttpBasicAuthenticator("distancetrackerapp", savedCode);

            var restRequest = new RestRequest(url, Method.POST).AddJsonBody(_lap, "application/json");
            var response = await client.PostAsync<LapRecord>(restRequest);
            if (response != null)
            {
                if (response.Id != null)
                {
                    retrieved_result = response;
                }
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

    public static class Endpoints
    {
        //SS Conf
        public static string DistTrackURLBase = "https://distancetrackerfunctions2022.azurewebsites.net/api";

        public static string AddUser = "Post-CreateUser";
        public static string Users = "Get-Users";
        public static string GetUser = "Get-AUser";
    
        public static string AddLapRecord = "Post-LapRecord";
        public static string LapRecords = "Get-LapRecords";
        public static string DeleteLap = "DELETE-LapRecord";

        public static string AddRaceEvent = "Post-RaceEvent";
        public static string RaceEvents = "Get-RaceEvents";
                
        public static string AddRunner = "Post-Runner";
        public static string Runners = "Get-Runners";

        public static string Settings = "Get-Settings";

        public static string code = "pZ0O29mHHLRgXE8fMikc7qO130RyLWXw16BaAyZsYRuEAzFuNznU9A==";       
    }
}
