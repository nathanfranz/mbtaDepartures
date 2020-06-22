using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MbtaApp.BL.Managers;
using MbtaApp.DL.Repositories;
using MbtaApp.Models;
using Moq;
using Xunit;

namespace MbtaApp.BL.UnitTests.Managers
{
    public class MbtaAppManagerUnitTests
    {
        private const string ON_TIME_STATUS = "On time";
        
        private readonly MbtaAppManager _mbtaAppManager;
        
        private readonly Mock<IMbtaApiRepository> _mbtaAppRepositoryMock;

        public MbtaAppManagerUnitTests()
        {
            _mbtaAppRepositoryMock = new Mock<IMbtaApiRepository>();
            _mbtaAppManager = new MbtaAppManager(_mbtaAppRepositoryMock.Object);
            
            _mbtaAppRepositoryMock
                .Setup(x => x.GetNorthStationSchedules())
                .ReturnsAsync(CreateSchedules());
            
            _mbtaAppRepositoryMock
                .Setup(x => x.GetNorthStationPredictions())
                .ReturnsAsync(CreatePredictions());
            
            _mbtaAppRepositoryMock
                .Setup(x => x.GetAllCommuterRailRoutes())
                .ReturnsAsync(CreateRoutes());
        }
        
        [Fact]
        public async Task GetTrimmedSchedulesFiltersDates()
        {
            // In CreateSchedules there is one null departure time and one less than current time departure time.
            // So the expected result should have 2 less than the total count.
            var expectedTotalScheduleCount = CreateSchedules().Count - 2;

            var schedules = await _mbtaAppManager.GetTrimmedSchedules();
            
            Assert.Equal(expectedTotalScheduleCount, schedules.Count);
        }

        [Fact]
        public async Task GetDestinationReturnsCorrectDestination()
        {
            var destination = _mbtaAppManager.GetDestination("testRouteId", 1, CreateRoutes());
            
            Assert.Equal("testDestination1", destination);
        }
        
        [Fact]
        public async Task UpdateIfPredictionUpdates()
        {
            var schedules = CreateSchedules();
            var scheduleToChange = schedules[2];

            var predictions = CreatePredictions();
            var prediction = predictions[1];
            
            var departureData = new DepartureResponse(
                scheduleToChange.attributes.departure_time, 
                scheduleToChange.trackNumber, 
                ON_TIME_STATUS, 
                scheduleToChange.attributes.direction_id);
            
            _mbtaAppManager.UpdateIfPrediction(departureData, scheduleToChange, predictions.ToHashSet());
            
            Assert.NotEqual(DateTime.Parse(prediction.attributes.departure_time), DateTime.Parse(scheduleToChange.attributes.departure_time));
            
            Assert.Equal(prediction.attributes.status.ToUpper(), departureData.Status);
            Assert.NotEqual(ON_TIME_STATUS, prediction.attributes.status);
            
            Assert.NotEqual(prediction.attributes.direction_id, scheduleToChange.attributes.direction_id);
        }

        [Fact]
        public async Task GetDepartureDataDepartTimeTBDAtEnd()
        {
            var departureData = await _mbtaAppManager.GetDepartureData();
            
            Assert.Equal("TBD",departureData.Last().DepartureTime);
        }
        
        [Theory]
        [InlineData(null, "TBD")]
        [InlineData("", "TBD")]
        [InlineData("North Station-01", "1")]
        [InlineData("North Station-05", "5")]
        [InlineData("North Station-09", "9")]
        [InlineData("North Station-10", "10")]
        public async Task ConvertStopIdToTrackNumberWorks(string stopId, string expectedTrackNumber)
        {
            var returnedTrackNumber = _mbtaAppManager.ConvertStopIdToTrackNumber(stopId);
            
            Assert.Equal(expectedTrackNumber, returnedTrackNumber);
        }

