using System.Collections.Generic;
using System.Threading.Tasks;
using MbtaApp.BL.Managers;
using MbtaApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace MbtaApp.Controllers
{
    [Route("mbta")]
    [Produces("application/json")]
    public class MbtaController : Controller
    {
        /// <summary>
        /// Gets the departure board data for all tracks. Takes long to run to avoid mbta timeout error.
        /// </summary>
        /// <returns>The departure board data.</returns>
        /// <response code="200">Successfully retrieved the departure board data.</response>
        [HttpGet("departureData")]
        public async Task<List<DepartureResponse>> GetDepartureData()
        {
            var mbtaAppManager = new MbtaAppManager();
            
            var departureData = await mbtaAppManager.GetDepartureData();

            return departureData;
        }
    }
}