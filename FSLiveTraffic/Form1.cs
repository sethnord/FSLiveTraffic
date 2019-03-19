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
        int _trackRad = 200;
        int _maxAC = 100;

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
            MessageBox.Show(AirTraffic.Get(PlayerData.playerLat, PlayerData.playerLng, _trackRad));
        }
    }
}
