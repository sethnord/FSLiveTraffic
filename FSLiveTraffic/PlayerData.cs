using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace FSLiveTraffic
{
    class PlayerData
    {
        static NetworkStream stream;
        public static Double playerLat = 0.0;
        public static Double playerLng = 0.0;
        public static Double playerHdg = 0.0;

        public static bool stop = false;

        public static void StartConnection()
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect("127.0.0.1", 10747);
            }
            catch (SocketException)
            {
                MessageBox.Show("Unable to connect to server!");
            }

            new Thread(() =>
            {
                while (true)
                {
                    //This runs forever until stop is set to true.
                    if (stop)
                    {
                        break;
                    }

                    byte[] buffer = new byte[1024];

                    try
                    {
                        stream = client.GetStream();
                    }
                    catch (InvalidOperationException)
                    {

                    }
                    
                    string data = String.Empty;
                    Int32 bytes = stream.Read(buffer, 0, buffer.Length);
                    data = Encoding.ASCII.GetString(buffer, 0, bytes);

                    //Okay, now, we have the data, it SHOULD be in this format
                    //Qs121=0;0;5.072578;0;0;0.891259;-0.077921
                    //Header;Unused;Unused,Heading(RAD),Unused,Unused,Latitude(RAD),Longitude(RAD)
                    //Next, let's parse the data.
                    double[] locData = ParsePacket(data);
                    playerHdg = locData[0];
                    playerLat = locData[1];
                    playerLng = locData[2];
                    Thread.Sleep(1);
                }
            }).Start();
        }

        public static double[] ParsePacket(string raw)
        {
            int ctr = 0;
            double[] data = new double[3];
            char[] indivLetters = raw.ToCharArray();
            string working = String.Empty;
            bool lastOne = false;
            int lstCtr = 1;

            foreach(char c in indivLetters)
            {
                if (c == '-' && lastOne == true)
                {
                    //Then count up 8 characters AFTER this one
                }
                else
                {
                    if (lastOne == true)
                    {
                        //Count up 8 characters INCLUDING this one
                        if(lstCtr == 8)
                        {
                            double lon = Convert.ToDouble(working);
                            double londeg = lon * 180 / Math.PI;
                            data[2] = londeg;
                            break;
                        }
                        lstCtr++;
                    }
                }
                if (c != ';')
                {
                    //Add this character to the working string
                    working += c;
                }
                else
                {
                    //Figure out which slot this is in, and save it in the array if it is needed.
                    switch (ctr)
                    {
                        case 0:
                            //Header --IGNORE--
                            break;
                        case 1:
                            //--IGNORE--
                            break;
                        case 2:
                            //Heading- Convert to degrees and stick it in data[0]
                            double heading = Convert.ToDouble(working);
                            double degrees = heading * 180 / Math.PI;
                            data[0] = degrees;
                            break;
                        case 3:
                            //--IGNORE--
                            break;
                        case 4:
                            //--IGNORE--
                            break;
                        case 5:
                            //Latitude- Convert to degrees- stick in data[1].
                            double lat = Convert.ToDouble(working);
                            double latdeg = lat * 180 / Math.PI;
                            data[1] = latdeg;
                            lastOne = true;
                            break;
                    }
                    //After we have entered data (or ignored it),
                    //advance the counter and clear the working string.
                    ctr++;
                    working = String.Empty;
                }
            }
            ctr = 0;
            return data;
        }
    }
}
