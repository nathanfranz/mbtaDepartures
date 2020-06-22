using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MbtaApp.DL.Repositories;
using MbtaApp.Models;

namespace MbtaApp.BL.Managers
{
    public class MbtaAppManager
    {
        private const string ON_TIME_STATUS = "On time";

        private readonly IMbtaApiRepository _mbtaApiRepository;
        
        public MbtaAppManager(IMbtaApiRepository mbtaApiRepository)
        {
            _mbtaApiRepository = mbtaApiRepository;
        }
        
        public MbtaAppManager()
        {
            _mbtaApiRepository = new MbtaApiRepository();
        }

        public async Task<List<DepartureResponse>> GetDepartureData()
        {
            var departureDataList = new List<DepartureResponse>();

            var schedules = await GetTrimmedSchedules();
            var predictions = await _mbtaApiRepository.GetNorthStationPredictions();
            var routes = await _mbtaApiRepository.GetAllCommuterRailRoutes();

            foreach (var schedule in schedules)
            {
                // If on schedule then status will be "On Time"
                var departureData = 
                    new DepartureResponse(
                        schedule.attributes.departure_time, 
                        ConvertStopIdToTrackNumber(schedule.relationships.stop.data.id), 
                        ON_TIME_STATUS,
                        schedule.attributes.direction_id);
                
                UpdateIfPrediction(departureData, schedule, predictions.ToHashSet());

                departureData.Destination = GetDestination(schedule.relationships.route.data.id,
                    departureData.DirectionId, routes);

                departureDataList.Add(departureData);
            }

            // Remove TBD times so list can be ordered and then re-add them
            var tbdTimes = departureDataList.Where(x => x.DepartureTime == "TBD").ToList();
            departureDataList.RemoveAll(x => x.DepartureTime == "TBD");
            var orderedDepartureTimes = departureDataList.OrderBy(x => DateTime.Parse(x.DepartureTime)).ToList();
            orderedDepartureTimes.AddRange(tbdTimes);

            return orderedDepartureTimes;
        }

        // Assumed predictions are up to date and therefore overwrite schedule's data
        public void UpdateIfPrediction(DepartureResponse departureData, 
            ScheduleResource schedule,
            HashSet<PredictionResource> predictions)
        {
            // In schedules and predictions it looked like the tripId was the only unique id in both (should only be 1 returned)
            var scheduleTripId = schedule.relationships.trip.data.id;
            var prediction = predictions.FirstOrDefault(x => x.relationships.trip.data.id == scheduleTripId);

            if (prediction == null) return;
            
            // If train is not on time and has null departure time then departure time is set to "TBD" in the model
            if (!string.IsNullOrEmpty(prediction.attributes.status) && prediction.attributes.status != ON_TIME_STATUS && prediction.attributes.departure_time == null)
            {
                departureData.DepartureTime = "TBD";
            }
            else if (prediction.attributes.departure_time != null)
            {
                departureData.DepartureTime = prediction.attributes.departure_time;
            }
                
            if (!string.IsNullOrEmpty(prediction.attributes.status))
            {
                departureData.Status = prediction.attributes.status;
            }
            
            departureData.TrackNumber = ConvertStopIdToTrackNumber(prediction.relationships.stop.data.id);

            departureData.DirectionId = prediction.attributes.direction_id;
        }
        
        public string GetDestination(string routeId, int directionId, HashSet<RouteResource> routes)
        {
            var route = routes.FirstOrDefault(x => x.id == routeId);

            return route != null && !string.IsNullOrEmpty(route.attributes.direction_destinations[directionId]) 
                ? route.attributes.direction_destinations[directionId] : "TBD";
        }

        public async Task<HashSet<ScheduleResource>> GetTrimmedSchedules()
        {
            var schedulesForTrack = await _mbtaApiRepository.GetNorthStationSchedules();

            // Only schedules that have departure times greater than current time
            var trimmedSchedules = schedulesForTrack
                .Where(x => !string.IsNullOrEmpty(x.attributes.departure_time))
                .Where(x => Convert.ToDateTime(x.attributes.departure_time) > DateTime.Now).ToList();

            // Order of schedules doesn't matter and this will be more efficient for lookups
            return trimmedSchedules.ToHashSet();
        }

        public string ConvertStopIdToTrackNumber(string stopId)
        {
            if (stopId == null || !stopId.Contains("-"))
            {
                return "TBD";
            }

            var trackNumberString = stopId.Substring(stopId.LastIndexOf('-') + 1);

            // If track number < 10 remove the preceding 0
            if (trackNumberString.IndexOf("0", StringComparison.Ordinal) == 0)
            {
                trackNumberString = trackNumberString.Substring(1);
            }

            return trackNumberString;
        }
    }
}