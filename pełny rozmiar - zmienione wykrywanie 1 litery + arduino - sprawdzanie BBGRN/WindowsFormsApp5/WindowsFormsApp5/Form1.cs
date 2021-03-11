using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//using EasyModbus;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;

namespace WindowsFormsApp5
{
    

    public partial class Form1 : Form
    {
        SerialPort port, port2;
        string lineReadIn, lineReadIn2;
        bool temperaturaOK = false;
        string temperatura_do_MES1, temperatura_do_MES2;
        


        // this will prevent cross-threading between the serial port
        // received data thread & the display of that data on the central thread
        private delegate void preventCrossThreading(string x);
        private preventCrossThreading accessControlFromCentralThread;
        private preventCrossThreading accessControlFromCentralThread2;



        public Form1()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            WindowState = FormWindowState.Maximized;



            ToolTip toolTip1 = new ToolTip();
            toolTip1.ShowAlways = true;
            toolTip1.AutoPopDelay = 3000;
            toolTip1.InitialDelay = 50;
            toolTip1.ReshowDelay = 200;
            toolTip1.SetToolTip(textBox2, "hasło: UTR");

            // create and open the serial port (configured to my machine)
            // this is a Down-n-Dirty mechanism devoid of try-catch blocks and
            // other niceties associated with polite programming
            const string com = "COM2";
            port = new SerialPort(com, 115200, Parity.Even, 8, StopBits.One);

            //   port.ErrorReceived += new SerialErrorReceivedEventHandler();
            try
            {
                port.Open();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Error: Port " + com + " jest zajęty");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uart exception: " + ex);
            }


 

            if (port.IsOpen)
            {
                // set the 'invoke' delegate and attach the 'receive-data' function
                // to the serial port 'receive' event.

                accessControlFromCentralThread = displayTextReadIn;
                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                
            }

            const string com2 = "COM6";     //skaner potting
                                            //port3 = new SerialPort(com3, 9600, Parity.None, 8, StopBits.One);
            port2 = new SerialPort(com2, 9600, Parity.None, 8, StopBits.One);
            // port2 = new SerialPort(com2, 9600, Parity.None, 8, StopBits.One);

            try
            {
                port2.Open();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Error: Port " + com + " jest zajęty");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uart exception: " + ex);
            }



            //port3 = new SerialPort(com3, 115200, Parity.Even, 8, StopBits.One);
            if (port2.IsOpen)
            {
                // set the 'invoke' delegate and attach the 'receive-data' function
                // to the serial port 'receive' event.

                accessControlFromCentralThread2 = displayTextReadIn2;
                port2.DataReceived += new SerialDataReceivedEventHandler(port2_DataReceived);

            }





        }




        public bool ControlInvokeRequired(Control c, Action a)
        {
            if (c.InvokeRequired) c.Invoke(new MethodInvoker(delegate { a(); }));
            else return false;

            return true;
        }
        public void UpdateControl(Control myControl, Color c, String s, bool widzialnosc)
        {
            //Check if invoke requied if so return - as i will be recalled in correct thread
            if (ControlInvokeRequired(myControl, () => UpdateControl(myControl, c, s, widzialnosc))) return;
            myControl.Text = s;
            myControl.BackColor = c;
            myControl.Visible = widzialnosc;
        }












        public void OnApplicationExit(object sender, EventArgs e)
        {
            try
            {
                port.Write("LOFF\r");
            }
            catch
            {
                MessageBox.Show("Brak możlowości wyzwolenia skaneru", "Info", MessageBoxButtons.OK);
            }
            System.Windows.Forms.Application.Exit();

        }



        string wydruk;
