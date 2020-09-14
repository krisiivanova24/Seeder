using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebsiteSearchPrices
{
    public partial class StartForm : Form
    {
        string myIP;
        String firstMacAddress;
        DBhelper dbheper = new DBhelper();
        public StartForm()
        {            
            try
            {
                InitializeComponent();
                if (CheckForInternetConnection())
                {
                    dbheper.InitializeDB();
                    if (dbheper.SELECTMacIP(GetMacAddress(), GetIPAddress()))
                    {
                        panel2.Visible = true;
                    }
                }
                else
                {
                    panel1.Show();
                }
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }

        }
        private string GetMacAddress()
        {
            try
            {
                firstMacAddress = NetworkInterface.GetAllNetworkInterfaces().Where(nic =>
                nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();
                return firstMacAddress;
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }
            return firstMacAddress;
        }
        private string GetIPAddress()
        {
            try
            {
                myIP = new System.Net.WebClient().DownloadString("https://api.ipify.org");
                return myIP;
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }
            return myIP;
        }

        private static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckForInternetConnection())
                {
                    panel1.Hide();
                    dbheper.InitializeDB();
                    if (dbheper.SELECTMacIP(GetMacAddress(), GetIPAddress()))
                    {
                        panel2.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text == "")
                {
                    MessageBox.Show($"Моля, първо въведете идентификационен номер!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (dbheper.HasSpecialChar(textBox1.Text))
                {
                    MessageBox.Show($"Моля, не въвеждайте специални знаци!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Text = "";
                }
                else if (dbheper.SELECTIDnumber(textBox1.Text) || !dbheper.SELECTValidUntil(textBox1.Text))
                {
                    dbheper.INSERTcompinfo(" ", GetMacAddress(), GetIPAddress());
                    textBox1.Text = "";
                    MessageBox.Show($"Не съществува такъв номер в системата или абонаментът е изтекъл. Моля, закупете нов!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }                
                else
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
                    ConfigurationManager.RefreshSection("connectionStrings");
                    config.ConnectionStrings.ConnectionStrings["MAC Address"].ConnectionString = GetMacAddress();
                    config.ConnectionStrings.ConnectionStrings["IP Address"].ConnectionString = GetIPAddress();
                    config.ConnectionStrings.ConnectionStrings["IDNumber"].ConnectionString = textBox1.Text;
                    config.Save();

                    if (!dbheper.SELECTMacIP(GetMacAddress(), GetIPAddress()))
                    {
                        dbheper.UPDATEcompinfo(textBox1.Text, GetMacAddress(), GetIPAddress());
                    }
                    else
                    {
                        dbheper.INSERTcompinfo(textBox1.Text, GetMacAddress(), GetIPAddress());
                    }
                    Form1 nextForm = new Form1();
                    this.Hide();
                    nextForm.ShowDialog();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkBox1.Checked == true)
                {
                    panel2.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }

        }
        private void StartForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // Use this since we are a WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                {  // Use this since we are a console app
                    System.Environment.Exit(1);
                }
            }
        }
    }
}
