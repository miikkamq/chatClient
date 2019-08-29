using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

       
       
        private const string CRLF = "\r\n";
        private const string localHost = "127.0.0.1";


        private TcpClient clientSocket;
        private IPAddress serverIpAddress;
        private int _port;
        private string name;


        public MainWindow()
        {            
            InitializeComponent();
            name = txtName.Text;
            btnConnect.IsEnabled = true;
            btnDisconnect.IsEnabled = false;
            btnSend.IsEnabled = false;
            OpenLogFile();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {

            // Create a TCP/IP socket.
          

            try
            {
                name = txtName.Text;

                serverIpAddress = CheckIP(txtIP.Text);
               _port =  CheckPort(txtPort.Text);  
          
                clientSocket = new TcpClient(serverIpAddress.ToString(), _port);
                Thread t = new Thread(ProcessClientTransactions);

                t.IsBackground = true;
                t.Start(clientSocket);

                btnConnect.IsEnabled = false;
                btnSend.IsEnabled = true;
                btnDisconnect.IsEnabled = true;
                txtName.IsEnabled = false;


                
            }
            catch
            {
                txtMessageScreen.Text +=  "Server is offline or connection input values are incorrect...." + CRLF;

            }


        }


        private void ProcessClientTransactions(object tcpClient)
        {
            TcpClient client = (TcpClient)tcpClient;
            string connectedName;
            string nameList;
            string disconnectedClient;
            string input = string.Empty;            
            StreamReader reader = null;
            StreamWriter writer = null;
           
            
            Dispatcher.BeginInvoke((Action)(() =>  name = txtName.Text));




            try { 
            
                writer = new StreamWriter(client.GetStream());
                reader = new StreamReader(client.GetStream());




                // tell the server we are connected
                writer.WriteLine("Connected" +  name);
                writer.Flush();


                // while the connection is still alive
                while (client.Connected)
                {
                 
                    input = reader.ReadLine(); // when we receive something from server

                    if (input == null)
                    {                
                      Disconnect();
                    }
                    else
                    {
                      
                                    // if someone connects
                                    if (input.Substring(0, 5) == "$conn")
                                    {
                                        connectedName = input.Substring(5);
                                        Dispatcher.BeginInvoke((Action)(() => listBox_connectedClients.Items.Add(connectedName)));
                                        Dispatcher.BeginInvoke((Action)(() => txtMessageScreen.AppendText(connectedName + " has connected to the server" + CRLF)));
                                     
                                    }

                                    // For online list
                                    else if (input.Substring(0, 5) == "#list")
                                    {
                                        nameList = input.Substring(5);
                                        Dispatcher.BeginInvoke((Action)(() => listBox_connectedClients.Items.Add(nameList)));
                             
                                    }

                                    // If someone disconnects
                                    else if (input.Substring(0, 5) == "£disc")
                                    {
                                        disconnectedClient = input.Substring(5);
                                        Dispatcher.BeginInvoke((Action)(() => listBox_connectedClients.Items.Remove(disconnectedClient)));
                                        Dispatcher.BeginInvoke((Action)(() => txtMessageScreen.AppendText(disconnectedClient + " has disconnected from the server" + CRLF)));
                           
                                    }
                                    else
                                    {

                                        //For normal messages
                                        Dispatcher.BeginInvoke((Action)(() => txtMessageScreen.AppendText(input + CRLF)));

                                    }
                                   
                               }
          
                                   
                                
                     
                    


                }
            }
            catch 
            {
  
               //     Dispatcher.BeginInvoke((Action)(() => txtMessageScreen.AppendText("Problem communicating with the server." + CRLF)));

            }
            Dispatcher.BeginInvoke((Action)(() => listBox_connectedClients.Items.Clear()));
            Dispatcher.BeginInvoke((Action)(() => txtName.IsEnabled = true));
            Dispatcher.BeginInvoke((Action)(() => btnDisconnect.IsEnabled = false));
            Dispatcher.BeginInvoke((Action)(() => btnConnect.IsEnabled = true)); 

        }


        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
           
            btnDisconnect.IsEnabled = false;
            btnConnect.IsEnabled = true;
            btnSend.IsEnabled = false;
            txtName.IsEnabled = true;
            //Close connection   
            Disconnect();
                

        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {

               // if the message box is not empty
            if (txtMessage.Text != "")
            {
               

                try
                {



                    if (clientSocket.Connected)
                    {
                        StreamWriter writer = new StreamWriter(clientSocket.GetStream());
                        writer.WriteLine("<" + txtName.Text + "> " + txtMessage.Text);
                        writer.Flush();
                        txtMessage.Clear();
                    }
                }
                catch (Exception ex)
                {
                    txtMessageScreen.Text += ex;
                }
            }
        }



    




        private void Disconnect()
        {

            try
            {
                //close socket
                clientSocket.Close();
               
                listBox_connectedClients.Items.Clear();
                Dispatcher.BeginInvoke((Action)(() => txtMessageScreen.AppendText( "Disconnected from the server!" + CRLF)));


                Dispatcher.BeginInvoke((Action)(() => btnDisconnect.IsEnabled = false));
                Dispatcher.BeginInvoke((Action)(() => btnConnect.IsEnabled = true));
                Dispatcher.BeginInvoke((Action)(() => btnSend.IsEnabled = false));
       
            }
            catch 
            {
                listBox_connectedClients.Items.Clear();
              


            }
            listBox_connectedClients.Items.Clear();
        

        }


        private IPAddress CheckIP(string ipCheck)
        {
            IPAddress address = IPAddress.Parse(localHost);

            // check if the ip textbox is empty
                if(txtIP.Text == "")
                {
                txtMessageScreen.Text += "IP Address is missing." + CRLF;
                address = null;
                }
          
                else if (!IPAddress.TryParse(ipCheck, out address))
                {
                    address = IPAddress.Parse(localHost);
                }
            
           

            return address;

        }


        private int CheckPort(string portCheck)
        {
           int tryport = 0;

           if (txtPort.Text == "")
            {
                txtMessageScreen.Text += "Port value is missing." + CRLF;
            
            }
           // check if not numeric value
           else if(!Int32.TryParse(txtPort.Text, out tryport))
            {
                txtMessageScreen.Text += "Port should be numeric value." + CRLF;
             
            }

            return tryport;
        }

     

        private void MenuItemWhisper_Click(object sender, RoutedEventArgs e)
        {

            if (listBox_connectedClients.SelectedIndex == -1)
            {
                return;
            }

            else
            {
                // For whispering
                Whisper();
            }

        }

        private void Whisper()
        {
            // Coming soon......
        }


        private void OpenLogFile()
        {
            string path = "logfile.txt";
            // Open the logfile when application starts

            // If does not exist, create a new file
            if (!File.Exists(path))
            {
                File.Create(path);

            }

            // Else just read the existing file
            else
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s == "")
                        {
                            txtMessageScreen.AppendText(s);

                        }
                        else
                        {
                            txtMessageScreen.AppendText(s + CRLF);

                        }

                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string path = "logfile.txt";
            // Write all messages to logfile

            string appendText = txtMessageScreen.Text;
            File.WriteAllText(path, appendText);

        }

        private void BtnClearScreen_Click(object sender, RoutedEventArgs e)
        {
            // for clearing the screen
            txtMessageScreen.Clear();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // when the app starts
            txtMessageScreen.ScrollToEnd();
        }

        private void TxtMessageScreen_TextChanged(object sender, TextChangedEventArgs e)
        {
            // If text changes
            txtMessageScreen.ScrollToEnd();
        }
    }
}