//        string[] result = new string[100];

        // this is called when the serial port has receive-data for us.
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs rcvdData)
        {
            start = DateTime.Now;

            while (port.BytesToRead > 0)
            {

                //   lineReadIn += port.ReadExisting();

                lineReadIn += port.ReadExisting();
                //   lineReadIn += Environment.NewLine;

                // lineReadIn += "\r\n";
                //   lineReadIn += lineReadIn;

           //     flaga = false;
           //     aTimer.Stop();

                Thread.Sleep(25);
            }

            //   flaga = true;


            // display what we've acquired.
            
            UpdateControl(label2, SystemColors.Control, "Błąd czytania barkodu. Otwórz szufladę i sprawdź barkod", false);

            lineReadIn = lineReadIn.ToUpper();
            string firstletter = lineReadIn.Remove(1);

            lineReadIn = Regex.Replace(lineReadIn, @"\s+", string.Empty);
            if (lineReadIn.Length > 19  || lineReadIn.Equals("E"))
            {
                UpdateControl(label2, Color.Red, "Błąd czytania barkodu. Otwórz szufladę i sprawdź barkod", true);
                this.BackColor = Color.FromArgb(255, 128, 128);
            }
            else if (lineReadIn.Length < 19)
            {
                UpdateControl(label2, Color.Red, "Błąd czytania barkodu. Otwórz szufladę i sprawdź barkod", true);
                this.BackColor = Color.FromArgb(255, 128, 128);
            }
            else if (firstletter.Equals("A"))
            {
                if (sprawdzeniekrok(lineReadIn) == 1)
                {
                    tworzeniepliku(lineReadIn);                    
                }
                else
                    this.BackColor = Color.FromArgb(255, 128, 128);

                displayTextReadIn(lineReadIn);
                //  Application.DoEvents();
                //  Thread.Sleep(5000);

                Task.Run(async () => await Test());
             //   UpdateControl(label3, Color.LawnGreen, "Skanowanie OK", false);
              //  this.BackColor = SystemColors.Control;
            }

            displayTextReadIn(lineReadIn);
            wydruk = lineReadIn;
            lineReadIn = string.Empty;




        }// end function 'port_dataReceived'

        private  async Task Test()
        {

                      await Task.Delay(5000);
                     UpdateControl(label3, Color.LawnGreen, "Skanowanie OK", false);
                    this.BackColor = SystemColors.Control;
            // await - jakas inna dluga operacja
        }

        private void port2_DataReceived(object sender, SerialDataReceivedEventArgs rcvdData)
        {

            while (port2.BytesToRead > 0)
            {

                //   lineReadIn += port.ReadExisting();

                lineReadIn2 += port2.ReadExisting();
                //   lineReadIn += Environment.NewLine;

                // lineReadIn += "\r\n";
                //   lineReadIn += lineReadIn;

                //     flaga = false;
                //     aTimer.Stop();

                Thread.Sleep(25);
            }



            if (lineReadIn2.Contains("OK"))
            {
                temperaturaOK = true;
                UpdateControl(label8, Color.LawnGreen, "temperaturaOK", true);
            }
            else if (lineReadIn2.Contains("BAD"))
            { 
                temperaturaOK = false;
                UpdateControl(label8, Color.Red, "temperaturaNOK", true);
            }
            else if (lineReadIn2.Contains("A2"))
            {
                temperatura_do_MES1 = lineReadIn2;
                temperatura_do_MES1 = temperatura_do_MES1.Remove(0, 8);
                UpdateControl(label6, Color.WhiteSmoke, temperatura_do_MES1, true);
            }
            else if (lineReadIn2.Contains("A4"))
            {
                temperatura_do_MES2 = lineReadIn2;
                temperatura_do_MES2 = temperatura_do_MES2.Remove(0, 8);
                UpdateControl(label7, Color.WhiteSmoke, temperatura_do_MES2, true);
            }
            if (!(lineReadIn2.Contains("BAD") || lineReadIn2.Contains("OK")))
                ;
             //   displayTextReadIn2(lineReadIn2);
            lineReadIn2 = string.Empty;


        }

        private void displayTextReadIn2(string ToBeDisplayed)          //wyswietlanie sygnalu na drugim texboxie
        {
            
            if (label5.InvokeRequired)
                label5.BeginInvoke(accessControlFromCentralThread, ToBeDisplayed);
            else
                label5.Text = ToBeDisplayed;

        }


        // this, hopefully, will prevent cross threading.
        private void displayTextReadIn(string ToBeDisplayed)          //wyswietlanie sygnalu na drugim texboxie
        {
            if (textBox1.InvokeRequired)
                textBox1.BeginInvoke(accessControlFromCentralThread, ToBeDisplayed);
            else
                textBox1.Text = ToBeDisplayed;
          
        }


        DateTime stop, start;
        //---------------------------------------------------------------------------------------------

        


        private int tworzeniepliku(string sn)
        {

            sn = Regex.Replace(sn, @"\s+", string.Empty);

            //     if(sn.Length > 8)
            //      sn = sn.Remove(8);

            if (sn == "ERROR" || sn.Length != 19)
            {
                UpdateControl(label2, Color.Red, "Błąd czytania barkodu. Otwórz szufladę i sprawdź barkod", true);
                UpdateControl(label3, Color.LawnGreen, "Skanowanie OK", false);
                this.BackColor = Color.FromArgb(255, 128, 128);
                return 0;
            }
            else
            {
              //  this.BackColor = Color.FromArgb(0, 192, 0);
                UpdateControl(label2, Color.Red, "Błąd czytania barkodu. Otwórz szufladę i sprawdź barkod", false);
                UpdateControl(label3, Color.LawnGreen, "Skanowanie OK", true);
                
                if (temperaturaOK)
                {
                    this.BackColor = Color.FromArgb(0, 192, 0);
                    port2.Write("@");
                    temperaturaOK = false;
                }
                else
                {                    
                   // Application.DoEvents();
                    if (!temperaturaOK)
                    {
                       UpdateControl(label2, Color.Red, "Błąd temperatury", true);
                       UpdateControl(label3, Color.LawnGreen, "Skanowanie OK", false);
                       this.BackColor = Color.FromArgb(255, 128, 128);
                    }

                }
            }
            stop = DateTime.Now;
            string stop_String = stop.ToString("yyyy-MM-dd HH:mm:ss");
            // textBox1.Text = sn;

            string sciezka = (@"C:/tars/");      //definiowanieścieżki do której zapisywane logi
            string sourceFile = @"C:/tars/" + @sn + @"(" + @stop.ToString("yyyy-MM-dd HH-mm-ss") + @")" + @".Tars";
            string destinationFile = @"C:/copylogi/" + @stop.Day + @"-" + @stop.Month + @"-" + @stop.Year + @"/" + @sn + @"(" + @stop.ToString("yyyy-MM-dd HH-mm-ss") + @")" + @".Tars";

            stop = DateTime.Now;


            if (Directory.Exists(sciezka))       //sprawdzanie czy sciezka istnieje
            {
                ;
            }
            else
                System.IO.Directory.CreateDirectory(sciezka); //jeśli nie to ją tworzy

            if (Directory.Exists(@"C:/copylogi/" + @stop.Day + @"-" + @stop.Month + @"-" + @stop.Year + @"/"))       //sprawdzanie czy sciezka istnieje
            {
                ;
            }
            else
                System.IO.Directory.CreateDirectory(@"C:/copylogi/" + @stop.Day + @"-" + @stop.Month + @"-" + @stop.Year + @"/"); //jeśli nie to ją tworzy


            try
            {
                using (StreamWriter sw = new StreamWriter("C:/tars/" + sn + "(" + @stop.ToString("yyyy-MM-dd HH-mm-ss") + ")" + ".Tars"))
                {


                    sw.WriteLine("S{0}", sn);
                    sw.WriteLine("CITRON");
                    sw.WriteLine("NPLKWIM0T26B2PR1");
                    sw.WriteLine("PHOT_PRESS");
                    sw.WriteLine("Ooperator");
                    sw.WriteLine("Mheater1_temp");
                    sw.WriteLine("d" + temperatura_do_MES1);
                    sw.WriteLine("Mheater2_temp");
                    sw.WriteLine("d" + temperatura_do_MES2);

                    // sw.WriteLine("[" + start.Year + "-" + stop.Month + "-" + stop.Day + " " + stop.Hour + ":" + stop.Minute + ":" + stop.Second);
                    sw.WriteLine("[" + start.ToString("yyyy-MM-dd HH:mm:ss"));
                    sw.WriteLine("]" + stop_String);


                    sw.WriteLine("TP");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                port.Write("LOFF\r");
            }

            try
            {
                File.Copy(sourceFile, destinationFile, true);
            }
            catch (IOException iox)
            {
                MessageBox.Show(iox.Message);
            }


            return 1;


        }



        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                port.Write("LON\r");
            }
            catch
            {
                MessageBox.Show("Brak możlowości wyzwolenia skaneru", "Info", MessageBoxButtons.OK);
            }
          //  port.WriteLine("LON");
            //port.Write("4C4F4E");
            //port.WriteLine("4C4F4E");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(textBox2.Text.Contains("UTR"))
                UpdateControl(UTR, SystemColors.Control, "UTR", false);
            else
                UpdateControl(UTR, SystemColors.Control, "UTR", true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                port2.Write("@");
            }
            catch
            {
                MessageBox.Show("Błąd wyzwyolenia maszyny");
            }
            
        //    tworzeniepliku("A111111111111111110");
          //  Application.DoEvents();
        //    Test();
            //Thread.Sleep(5000);
            //UpdateControl(label3, Color.LawnGreen, "Skanowanie OK", false);
            //this.BackColor = SystemColors.Control;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                port.Write("LOFF\r");
            }
            catch
            {
                MessageBox.Show("Brak możlowości wyzwolenia skaneru", "Info", MessageBoxButtons.OK);
            }
        }

        private void label5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }





        const int M_NIENARODZONY = 1;
        const int M_BRAK_KROKU = 2;
        const int M_FAIL = 3;
        const int M_BRAK_POLACZENIA_Z_MES = 4;


        public int sprawdzanieMES(string SerialTxt)
        {
            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient("BoardsSoap"))
            {
                DataSet Result;
                try
                {
                    Result = wsMES.GetBoardHistoryDS(@"itron", SerialTxt);
                }
                catch
                {
                    return M_BRAK_POLACZENIA_Z_MES;
                }

                var Test = Result.Tables[0].TableName;
                if (Test != "BoardHistory") return M_NIENARODZONY; //numer produktu nie widnieje w systemie MES

                //where row.Field<string>("Test_Process").ToUpper() == "FVT / HOT_PRESS".ToUpper() || row.Field<string>("Test_Process").ToUpper() == "FVT / HOT_PRESS".ToUpper()
                var data = (from row in Result.Tables["BoardHistory"].AsEnumerable()
                            where row.Field<string>("Test_Process").ToUpper() == "QC / BB_GRN".ToUpper() || row.Field<string>("Test_Process").ToUpper() == "QC / BB_GRN".ToUpper()
                            select new
                            {
                                TestProcess = row.Field<string>("Test_Process"),
                                TestType = row.Field<string>("TestType"),
                                TestStatus = row.Field<string>("TestStatus"),
                                StartDateTime = row.Field<DateTime>("StartDateTime"),
                                StopDateTime = row.Field<DateTime>("StopDateTime"),
                            }).FirstOrDefault();


                if (data != null)
                {
                    //sprawdzamy PASS w poprzednim kroku
                    if ("PASS" == data.TestStatus.ToUpper()) return 0; //wszystko jest OK
                    else return M_FAIL;
                }
                else return M_BRAK_KROKU; //brak poprzedniego kroku
            }
        }




        private int sprawdzeniekrok(string sn)
        {
            int Result;

            Result = sprawdzanieMES(sn); //przykladowy numer seryjny 9100000668
            switch (Result)
            {
                case M_BRAK_POLACZENIA_Z_MES:
                  //  MessageBox.Show("Brak połączenia z MES.", "Info", MessageBoxButtons.OK);
                    UpdateControl(label2, Color.Red, "Brak połączenia z MES.", true);
                    break;

                case M_NIENARODZONY:
                 //   MessageBox.Show("Numer nienarodzony w MES.", "Info", MessageBoxButtons.OK);
                    UpdateControl(label2, Color.Red, "Numer nienarodzony w MES.", true);
                    break;

                case M_BRAK_KROKU:
                 //   MessageBox.Show("Brak poprzedniego kroku.", "Info", MessageBoxButtons.OK);
                    UpdateControl(label2, Color.Red, "Brak poprzedniego kroku.", true);
                    break;

                case M_FAIL:
                 //   MessageBox.Show("Poprzedni krok = FAIL.", "Info", MessageBoxButtons.OK);
                    UpdateControl(label2, Color.Red, "Poprzedni krok = FAIL.", true);
                    break;

                default:
                    //  MessageBox.Show("Wszystko jest OK", "Info", MessageBoxButtons.OK);                   
                    return 1;
            }
            return 0;
        }

    }
}
