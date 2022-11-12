﻿using System;
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

namespace DistanceTracker
{
    public static class DataService
    {
        //If running a local service...?
        //static string Baseurl = DeviceInfo.Platform == DevicePlatform.Android ?
        //                                    "http://10.0.2.2:5000" : "http://localhost:5000";


        static RestClient client;
        static string BaseUrl = Endpoints.SSConfBaseURL;


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
            GetAsync<IEnumerable<RaceEvent>>(Endpoints.RaceEvents, Endpoints.RaceEvents, forceRefresh: forceRefresh, raceEvent: raceEvent);



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
                    json = Barrel.Current.Get<string>($"{user}-{keybase}");
                else if (!forceRefresh && !Barrel.Current.IsExpired($"{user}-{keybase}"))
                    json = Barrel.Current.Get<string>($"{user}-{keybase}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get data from cache {ex}");
            }


            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.WriteLine($"CONF KEY {raceEvent}");

                    url = $"{url}?code={Endpoints.code}";
                    if (!string.IsNullOrWhiteSpace(raceEvent))
                        url = $"{url}&conference={raceEvent}";
                    Debug.WriteLine($"URL -- {url}");

                    var request = new RestRequest(url, DataFormat.Json);
                    dataObject = await client.GetAsync<T>(request);

                    AddToBarrel(key: $"{user}-{keybase}",
                                data: JsonConvert.SerializeObject(dataObject),
                                expireIn: TimeSpan.FromMinutes(mins),
                                dataObject);

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

            var url = $"{Endpoints.SSConfBaseURL}/{Endpoints.AddRunner}?code={Endpoints.code}";
            Debug.WriteLine(url);

            //another way of sending the request
            //var restRequest = new RestRequest(url, Method.POST);
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("application/json", jsonObject, ParameterType.RequestBody);
            //var response = await client.ExecuteAsync(restRequest);

            var restRequest = new RestRequest(url, Method.POST).AddJsonBody(_runner, "application/json");
            var response = await client.PostAsync<Runner>(restRequest);
            if (response != null)
            {
                retrieved_runner = response;
            }

            return retrieved_runner;
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
        public static string SSConfBaseURL = "https://conferencefunctions20210624142237.azurewebsites.net/api";

        public static string AddUser = "Post-CreateUser";
        public static string Users = "Get-Users";
        public static string GetUser = "Get-AUser";
    
        public static string AddLapRecord = "Post-LapRecord";
        public static string LapRecords = "Get-LapRecords";

        public static string AddRaceEvent = "Post-RaceEvent";
        public static string RaceEvents = "Get-RaceEvents";
                
        public static string AddRunner = "Post-Runner";
        public static string Runners = "Get-Runners";

        public static string Settings = "Get-Settings";

        public static string code = "oEJW9mvQNtIASHcjGBSCpbEpQs84d9k8ANgov5hAgWMTX2Q5T0NaFQ==";       
    }
}