        // Test Models
        private List<ScheduleResource> CreateSchedules()
        {
            return new List<ScheduleResource>()
            {
                // Schedule with no departure time to test that it gets filtered
                new ScheduleResource()
                {
                    type = "schedule",
                    id = "schedule-CR-Weekday-StormB-19-1100C0-North Station",
                    attributes = new ScheduleAttributes()
                    {
                        arrival_time = "2020-06-18T08:12:00-04:00",
                        departure_time = null,
                        direction_id = 1
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "CR-Weekday-StormB-19-1100C0"
                            }
                        }
                    }
                },

                // Schedule with departure time less than current time to test that it gets filtered
                new ScheduleResource()
                {
                    type = "schedule",
                    id = "schedule-CR-Weekday-StormB-19-1100C0-North Station",
                    attributes = new ScheduleAttributes()
                    {
                        arrival_time = null,
                        departure_time = DateTime.MinValue.ToString(),
                        direction_id = 1
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station-10",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "CR-Weekday-StormB-19-1100C0"
                            }
                        }
                    }
                },
                
                new ScheduleResource()
                {
                    type = "schedule",
                    id = "schedule-CR-Weekday-StormB-19-1100C0-North Station",
                    attributes = new ScheduleAttributes()
                    {
                        arrival_time = null,
                        departure_time = DateTime.MaxValue.ToString(),
                        direction_id = 1
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station-01",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "MatchingTripId1"
                            }
                        }
                    }
                },
                
                new ScheduleResource()
                {
                    type = "schedule",
                    id = "schedule-CR-Weekday-StormB-19-1100C0-North Station",
                    attributes = new ScheduleAttributes()
                    {
                        arrival_time = null,
                        departure_time = DateTime.MaxValue.ToString(),
                        direction_id = 1
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "MatchingTripId2"
                            }
                        }
                    }
                },
                
                new ScheduleResource()
                {
                    type = "schedule",
                    id = "schedule-CR-Weekday-StormB-19-1100C0-North Station",
                    attributes = new ScheduleAttributes()
                    {
                        arrival_time = null,
                        departure_time = DateTime.MaxValue.ToString(),
                        direction_id = 1
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station-02",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "CR-Weekday-StormB-19-1100C0"
                            }
                        }
                    }
                }
            };
        }
        
        private List<PredictionResource> CreatePredictions()
        {
            return new List<PredictionResource>()
            {
                new PredictionResource()
                {
                    id = "prediction-CR-Weekday-StormB-19-1104C0-North Station",
                    attributes = new PredictionAttributes()
                    {
                        departure_time = "2020-06-18T08:12:00-04:00",
                        direction_id = 0,
                        status = "Delayed"
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station-09",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "CR-Weekday-StormB-19-1100C0"
                            }
                        }
                    }
                },
                new PredictionResource()
                {
                    id = "prediction-CR-Weekday-StormB-19-1104C0-North Station",
                    attributes = new PredictionAttributes()
                    {
                        departure_time = "2020-06-18T08:12:00-04:00",
                        direction_id = 0,
                        status = "Delayed"
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station-10",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "MatchingTripId1"
                            }
                        }
                    }
                },
                new PredictionResource()
                {
                    id = "prediction-CR-Weekday-StormB-19-1104C0-North Station",
                    attributes = new PredictionAttributes()
                    {
                        departure_time = null,
                        direction_id = 0,
                        status = "Delayed"
                    },
                    relationships = new Relationships()
                    {
                        route = new Route()
                        {
                            data = new Data()
                            {
                                id = "testRouteId",
                                type = "route"
                            }
                        },
                        stop = new Stop()
                        {
                            data = new Data()
                            {
                                id = "North Station-03",
                            }
                        },
                        trip = new Trip()
                        {
                            data = new Data()
                            {
                                id = "MatchingTripId2"
                            }
                        }
                    }
                }
            };
        }

        public HashSet<RouteResource> CreateRoutes()
        {
            return new HashSet<RouteResource>()
            {
                new RouteResource()
                {
                    id = "testRouteId",
                    attributes = new RouteAttributes()
                    {
                        direction_destinations = new List<string>()
                        {
                            "testDestination0",
                            "testDestination1"
                        }
                    }
                }
            };
        }
    }
}