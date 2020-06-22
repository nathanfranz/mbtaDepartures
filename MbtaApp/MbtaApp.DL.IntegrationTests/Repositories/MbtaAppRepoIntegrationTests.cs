using System.Linq;
using System.Threading.Tasks;
using MbtaApp.DL.Repositories;
using Xunit;

namespace MbtaApp.DL.IntegrationTests.Repositories
{
    public class MbtaAppRepoIntegrationTests
    {
        private static MbtaApiRepository _mbtaApiRepository = new MbtaApiRepository();

        [Fact]
        public async Task GetNorthStationSchedulesReturnsCorrectData()
        {
            var schedules = await _mbtaApiRepository.GetNorthStationSchedules();

            var schedule = schedules.First(x => x.attributes.departure_time != null);
            
            Assert.NotNull(schedule);
            Assert.NotNull(schedule.attributes.departure_time);
            Assert.NotNull(schedule.relationships.route.data.id);
            Assert.NotNull(schedule.relationships.trip.data.id);
            
            Assert.IsType<System.Int32>(schedule.attributes.direction_id);
        }
        
        [Fact]
        public async Task GetNorthStationPredictionsReturnsCorrectData()
        {
            var predictions = await _mbtaApiRepository.GetNorthStationPredictions();

            var predictionsDepartureTime = predictions.Count(x => x.attributes.departure_time == null || x.attributes.departure_time != null);
            Assert.True(predictionsDepartureTime > 0);
            
            var predictionsStatus = predictions.Count(x => x.attributes.status == null || x.attributes.status != null);
            Assert.True(predictionsStatus > 0);
            
            var predictionsTripId = predictions.Count(x => x.relationships.trip.data.id == null || x.relationships.trip.data.id != null);
            Assert.True(predictionsTripId > 0);

            Assert.IsType<System.Int32>(predictions.First().attributes.direction_id);
        }
        
        [Fact]
        public async Task GetAllCommuterRailRoutes()
        {
            var routes = await _mbtaApiRepository.GetAllCommuterRailRoutes();
            Assert.NotEmpty(routes);
        }
    }
}