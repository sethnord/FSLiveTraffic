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

namespace FSLiveTraffic
{
    //TEST COMMIT
    public partial class Form1 : Form
    {
        int _trackRad = 200;
        int _maxAC = 100;

        System.Timers.Timer wxUpdate;
        int _wxupdateInterval = 60000;

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
                if(response[0] != null)
                {
                    double pressure = Convert.ToDouble(response[5]) * 33.864;

                    MessageBox.Show(pressure.ToString()); //DEBUG

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
                }
            }
            //Otherwise do nothing
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //Change tracking radius
            _trackRad = trackBar1.Value;
            //Change the label
            label18.Text = _trackRad.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //Change max no of AC
            _maxAC = trackBar2.Value;
            //Change the label
            label30.Text = _maxAC.ToString();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //Multiply 60000 by the value
            _wxupdateInterval = 60000 * (int)numericUpDown1.Value;
            wxUpdate.Interval = _wxupdateInterval;
        }

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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Connect via TCP to the player location... ehrm, API?
            PlayerData.StartConnection(); //This connects and starts updating the player position variables.
            //In order to stop this, we need to set a variable to stop.
            toolStripStatusLabel2.ForeColor = Color.LimeGreen;
            toolStripStatusLabel2.Text = "CONNECTED";
            _TcpConnected = true;
        }
    }
}
