using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Net;
using System.Net.Sockets;

namespace FSLiveTraffic
{
    //TODO: Delete unneccesary comments.
    //TODO: Clean up code.
    public partial class Form1 : Form
    {
        Aircraft[] _planeList;

        int _trackRad = 200;
        public static int _maxAC = 100;

        System.Timers.Timer wxUpdate;
        int _wxupdateInterval = 8000;

        public static bool _TcpConnected = false;
        bool _UdpConnected = false;
        bool _ADSBConnected = false;
        bool _WxConnected = false;

        public Form1()
        {
            InitializeComponent();
            //Create WX update timer.
            wxUpdate = new System.Timers.Timer();
            wxUpdate.Interval = _wxupdateInterval;
            wxUpdate.Elapsed += WxUpdate_Elapsed;
            //We will wait to start this until we are connected to the weather server.
        }

        private void WxUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Called every time the timer interval elapses.
            //Update weather info
            if (_TcpConnected)
            {
                //If we are connected to the player location server, go ahead and get the first wx request
                string[] response = WxManager.GetMetar(PlayerData.playerLat, PlayerData.playerLng, 20); //Get the closest metar within a 20 nm radius
                                                                                                        //Convert from inHg to QNH
                                                                                                        //Update player location display
                

                if (label2.InvokeRequired)
                {
                    label2.Invoke(new Action(() => label2.Text = PlayerData.playerLat.ToString()));
                }
                else
                {
                    label2.Text = PlayerData.playerLat.ToString();
                }

                if (label3.InvokeRequired)
                {
                    label3.Invoke(new Action(() => label3.Text = PlayerData.playerLng.ToString()));
                }
                else
                {
                    label3.Text = PlayerData.playerLng.ToString();
                }
                
                if(response[0] != null)
                {
                    double pressure = Convert.ToDouble(response[5]) * 33.864;


                    //Update the screen
                    //Combine the wind speed
                    string winddir = response[3];
                    string windspd = response[4];
                    if (label28.InvokeRequired)
                    {
                        label28.Invoke(new Action(() => label28.Text = winddir + "/" + windspd));
                    }
                    else
                    {
                        label28.Text = winddir + "/" + windspd;
                    }

                    if (label26.InvokeRequired)
                    {
                        label26.Invoke(new Action(() => label26.Text = pressure.ToString()));
                    }
                    else
                    {
                        label26.Text = pressure.ToString();
                    }

                    if (label24.InvokeRequired)
                    {
                        label24.Invoke(new Action(() => label24.Text = response[0]));
                    }
                    else
                    {
                        label24.Text = response[0];
                    }

                    //Now that we have updated the screen, we need to send the pressure over UDP
                    //First, convert double to whole int- then to string.
                    int QNH = Convert.ToInt32(pressure);

                    string almost = QNH.ToString();

                    string final = "Q" + almost; //So it will come out as 'Q1013' instead of just '1013'- this is required by the server software.


                    //Then, make it a byte[] that can be sent via UDP

                    byte[] toSend = Encoding.ASCII.GetBytes(final);

                    UdpClient udpSender = new UdpClient();

                    //Connect to the server
                    udpSender.Connect(IPAddress.Loopback, 49004);

                    //Send it!
                    //Per the spec, the QNH only needs to be sent every 8 or 9 seconds, but I don't want to DDOS the
                    //federal government's weather servers, so there isn't really much I can do here.
                    //Okay... they don't explicitly say not to do that, so I don't see much harm in requesting 1 station.
                    //I'll change the per minute section to be per second.
                    udpSender.Send(toSend, toSend.Length);

                    if (_ADSBConnected)
                    {
                        //I'm piggybacking off the wx timer because I'm too lazy to make another one.
                        //TODO: make a timer exclusively for ADS-B Data.
                        UpdateAircraft(AirTraffic.Get(PlayerData.playerLat, PlayerData.playerLng, _trackRad));
                    }
                }
            }
            //Otherwise do nothing
        } //Called every time the timer elapses

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        } //Unused

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //Change tracking radius
            _trackRad = trackBar1.Value;
            //Change the label
            label18.Text = _trackRad.ToString();
        } //Aircraft detection radius

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //Change max no of AC
            _maxAC = trackBar2.Value;
            //Change the label
            label30.Text = _maxAC.ToString();
        } //Max number of aircraft

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //Multiply 1000 by the value
            _wxupdateInterval = 1000 * (int)numericUpDown1.Value;
            wxUpdate.Interval = _wxupdateInterval;
            //By default, this is set to 8000, or 8 seconds.
        } //Weather Update Interval

        private void button4_Click(object sender, EventArgs e)
        {
            //First, test the connection to the server.
            if (WxManager.TestConnection())
            {
                if (_TcpConnected)
                {
                    //If we are already connected to the player location server, go ahead and get the first wx request
                    string[] response = WxManager.GetMetar(PlayerData.playerLat, PlayerData.playerLng, 20); //Get the closest metar within a 20 nm radius
                    //Convert from inHg to QNH
                    double pressure = Convert.ToDouble(response[5]) * 33.864;

                    //Update the screen
                    //Combine the wind speed
                    string winddir = response[3];
                    string windspd = response[4];
                    if (label28.InvokeRequired)
                    {
                        label28.Invoke(new Action(() => label28.Text = winddir + "/" + windspd));
                    }
                    else
                    {
                        label28.Text = winddir + "/" + windspd;
                    }

                    if (label26.InvokeRequired)
                    {
                        label26.Invoke(new Action(() => label26.Text = pressure.ToString()));
                    }
                    else
                    {
                        label26.Text = pressure.ToString();
                    }

                    if (label24.InvokeRequired)
                    {
                        label24.Invoke(new Action(() => label24.Text = response[0]));
                    }
                    else
                    {
                        label24.Text = response[0];
                    }

                    toolStripStatusLabel11.ForeColor = Color.LimeGreen;
                    toolStripStatusLabel11.Text = "CONNECTED";
                }
                //Now start the timer
                wxUpdate.Start();
            }
        } //WX Connect Button

        private void button1_Click(object sender, EventArgs e)
        {
            //Connect via TCP to the player location... ehrm, API?
            PlayerData.StartConnection(); //This connects and starts updating the player position variables.
            //In order to stop this, we need to set a variable to stop.
            toolStripStatusLabel2.ForeColor = Color.LimeGreen;
            toolStripStatusLabel2.Text = "CONNECTED";
            button1.Enabled = false;
            _TcpConnected = true;
        } //TCP Connect Button

        public void DisconnectTCP()
        {
            toolStripStatusLabel2.Text = "DISCONNECTED";
            toolStripStatusLabel2.ForeColor = Color.Red;
            button1.Enabled = true;
        }

        public void DisconnectWX()
        {
            toolStripStatusLabel11.Text = "DISCONNECTED";
            toolStripStatusLabel11.ForeColor = Color.Red;
            button4.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _planeList = AirTraffic.Get(PlayerData.playerLat, PlayerData.playerLng, _trackRad);
            UpdateAircraft(_planeList);
        }

        private void UpdateAircraft(Aircraft[] list)
        {
            //Send updated aircraft list via tcp.
            for(int i = 0; i < _maxAC; i++)
            {
                //We'll need to manually exit the loop if we reach the end of the array
                string[] toSend = new string[15];

                Aircraft working = list[i];


                try
                {
                    toSend[0] = working.icao;
                    toSend[1] = working.callsign;
                    toSend[2] = working.origin_country;
                    toSend[3] = working.time_position;
                    toSend[4] = working.last_contact;
                    toSend[5] = working.longitude;
                    toSend[6] = working.latitude;
                    toSend[7] = working.baro_altitude;
                    toSend[8] = working.on_ground;
                    toSend[9] = working.velocity;
                    toSend[10] = working.true_track;
                    toSend[11] = working.vertical_rate;
                    toSend[12] = working.geo_altitude;
                    toSend[13] = working.sqwawk;
                    toSend[14] = working.spi;
                }
                catch (NullReferenceException)
                {
                    break;
                }

                double agl = 0.0;
                double vs = 0.0;
                int ab = 0;
                double track = 0.0;
                double gs = 0.0;

                try
                {
                    agl = Convert.ToDouble(toSend[12]) * 3.281;
                    vs = Convert.ToDouble(toSend[11]) * 3.281;
                    if(toSend[8] == "false")
                    {
                        ab = 0;
                    }
                    else
                    {
                        ab = 1;
                    }
                    track = Convert.ToDouble(toSend[10]);
                    gs = Convert.ToDouble(toSend[9]) * 1.944;
                }
                catch (IndexOutOfRangeException)
                {
                    break;
                }
                
                string acType = "B738"; //TODO: Find some way to parse a CSV database and get the AC type.
                string tailNo = "N1234A"; //TODO: "" "" and get the tail number.
                //Because this is set to B738, all aircraft will show up as 737-800s with a tail number of N1234A

                //Now, condense this all into one string...
                //Per the spec:
                //AITFC,ICAO ID,LAT,LONG,ALT(FT),V/S(FT/MIN),AIRBORNE(1/0),TRACK(DEG),GROUNDSPEED(KTS),CALLSIGN,ICAO TYPE CODE,TAIL NO,ORIGIN,DESTINATION
                string data = "AITFC," + toSend[0] + "," + toSend[6] + "," + toSend[5] + "," + agl.ToString() + "," 
                    + vs + "," + ab + "," + track + "," + gs + "," + toSend[1] + "," + acType + "," + tailNo + ",,";

                byte[] sendable = Encoding.ASCII.GetBytes(data);
                SendACData(sendable);
            }
        }

        private void SendACData(byte[] data)
        {
            UdpClient udp = new UdpClient();
            udp.Connect(IPAddress.Loopback, 49003);
            udp.Send(data, data.Length);
            _ADSBConnected = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
