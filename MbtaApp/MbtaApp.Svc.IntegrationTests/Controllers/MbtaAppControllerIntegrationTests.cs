using System.Linq;
using System.Threading.Tasks;
using MbtaApp.Controllers;
using Xunit;

namespace MbtaApp.Svc.IntegrationTests
{
    public class MbtaAppIntegrationTests
    {
        private readonly MbtaController _mbtaController = new MbtaController();

        [Fact]
        public async Task GetDepartureDataAllTracksEndToEnd()
        {
            var departureData = await _mbtaController.GetDepartureData();
            
            Assert.NotEmpty(departureData);

            var departure = departureData.First();
            
            Assert.NotNull(departure.Carrier);
            Assert.NotNull(departure.Destination);
            Assert.NotNull(departure.Status);
            Assert.NotNull(departure.DepartureTime);
            Assert.NotNull(departure.TrackNumber);
        }
    }
}