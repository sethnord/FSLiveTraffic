using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSLiveTraffic
{
    class Aircraft
    {
        //This class represents each individual aircraft.
        public string icao { get; set; }
        public string callsign { get; set; }
        public string origin_country { get; set; }
        public string time_position { get; set; }
        public string last_contact { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public string baro_altitude { get; set; }
        public string on_ground { get; set; }
        public string velocity { get; set; }
        public string true_track { get; set; }
        public string vertical_rate { get; set; }
        public string geo_altitude { get; set; }
        public string sqwawk { get; set; }
        public string spi { get; set; }
        public string position_source { get; set; }

        public string[] ToArray()
        {
            //Does not return int[] sensors, there is no good way to send that as part of this array.
            string[] result = { icao,callsign,origin_country,time_position.ToString(),last_contact.ToString()
                    ,longitude.ToString(),latitude.ToString(),baro_altitude.ToString(),on_ground.ToString(),
                velocity.ToString(),true_track.ToString(),vertical_rate.ToString(),geo_altitude.ToString(),
                sqwawk,spi.ToString(),position_source.ToString() };

            return result;
        }
    }
}
