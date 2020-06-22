using System.Collections.Generic;
using System.Threading.Tasks;
using MbtaApp.Models;

namespace MbtaApp.DL.Repositories
{
    public interface IMbtaApiRepository
    {
        Task<List<ScheduleResource>> GetNorthStationSchedules();

        Task<List<PredictionResource>> GetNorthStationPredictions();

        Task<HashSet<RouteResource>> GetAllCommuterRailRoutes();
    }
}