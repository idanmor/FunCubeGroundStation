using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GroundStation
{
    public partial class TelemetryWindow : Form
    {
        System.Windows.Forms.Timer RealTimeDemo = new System.Windows.Forms.Timer();
        
        public TelemetryWindow()
        {
            InitializeComponent();
            RealTimeDemo.Interval = 5000;
            RealTimeDemo.Tick += new EventHandler(UpdateNewTelemetry);
            RealTimeDemo.Enabled = true;


        }

        private void BrowseWAV_Click(object sender, EventArgs e)
        {
            OpenWavFile.ShowDialog();
        }

        private void OpenWavFile_FileOk(object sender, CancelEventArgs e)
        {
            WAVfilePath.Text = OpenWavFile.FileName;
            Console.WriteLine(OpenWavFile.FileName);
            string RunDecBatchFile = @"rundec.bat";
            string SourceWav = @OpenWavFile.FileName;
            string PacketsFile = @"sssssssss.txt";

            var process = new Process
            {
                StartInfo =
                {
                    Arguments = String.Format("\"{0}\" \"{1}\"", SourceWav, PacketsFile) 
                }
            };
            process.StartInfo.FileName = RunDecBatchFile;
            process.EnableRaisingEvents = true;
            process.Exited += Parsing;
            bool b = process.Start();


        }

        private void Parsing(object sender, EventArgs e)
        {
          //  List<Telemetry> AllTelem = new List<Telemetry>();

            string[] Packets = File.ReadAllLines(@"sssssssss.txt");
            for (int i = 0; i < Packets.Length; i++)
            {
                byte[] bytes = Parser.StringToByte(Packets[i]);
               GroundStation.AllTelem.Add(new Telemetry(bytes));
            }
            ((Process)sender).Exited -= Parsing;
            ((Process)sender).EnableRaisingEvents = false;
            ((Process)sender).WaitForExit();
            ((Process)sender).Dispose();
            ((Process)sender).Close();
            
            
           UpdateGUIdel();
            
            
        }

        delegate void ChangeGUIVal();
       
        
    

         public  void  UpdateGUIdel()
        {
            ChangeGUIVal update = new ChangeGUIVal(StartUpdateGUI);
            this.Invoke(update);
           
        }

         public void UpdateNewTelemetry(Object myObject, EventArgs myEventArgs)
         {
             RealTimeDemo.Stop();
             if (GroundStation.IndexOfTelemetry < GroundStation.AllTelem.Count)
             {
                 
                 Telemetry telem = GroundStation.AllTelem.ElementAt(GroundStation.IndexOfTelemetry);
                 SystemSounds.Exclamation.Play();
                 RefreshGUIForNewValues(telem);
                 GroundStation.IndexOfTelemetry++;
                 RealTimeDemo.Start();
             }
             else
                 RealTimeDemo.Enabled = false;
         }

        

         public void StartUpdateGUI()
         {
             RealTimeDemo.Enabled = true;       
             UpdateNewTelemetry(null,null); // for first telemetry in each new wavfile !!!

         }

         private void Form1_Load(object sender, EventArgs e)
         {

         }

         public void RefreshGUIForNewValues(Telemetry telem)
         {
             //              EPS     
             this.PanXvoltTextBox.Text = telem.Get(Telemetry.DataIndex.PanelVoltX).AsDouble.ToString("F2") + " mV";
             this.PanYvoltTextBox.Text = telem.Get(Telemetry.DataIndex.PanelVoltY).AsDouble.ToString("F2") + " mV";
             this.PanZvoltTextBox.Text = telem.Get(Telemetry.DataIndex.PanelVoltZ).AsDouble.ToString("F2") + " mV";
             this.TotalPhCTextBox.Text = telem.Get(Telemetry.DataIndex.PanelCurrentTotal).AsDouble.ToString("F2") + " mA";
             this.BatteryVoltTextBox.Text = telem.Get(Telemetry.DataIndex.BattVolt0).AsDouble.ToString("F2") + " mV";
             this.TotalSysCTextBox.Text = telem.Get(Telemetry.DataIndex.BattCurrentBus).AsDouble.ToString("F2") + " mA";
             this.RebootCountTextBox.Text = telem.Get(Telemetry.DataIndex.RebootCount).AsInt.ToString();
             this.EPSSoftErrTextBox.Text = telem.Get(Telemetry.DataIndex.EpsErrorCount).AsInt.ToString();
             this.BconvTemp1textBox.Text = telem.Get(Telemetry.DataIndex.EpsTemp1).AsDouble.ToString("F2") + " ºc";
             this.BconvTemp2textBox.Text = telem.Get(Telemetry.DataIndex.EpsTemp2).AsDouble.ToString("F2") + " ºc";
             this.BconvTemp3textBox.Text = telem.Get(Telemetry.DataIndex.EpsTemp3).AsDouble.ToString("F2") + " ºc";
             this.BatteryTemptextBox.Text = telem.Get(Telemetry.DataIndex.BattTemp0).AsDouble.ToString("F2") + " ºc";
             this.Latch5CounttextBox.Text = telem.Get(Telemetry.DataIndex.LatchCount5_0).AsDouble.ToString("F2");
             this.Latch33CounttextBox.Text = telem.Get(Telemetry.DataIndex.LatchCount3_3).AsDouble.ToString("F2");
             this.ResetCausetextBox.Text = telem.Get(Telemetry.DataIndex.ResetCause).AsString;
             this.PPTrackingtextBox.Text = telem.Get(Telemetry.DataIndex.PptTrackingMode).AsString;

             //               BOB
             this.SunXplustextBox.Text = telem.Get(Telemetry.DataIndex.AsibSunSensorX1).AsDouble.ToString("F2");
             this.SunYplustextBox.Text = telem.Get(Telemetry.DataIndex.AsibSunSensorY1).AsDouble.ToString("F2");
             this.SunZplustextBox.Text = telem.Get(Telemetry.DataIndex.AsibSunSensorZ1).AsDouble.ToString("F2");
             this.XPlustmptextBox.Text = telem.Get(Telemetry.DataIndex.AsibPanelTempX1).AsDouble.ToString("F2") + " ºc";
             this.XminustmptextBox.Text = telem.Get(Telemetry.DataIndex.AsibPanelTempX2).AsDouble.ToString("F2") + " ºc";
             this.YPlustmptextBox.Text = telem.Get(Telemetry.DataIndex.AsibSunSensorY1).AsDouble.ToString("F2") + " ºc";
             this.YminustmptextBox.Text = telem.Get(Telemetry.DataIndex.AsibPanelTempY2).AsDouble.ToString("F2") + " ºc";
             this.Bus33Volttextbox.Text = telem.Get(Telemetry.DataIndex.AsibBusVolt3_3).AsDouble.ToString("F2") + " mV";
             this.Bus33CurrtextBox.Text = telem.Get(Telemetry.DataIndex.AsibBusCurrent3_3).AsDouble.ToString("F2") + " mA";
             this.Bus5Volttextbox.Text = telem.Get(Telemetry.DataIndex.AsibBusVolt5_0).AsDouble.ToString("F2") + " mV";

             //               RF
             this.DoplerRXtextBox.Text = telem.Get(Telemetry.DataIndex.RfReceiverDoppler).AsDouble.ToString("F2") + " kHZ";
             this.RssiRXtextBoxtextBox.Text = telem.Get(Telemetry.DataIndex.RfReceiverRSSI).AsDouble.ToString("F2") + " kHZ";
             this.RfTemptextBox.Text = telem.Get(Telemetry.DataIndex.RfTemp).AsDouble.ToString("F2") + " ºc";
             this.RxCurrtextBox.Text = telem.Get(Telemetry.DataIndex.RfReceiveCurrent).AsDouble.ToString("F2") + " mA";
             this.Tx33CurrtextBox.Text = telem.Get(Telemetry.DataIndex.RfTransmitCurrent3_3).AsDouble.ToString("F2") + " mA";
             this.Tx5CurrtextBox.Text = telem.Get(Telemetry.DataIndex.RfTransmitCurrent5_0).AsDouble.ToString("F2") + " mA";

             //               PA
             this.RxPowertextBox.Text = telem.Get(Telemetry.DataIndex.PaReversePower).AsDouble.ToString("F2") + " mW";
             this.FrwPowertextBox.Text = telem.Get(Telemetry.DataIndex.PaForwardPower).AsDouble.ToString("F2") + " mW";
             this.BoardtTemptextBox.Text = telem.Get(Telemetry.DataIndex.PaTemperature).AsDouble.ToString("F2") + " ºc";
             this.BoardCurrtextBox.Text = telem.Get(Telemetry.DataIndex.PaCurrent).AsDouble.ToString("F2") + " mA";

             //               ANTS
             this.AntTemp0textBox.Text = telem.Get(Telemetry.DataIndex.AntTempA).AsDouble.ToString("F2") + " ºc";
             this.AntTemp1textBox.Text = telem.Get(Telemetry.DataIndex.AntTempB).AsDouble.ToString("F2") + " ºc";
             this.deploy0.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.AntDeploy1).AsBool);
             this.deploy1.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.AntDeploy2).AsBool);
             this.deploy2.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.AntDeploy3).AsBool);
             this.deploy3.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.AntDeploy4).AsBool);

             //               SW
             this.SeqNumtextBox.Text = telem.Get(Telemetry.DataIndex.SequenceNumber).AsInt.ToString();
             this.DTMFCmdCnttextBox.Text = telem.Get(Telemetry.DataIndex.DtmfCommandCount).AsInt.ToString();
             this.DTMFLstCmdtextBox.Text = telem.Get(Telemetry.DataIndex.DtmfLastCommand).AsInt.ToString();
             this.DTMFCmdSucc.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DtmfCommandSuccess).AsBool);
             this.DataASIB.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DataValidBob).AsBool);
             this.DataEPS.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DataValidEps).AsBool);
             this.DataPA.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DataValidPa).AsBool);
             this.DataRF.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DataValidRf).AsBool);
             this.DataMSE.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DataValidMse).AsBool);
             this.DataANTS_A.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DataValidAnts2).AsBool);
             this.DataANTS_B.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DataValidAnts1).AsBool);
             this.Eclipse.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.InEclipse).AsInt.ToString());
             this.Safe.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.InSafeMode).AsInt.ToString());
             this.ABFHrd.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.HardwareABF).AsInt.ToString());
             this.ABFSoft.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.SoftwareABF).AsBool);
             this.DeployWait.ForeColor = Parser.OnOffElement(telem.Get(Telemetry.DataIndex.DeployWait).AsInt.ToString());

             //             GENERAL
             this.FrameIdtextBox.Text = telem.Get(Telemetry.DataIndex.FrameId).AsString;
             this.SatIdtextBox.Text = telem.Get(Telemetry.DataIndex.SatelliteId).AsInt.ToString();

             if (telem.IsFitter)
                 this.FitterMessageWindow.AppendText("   " + telem.SequenceNumber.ToString() + " , " + telem.Get(Telemetry.DataIndex.FrameId).AsString+" : " + Parser.GetFitterMessage(telem.Payload) + "\n");
             

         }

         private void ResetButton_Click(object sender, EventArgs e)
         {
             RealTimeDemo.Stop();
             RealTimeDemo.Enabled = false;
             GroundStation.AllTelem = new List<Telemetry>();
             GroundStation.IndexOfTelemetry = 0;
             ResetGUI();

         }

        private void ResetGUI(){

            //              EPS     
            this.PanXvoltTextBox.Text ="";
            this.PanYvoltTextBox.Text = "";
            this.PanZvoltTextBox.Text = "";
            this.TotalPhCTextBox.Text = "";
            this.BatteryVoltTextBox.Text = "";
            this.TotalSysCTextBox.Text = "";
            this.RebootCountTextBox.Text = "";
            this.EPSSoftErrTextBox.Text = "";
            this.BconvTemp1textBox.Text = "";
            this.BconvTemp2textBox.Text = "";
            this.BconvTemp3textBox.Text = "";
            this.BatteryTemptextBox.Text = "";
            this.Latch5CounttextBox.Text = "";
            this.Latch33CounttextBox.Text = "";
            this.ResetCausetextBox.Text = "";
            this.PPTrackingtextBox.Text = "";

            //               BOB
            this.SunXplustextBox.Text = "";
            this.SunYplustextBox.Text = "";
            this.SunZplustextBox.Text = "";
            this.XPlustmptextBox.Text = "";
            this.XminustmptextBox.Text = "";
            this.YPlustmptextBox.Text = "";
            this.YminustmptextBox.Text = "";
            this.Bus33Volttextbox.Text = "";
            this.Bus33CurrtextBox.Text = "";
            this.Bus5Volttextbox.Text = "";

            //               RF
            this.DoplerRXtextBox.Text = "";
            this.RssiRXtextBoxtextBox.Text = "";
            this.RfTemptextBox.Text = "";
            this.RxCurrtextBox.Text = "";
            this.Tx33CurrtextBox.Text = "";
            this.Tx5CurrtextBox.Text = "";

            //               PA
            this.RxPowertextBox.Text = "";
            this.FrwPowertextBox.Text = "";
            this.BoardtTemptextBox.Text = "";
            this.BoardCurrtextBox.Text = "";

            //               ANTS
            this.AntTemp0textBox.Text = "";
            this.AntTemp1textBox.Text = "";
            this.deploy0.ForeColor = Color.Black;
            this.deploy1.ForeColor = Color.Black;
            this.deploy2.ForeColor = Color.Black;
            this.deploy3.ForeColor = Color.Black;

            //               SW
            this.SeqNumtextBox.Text = "";
            this.DTMFCmdCnttextBox.Text = "";
            this.DTMFLstCmdtextBox.Text = "";
            this.DTMFCmdSucc.ForeColor = Color.Black;
            this.DataASIB.ForeColor = Color.Black;
            this.DataEPS.ForeColor = Color.Black;
            this.DataPA.ForeColor = Color.Black;
            this.DataRF.ForeColor = Color.Black;
            this.DataMSE.ForeColor = Color.Black;
            this.DataANTS_A.ForeColor = Color.Black;
            this.DataANTS_B.ForeColor = Color.Black;
            this.Eclipse.ForeColor = Color.Black;
            this.Safe.ForeColor = Color.Black;
            this.ABFHrd.ForeColor = Color.Black;
            this.ABFSoft.ForeColor = Color.Black;
            this.DeployWait.ForeColor = Color.Black;

            //             GENERAL
            this.FrameIdtextBox.Text = "";
            this.SatIdtextBox.Text = "";

            this.FitterMessageWindow.Clear();

        }
       
    }
}
