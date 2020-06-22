# mbtaDepartures
Departure times for MBTA North Station Commuter rail

# How to run
1. Run the micro service (runs on http://localhost:5001 but can be changed in MbtaApp.Svc => Properties => launchSettings.json).
2. Open index.html in MbtaApp.Svc (if you change where the micro service is running it must also be change here).

# Design Summary
As a backend engineer I wanted to demonstrate how I develop micro services so I created this micro service to make all the mbta API calls 
and manipulate the data. With limited frontend experience I just created a simple html file to display the data. The data displayed is for North Station
and is the same data shown here: https://commons.wikimedia.org/wiki/File:North_Station_departure_board,_December_2011.jpg (with the exception of
the train number as I couldn't find it within the mbta APIs).

My service returns all the departure data (except for past times) and since my frontend experience is limited the html page displays all of the data.
The frontend consumer may want this route to return all the data or the route could be updated to include paging query paramters so they could retrieve
the data in chunks.

Unit and integration tests were kept simple and only tested that everything worked as expected. Most edge cases were not tested.

# General Overview Of Service Functionality
1. All schedule data for North Station is retrieved
2. Schedule data is trimmed to only include departures
3. All prediction data for North Station is retrieved
4. Look for schedule and prediction that have matching tripIds (only id that is unique and the same for a prediction and schedule)
5. If tripIds don't match then use schedule data for returned departure data
6. If tripIds match then use prediction data for returned departure data (prediction data is assumed to be the most up to date)
7. Use destinationId and routeId on schedule/prediction data to retrieve destination and add it to returned departure data
8. Order all departure data by time with TBD times at the end
9. Return all departure data

# Assumptions Made (Usually cleared up by Project Managers)

* All schedules' ScheduleResource attributes have either an arrival or departure time. 
Checked with:
    response.Data.data
        .Where(x => x.attributes.departure_time == null && x.attributes.arrival_time == null);
* Trip ids are unique. 
Checked with:
    response.Data.data
        .GroupBy(x => x.relationships.trip.data.id)
        .Where(g => g.Count() > 1)
        .Select(y => y.Key);
* Unique schedules and predictions can only appear on one track. i.e. schedule1 cannot be in "North Station" and "North Station-01"
* If a schedule does not also have a prediction it can be assumed "On Time"
* If a schedule has a prediction that is not "On Time" and it has null departure time then departure time becomes TBD
