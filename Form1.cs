﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Ass04_TampusTicod
{
    public partial class fanControl : Form
    {
        private int[] _tempPoints;      //vertical points of the Graph
        private int _tempId = 0;        //Binary value of temperature from LM35
        private int _tempHolder;        //temporary temperature ID storage
        private int _fanSpeed = 0;      //PWM value of the fan speed
        private bool _isManual = true;  //Checks if fan is in manual mode

        #region Local Handlers
        private void UpdateComportList()
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            cboxComport.Items.Clear();

            foreach (var item in ports)
            {
                cboxComport.Items.Add(item);
            }
        }
        #endregion

        //Initializes the Application
        public fanControl()
        {
            InitializeComponent();
            InitializePoints();
        }

        //Updates COM Ports once click event is triggered
        private void cboxComport_Click(object sender, EventArgs e)
        {
            UpdateComportList();
        }

        //Attempts to connect to serial once clicked
        private void btnSerialConnect_Click(object sender, EventArgs e)
        {
            if ("Disconnect" == btnSerialConnect.Text)
            {
                if (true == Serial.IsOpen)
                {
                    tempReading.Text = "NaN";
                    Serial.WriteLine(0.ToString());

                    drawTimer.Stop();
                    Serial.Close();
                }

                btnSerialConnect.Text = "Connect";
                lblStatusText.Text = "Disconnected";
                lblStatusText.ForeColor = Color.Firebrick;
                gbxFanControl.Enabled = false; //disables fan control
                radioBtn_speedOff.Checked = true;
                cboxComport.Enabled = true;
                cboxBaudrate.Enabled = true;

                return;
            }

            //Getting user COM Port from comboBox
            try
            {
                Serial.PortName = cboxComport.Text;
            }
            catch
            {
                MessageBox.Show("COM Port must not be empty.", "No COM Port Selected");
                return;
            }

            //Getting user Baud Rate from comboBox
            try
            {
                Serial.BaudRate = int.Parse(cboxBaudrate.Text);
            }
            catch
            {
                MessageBox.Show("Baud Rate must not be empty.", "No Baudrate selected");
                return;
            }

            //Checking if COM Port is used by another application
            if (false == Serial.IsOpen)
            {
                try
                {   //COM Port available

                    Serial.Open();
                }
                catch
                {
                    MessageBox.Show("COM Port not accessible", "Error");
                    return;
                }

                if (Serial.IsOpen)
                {
                    drawTimer.Start();

                    btnSerialConnect.Text = "Disconnect";
                    lblStatusText.Text = "Connected, via " + cboxComport.Text;
                    lblStatusText.ForeColor = Color.MediumSeaGreen;
                    gbxFanControl.Enabled = true; //enables fan control
                    cboxComport.Enabled = false;
                    cboxBaudrate.Enabled = false;
                }
            }
        }

        //Checks Manual fan mode state
        private void radioBtn_Manual_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtn_Manual.Checked)
            {
                gbxAuto.Enabled = false;
                _isManual = true;

                FanControl(_isManual);
            }
            else
            {
                gbxAuto.Enabled = true;
                _isManual = false;

                FanControl(_isManual);
            }
        }

        //Checks Automatic fan mode state
        private void radioBtn_Auto_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtn_Auto.Checked)
            {
                gbxManual.Enabled = false;
                _isManual = false;

                FanControl(_isManual);
            }
            else
            {
                gbxManual.Enabled = true;
                _isManual = true;

                FanControl(_isManual);
            }
        }

        //Receives data from serial device
        private void Serial_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (Serial.IsOpen)
            {
                string stringTemperature = Serial.ReadLine();
                string strTempTrim = stringTemperature.Trim();

                this.BeginInvoke(new EventHandler(((object o, EventArgs a) =>
                {
                    if (int.TryParse(strTempTrim, out _tempId))
                    {
                        if (!(_tempId > 255))
                        {
                            _tempHolder = _tempId;
                            tempReading.Text = Convert_toTemp(_tempId);

                            FanControl(_isManual);
                            transLateToY(int.Parse(tempReading.Text));
                        }
                        else
                        {
                            tempReading.Text = "NaN";

                            FanControl(_isManual);
                            transLateToY(0);
                        }
                    }
                    else
                    {
                        tempReading.Text = Convert_toTemp(_tempHolder);
                        transLateToY(int.Parse(tempReading.Text));
                    }
                })));
            }
        }

        //Triggers specified events once time specified is elapsed
        private void drawTimer_Tick(object sender, EventArgs e)
        {
            pbGraph.Invalidate();
        }

        //Closes serial connections once form is closed
        private void fanControl_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (true == Serial.IsOpen)
            {
                Serial.WriteLine(0.ToString());
                Serial.Close();
            }
        }

        //Triggers when the value of the numericUpDown changes
        private void tempControl_ValueChanged(object sender, EventArgs e)
        {
            FanControl(_isManual);
        }

        //Functions necessary for the speed changing

        //Draws the graph
        private void pbGraph_Paint(object sender, PaintEventArgs e)
        {
<<<<<<< HEAD
            if (Serial.IsOpen)
            {
                Bitmap graphBm = new Bitmap(240, 125);
                Graphics graph = Graphics.FromImage(graphBm);
                graph.SmoothingMode = SmoothingMode.AntiAlias;

                Brush lineBrush = new SolidBrush(Color.DimGray);
                Brush pointBrush = new SolidBrush(Color.SlateGray);
                Brush graphBrush = new SolidBrush(Color.FromArgb(50, Color.SlateGray));

                Pen graphPen = new Pen(graphBrush, 1);
                Pen linePen = new Pen(lineBrush, 1);
                Pen pointPen = new Pen(pointBrush, 3);

                for (int i = 0; i < 9; i++)
                {

                    graph.DrawLine(graphPen, new Point(240 - (27 * i), 0), new Point(240 - (27 * i), 125));
                    graph.DrawLine(linePen, new Point(240 - (27 * i), (125 - _tempPoints[i])), new Point(240 - (27 * (i + 1)), (125 - _tempPoints[i + 1])));
                    graph.DrawEllipse(pointPen, (240 - (27 * i)) - 1, (125 - _tempPoints[i]) - 1, 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    graph.DrawLine(graphPen, new Point(0, (125 - (27 * i))), new Point(240, (125 - (27 * i))));
                }

                pbGraph.Image = graphBm;

=======
            Bitmap graphBm = new Bitmap(240, 125);
            Graphics graph = Graphics.FromImage(graphBm);
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            Brush lineBrush = new SolidBrush(Color.DimGray);
            Brush pointBrush = new SolidBrush(Color.SlateGray);
            Brush graphBrush = new SolidBrush(Color.FromArgb(50, Color.SlateGray));

            Pen graphPen = new Pen(graphBrush, 1);
            Pen linePen = new Pen(lineBrush, 1);
            Pen pointPen = new Pen(pointBrush, 3);

            if (Serial.IsOpen)
            {
                for (int i = 0; i < 9; i++)
                {
                    graph.DrawLine(graphPen, new Point(240 - (27 * i), 0), new Point(240 - (27 * i), 125));
                    graph.DrawLine(linePen, new Point(240 - (27 * i), (125 - _tempPoints[i])), new Point(240 - (27 * (i + 1)), (125 - _tempPoints[i + 1])));
                    graph.DrawEllipse(pointPen, (240 - (27 * i)) - 1, (125 - _tempPoints[i]) - 1, 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    graph.DrawLine(graphPen, new Point(0, (125 - (27 * i))), new Point(240, (125 - (27 * i))));
                }

                pbGraph.Image = graphBm;
>>>>>>> Amending For final checking and revision
                graph.Dispose();
            }
        }

        //Displays current motor RPM
        private string RpmDisp(int pwmSpeed)
        {
            int rpm;

            rpm = (int)Math.Round(3200 * (pwmSpeed / 255.0));

            return rpm.ToString();
        }

        //Converts temperature from binary to its equivalent integer value
        private string Convert_toTemp(int temp_inBin)
        {
            return Math.Round(((temp_inBin * 0.0043) * 100.0)).ToString();
        }

        //Initializes Y axis of temperature points in the graph
        private void InitializePoints()
        {
            int pointsCount = 10;

            _tempPoints = new Int32[pointsCount];
        }

        //Translates the binary value of the temperature into the Y axis of the temperature points in the graph
        private void transLateToY(int pointId)
        {
            Double tempYCoord = Math.Round((pointId / 110.0) * 120);

            for (int i = 9; i > 0; i--)
            {
                _tempPoints[i] = _tempPoints[i-1];
            }

            _tempPoints[0] = (int)tempYCoord;
        }

        //Adjusts fan speed
        private void FanControl(bool fanControlMode)
        {
            if (_isManual)
            {   //Manual Fan Control Mode

                if (radioBtn_speedOff.Checked)
                {
                    _fanSpeed = 0;
                }
                else if (radioBtn_speedLow.Checked)
                {
                    _fanSpeed = 102;
                }
                else if (radioBtn_speedMedium.Checked)
                {
                    _fanSpeed = 179;
                }
                else if (radioBtn_speedHigh.Checked)
                {
                    _fanSpeed = 255;
                }
                else
                {
                    _fanSpeed = 0;
                }

                if (Serial.IsOpen)
                {
                    testSpeed.Text = _fanSpeed.ToString();
                    Serial.WriteLine(_fanSpeed.ToString());

                    lblRPM.Text = RpmDisp(_fanSpeed);
                }
            }
            else
            {   //Automatic Fan Control Mode

                double tempDifference = ((_tempId * 0.0043) * 100.0) - (double)tempControl.Value;

                intDiff.Text = tempDifference.ToString();

                if (tempDifference >= 5.0)  
                {
                    //triggers if temperature differential is above 5 degrees

                    _fanSpeed = 255;
                }
                else if (tempDifference < 5.0 && tempDifference > 0)
                {
                    //triggers if temperature differential is between 0 and 5 degrees

                    double speedDouble = Math.Round((tempDifference / 5.0) * 255);
                    _fanSpeed = (int)speedDouble;
                }
                else                        
                {
                    //triggers when temperature differential is 0 degrees below;
                    
                    _fanSpeed = 0;
                }

                testSpeed.Text = _fanSpeed.ToString(); //to be removed

                if (Serial.IsOpen)
                {
                    Serial.WriteLine(_fanSpeed.ToString());
                    lblRPM.Text = RpmDisp(_fanSpeed);
                }
            }
        }
    }
}