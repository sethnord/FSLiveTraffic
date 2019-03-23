using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Xml;

namespace FSLiveTraffic
{
    class AirTraffic
    {
        //This class will handle communications with the opensky tracking site, and return an array of all a/c's within
        //a given area.

        static string _baseRequest = "https://opensky-network.org/api/states/all?";
        static Timer timer;

        public static Aircraft[] Get(double playerLat, double playerLng, int radius)
        {
            /*This function will return a variable length string array including the following:
             * 
             *0: ICAO address
             *1: Callsign- may be null
             *2: Country of Origin
             *3: Unix timestamp of last position update, may be null if no response within 15 seconds.
             *4: Unix timestamp of last update of any sort. (Any valid message from Xponder.)
             *5: Longitude in Decimal Degrees- May be null
             *6: Latitude in Decimal Degrees- May be null
             *7: Barometric altitude in meters- can be null
             *8: Boolean value indicating weither the ac is on the ground
             *9: Groundspeed in m/s- can be null
             *10: True heading in degrees- Can be null
             *11: Vertical speed in m/s- can be null
             *12: Integer array which contributed to state vector- Can be null
             *13: Geometric? altitude in meters- can be null
             *14: Sqwawk code- Can be null
             *15: Special purpose indicator
             *16: Int representing origin of the position-
             *0=ADS-B, 1=ASTERIX, 2=MLAT
             */

            //Firstly, we need to formulate a request, given player location and radius.
            //The boundaries actually define a square, and not a cirle, so it is really NM^2
            //Assuming the player is the dead center of the square, we need to divide radius by 2, then
            //calculate player pos +- r/2 for both latitude and longitude.

            //Our maximum radius is in nautical miles, so we need to convert that to decimal degrees.
            //We need to calculate the length of 1 degree longitude because spheres...
            //The formula fo this is as follows:
            //dLng[sm] = cos(Lat)*69.172
            //Unlike longitude, latitude is constant enough for us to use, about 68.703 sm
            //In case you're wondering, yes, the earth is round.
            //50 lines of comments... I really need to tone this down.

            //Calculate nm per degrees given current position.

            //timer.Interval = 10000; //10 Seconds

            double milesPerDegreeLat = 68.703;
            double milesPerDegreeLng = NauticalMilesPerDegree(playerLng);
            double squareRtRadius = Math.Sqrt(radius);
            double lngDegrees = squareRtRadius / milesPerDegreeLat;
            double latDegrees = squareRtRadius / milesPerDegreeLng;

            //These are the bounds that define our square
            double lowerLng = playerLng - lngDegrees;
            double upperLat = playerLat - latDegrees;
            double upperLng = playerLng + lngDegrees;
            double lowerLat = playerLat + latDegrees;

            //https://opensky-network.org/apidoc/rest.html#limitations

            //Now that we have the upper and lower bounds of the square, we can formulate a request.
            string finalRequest = _baseRequest + "lamin=" + lowerLat + "&lomin=" + lowerLng + "&lamax=" + upperLat + "&lomax=" + upperLng;

            //INFO: We are limited to one request per 10 seconds
            //The website doesn't block us if we refresh the page, it just doesn't give us new data, so not a huge issue
            //I might add an option where someone could open their own account and use that for faster data...
            //Per the spec, the protocol requires that we send ac position every 8-9 seconds... 10 seconds probably won't hurt.

            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(finalRequest);

                var result = JsonConvert.DeserializeObject<AircraftList>(json);

                //We need an aircraft array to hold all of the aircraft we are going to parse.
                Aircraft[] aircraft = new Aircraft[Form1._maxAC];

                    for (int i = 0; i < Form1._maxAC ; i++) //This loop is for each individual aircraft.
                    {
                        //Create an aircraft object to hold this data.
                        Aircraft thisPlane = new Aircraft();

                    string testVal;
                    try
                    {
                        testVal = result.states[i][0];
                    }
                    catch(IndexOutOfRangeException)
                    {
                        break;
                    }
                    if (testVal == null)
                    {
                        break;
                    }
                        thisPlane.icao = result.states[i][0];
                        thisPlane.callsign = result.states[i][1];
                        thisPlane.origin_country = result.states[i][2];
                        thisPlane.time_position = result.states[i][3];
                        thisPlane.last_contact = result.states[i][4];
                        thisPlane.longitude = result.states[i][5];
                        thisPlane.latitude = result.states[i][6];
                        thisPlane.baro_altitude = result.states[i][7];
                        thisPlane.on_ground = result.states[i][8];
                        thisPlane.velocity = result.states[i][9];
                        thisPlane.true_track = result.states[i][10];
                        thisPlane.vertical_rate = result.states[i][11];
                        //12 was omitted because it serves no purpose in life.
                        thisPlane.geo_altitude = result.states[i][13];
                        thisPlane.sqwawk = result.states[i][14];
                        thisPlane.spi = result.states[i][15];
                        thisPlane.position_source = result.states[i][16];

                        aircraft[i] = thisPlane;
                    }

                return aircraft;
            }

        }

        public static double NauticalMilesPerDegree(double longitude)
        {
            double smPerDegree = Math.Cos(longitude) * 69.172;
            double toReturn = smPerDegree / 1.151; //Convert to NM
            return toReturn;
        }
    }
}
