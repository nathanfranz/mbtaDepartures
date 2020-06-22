using MbtaApp.DL.Repositories;
using Xunit;

namespace MbtaApp.DL.UnitTests.Repositories
{
    public class MbtaAppRepoUnitTests
    {
        private static MbtaApiRepository _mbtaApiRepository = new MbtaApiRepository();

        [Fact]
        public void GetNorthStationQueryStringReturnsCorrectString()
        {
            var expectedString = "North%20Station,North%20Station-01,North%20Station-02,North%20Station-03,North%20Station-04,North%20Station-05," +
                                 "North%20Station-06,North%20Station-07,North%20Station-08,North%20Station-09,North%20Station-10,";

            var actualString = _mbtaApiRepository.GetNorthStationQueryString();
            
            Assert.Equal(expectedString, actualString);
        }
    }
}