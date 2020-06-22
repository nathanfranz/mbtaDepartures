using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MbtaApp.Models;
using RestSharp;

namespace MbtaApp.DL.Repositories
{
    public class MbtaApiRepository : IMbtaApiRepository
    {
        // Initialize rest client
        private const string BaseAddress = "https://api-v3.mbta.com";
        private IRestClient _mbtaClient = new RestClient(BaseAddress);
        
        // Routes
        private const string SchedulesRoute = "/schedules";
        private const string PredictionsRoute = "/predictions";
        private const string RoutesRoute = "/routes";

        // Filters (Query Parameters)
        private const string StopFilter = "filter[stop]=";
        private const string TypeFilter = "filter[type]=";

        // Ids
        private const string NorthStationStopId = "North%20Station";
        private const string CommuterRailId = "2";
        
        // Misc
        private const int NumberOfTracks = 10;

        public async Task<List<ScheduleResource>> GetNorthStationSchedules()
        {
            var uri = SchedulesRoute + "?" + StopFilter + GetNorthStationQueryString();
            var userRequest = new RestRequest(uri, Method.GET);

            var response = await _mbtaClient.ExecuteAsync<Schedules>(userRequest);
            CheckTooManyRequests(response);
            return response.Data.data ?? new List<ScheduleResource>();
        }
        
        public async Task<List<PredictionResource>> GetNorthStationPredictions()
        {
            var uri = PredictionsRoute + "?" + StopFilter + GetNorthStationQueryString();
            var userRequest = new RestRequest(uri, Method.GET);

            var response = await _mbtaClient.ExecuteAsync<Predictions>(userRequest);
            CheckTooManyRequests(response);
            return response.Data.data ?? new List<PredictionResource>();
        }
        
        public async Task<HashSet<RouteResource>> GetAllCommuterRailRoutes()
        {
            var uri = RoutesRoute + "?" + TypeFilter + CommuterRailId;
            var userRequest = new RestRequest(uri, Method.GET);

            var response = await _mbtaClient.ExecuteAsync<Routes>(userRequest);
            CheckTooManyRequests(response);
            return response.Data.data == null ? new HashSet<RouteResource>() : response.Data.data.ToHashSet();
        }

        // North station track stop ids are in the format "North%20Station-**" where "**" is the track number
        // This constructs the query string for all tracks
        public string GetNorthStationQueryString()
        {
            var queryString = NorthStationStopId + ",";
            
            for(var i = 1; i <= NumberOfTracks; i++)
            {
                if (i < 10)
                {
                    queryString = queryString + NorthStationStopId + "-0" + i + ",";
                }
                else
                {
                    queryString = queryString + NorthStationStopId + "-" + i + ",";
                }
            }

            return queryString;
        }

        // Mbta API will throw an error if you hit it too much
        // This is not tested as it breaks the other tests since they will be affected by producing this error
        private void CheckTooManyRequests(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new Exception("The Mbta API has rate limited the amount of requests this service can make.");
            }
        }
    }
}