using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace FSLiveTraffic
{
    class WxManager
    {
        static string _wxserverAddr = "https://www.aviationweather.gov/adds/dataserver_current/";

        //Handles communication with the weather server.
        public static string[] GetMetar(double lat, double lng, int radius)
        {
            string[] placeholder = new string[6];

            //Placeholder indexes:
            //0- Station ID
            //1- Latitude
            //2- Longitude
            //3- Wind Direction
            //4- Wind Speed
            //5- Altimeter (inHg)

            //Formulate the request:
            string request = "httpparam?dataSource=metars&requestType=retrieve&format=xml&hoursBeforeNow=1&mostRecent=true";
            //Now add in lat/long info and radius.
            string variable = "&radialDistance=" + radius + ";" + lng + "," + lat;
            //Combine the two pieces
            string toSend = _wxserverAddr + request + variable;

            //Now, grab it from the server.
            XmlDocument rawXML = new XmlDocument();

            rawXML.Load(toSend);

            rawXML.Save("temp.xml");


            //Now that we have the XML file, go ahead and extract the first metar entry

            //If we aren't connected to the location ...API then we wont be getting a response.
            if (Form1._TcpConnected)
            {
                XmlDocument doc = new XmlDocument();

                doc.Load("temp.xml");

                XmlNode node = doc.DocumentElement.SelectSingleNode("/response/data");
                if(node.Attributes["num_results"].Value != "0")
                {
                    node = doc.DocumentElement.SelectSingleNode("/response/data/METAR/station_id");
                    placeholder[0] = node.InnerText;

                    node = rawXML.DocumentElement.SelectSingleNode("/response/data/METAR/latitude");
                    placeholder[1] = node.InnerText;

                    node = rawXML.DocumentElement.SelectSingleNode("/response/data/METAR/longitude");
                    placeholder[2] = node.InnerText;

                    node = rawXML.DocumentElement.SelectSingleNode("/response/data/METAR/wind_dir_degrees");
                    placeholder[3] = node.InnerText;

                    node = rawXML.DocumentElement.SelectSingleNode("/response/data/METAR/wind_speed_kt");
                    placeholder[4] = node.InnerText;

                    node = rawXML.DocumentElement.SelectSingleNode("/response/data/METAR/altim_in_hg");
                    placeholder[5] = node.InnerText;
                }
                else
                {
                    placeholder[0] = null;
                    placeholder[1] = null;
                    placeholder[2] = null;
                    placeholder[3] = null;
                    placeholder[4] = null;
                    placeholder[5] = null;
                    //Set everything to null so we know not to update this cycle.
                }
            }
            else
            {
                placeholder[0] = null;
                placeholder[1] = null;
                placeholder[2] = null;
                placeholder[3] = null;
                placeholder[4] = null;
                placeholder[5] = null;
            }
            

            return placeholder;
            //Gets the first METAR that is sent by the server.
        }

        public static bool TestConnection()
        {
            bool success = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://aviationweather.gov");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    success = true;
                }
            }
            catch (WebException)
            {
                
            }
            return success;
        }
    }
}
