using K4os.Compression.LZ4.Internal;
using MySql.Data.MySqlClient;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace WebsiteSearchPrices
{
    public partial class Form1 : Form, IDisposable
    {
        #region MEMBERS
        private MySqlConnection connection;
        private string URLUpdate = null;
        private int hourTimer = 1;
        private List<Sites> Sites = new List<Sites>();
        private string today = null;
        private DBhelper dbhelper;
        private string idnumber;
        List<string> emails = new List<string>();
        List<string>sites = new List<string>();
        Dictionary<string, string> allUrls = new Dictionary<string, string>();
        private System.Timers.Timer timer;
        private System.Timers.Timer timer2;
        Global globalItems;
        #endregion

        #region FORMLOAD
        public Form1()
        {            
            try
            {
                InitializeComponent();
                InitializeButtonsAccess();
                idnumber = ConfigurationManager.ConnectionStrings["IDNumber"].ConnectionString;
                dbhelper = new DBhelper();
                emails = dbhelper.SELECTemails(idnumber);
                globalItems = new Global(emails);
                InitializeDB();
                dbhelper.InitializeDB();
                MySQL_ToDatagridview();
                TakeInfo();
                sites = dbhelper.SELECTsites(idnumber).ToList();

                timer = new System.Timers.Timer();
                timer.Interval = 1000 * hourTimer * 60 * 60;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                timer.Start();

                timer2 = new System.Timers.Timer();
                timer2.Interval = 1000 * 5 * 60;
                timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer2_Elapsed);
                timer2.Start();

                foreach (var site in sites)
                {
                    comboBox1.Items.Add(site);
                }
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }
        }

        private void InitializeButtonsAccess()
        {
            panel7.Visible = false;
            pictureBox1.Controls.Add(pictureBox2);
            pictureBox2.BackColor = Color.Transparent;
            pictureBox1.Controls.Add(pictureBox3);
            pictureBox3.BackColor = Color.Transparent;
            pictureBox1.Controls.Add(pictureBox4);
            pictureBox4.BackColor = Color.Transparent;
            pictureBox1.Controls.Add(panel2);
            panel2.BackColor = Color.Transparent;
            pictureBox1.Controls.Add(label18);
            label18.BackColor = Color.Transparent;
            pictureBox1.Controls.Add(label19);
            label19.BackColor = Color.Transparent;
        }
        #endregion
        void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!dbhelper.SELECTValidUntil(idnumber) || dbhelper.SELECTclosetapp(idnumber))
            {
                Application.Exit();
            }
            else if(dbhelper.SELECTrestartapp(idnumber))
            {
                RestartProgram();
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!dbhelper.SELECTValidUntil(idnumber) || dbhelper.SELECTclosetapp(idnumber))
            {
                Application.Exit();                
            }
            else if (dbhelper.SELECTrestartapp(idnumber))
            {
                RestartProgram();
            }
            else
            {
                MyMethodAsync();
            }
        }
        private void RestartProgram()
        {
            // Get file path of current process 
            var filePath = Assembly.GetExecutingAssembly().Location;
            //var filePath = Application.ExecutablePath;  // for WinForms

            // Start program
            Process.Start(filePath);

            // For Windows Forms app
            Application.Exit();

            // For all Windows application but typically for Console app.
            //Environment.Exit(0);
        }
        private async Task MyMethodAsync()
        {
            MySQL_ToDatagridview();
            if (Sites.Count != 0)
            {
                foreach (var item in this.Sites)
                {

                    if (item.SiteType == "Mebelino")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split(' ', '>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            int n = list.IndexOf("class=\"price-group\"");
                            string price = list[n + 4] + "лв.";

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "MebeliBG")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split(' ', '>').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("class=\"ty-price-num\"");
                            price = list[n + 1].Replace("</span", "лв.");

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Mebelilazur")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("цена");
                            price = list[n + 6] + "лв.";

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Mebelilargo")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split(' ', '>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("price-new-js\"");
                            price = list[n + 1] + "лв.";

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Mebelisto")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split('\"', '>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("woocommerce-Price-amount amount");
                            price = list[n + 1] + "лв.";

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Videnov")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split('\"', '>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("product-price");
                            price = list[n + 1].Trim();

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Mondomebeli")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split(' ', '>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (list[i] == "class=\"price-wrapper\"")
                                    if (list[i + 2] == "class=\"price\"")
                                        price = list[i + 8] + "," + list[i + 11] + "лв.".Trim();
                            }
                            if (price == "0")
                            {
                                int n = list.IndexOf("class=\"price\"");
                                price = list[n + 2] + "," + list[n + 5] + "лв.".Trim();
                            }
                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Mebelizona")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("span id=\"our_price_display\"");
                            price = list[n + 1].Trim();

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Mebidea")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            //WebClient client = new WebClient();
                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("div class=\"blockElement itemPriceBlock\"");
                            price = list[n + 3] + "лв.".Trim();

                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Aiko-bg")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split(' ', '>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            list.RemoveAll(x => x == "\n");
                            string price = "0";

                            int n = list.IndexOf("id=\"productPrice\"");
                            for (int i = n; i < n + 50; i++)
                            {
                                if (list[i] == "id=\"price\"")
                                {
                                    price = list[n + 5] + "лв.".Trim();
                                }
                                else if (list[i] == "id=\"promo_price\"")
                                {
                                    price = list[n + 13] + "лв.".Trim();
                                }
                            }
                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Dizmabg")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("div class=\"price\"");
                            price = list[n + 3].Trim();
                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Ivelibg")
                    {
                        string content = null;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                client.Encoding = System.Text.Encoding.UTF8;
                                content = await client.DownloadStringTaskAsync(item.Url);
                            }

                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("span class=\"_product-details-price-new price-new-js rtl-ltr\"");
                            price = list[n + 1].Trim();
                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                    }
                    else if (item.SiteType == "Krezbg")
                    {
                        try
                        {
                            WebClient client = new WebClient();
                            client.Encoding = System.Text.Encoding.UTF8;
                            string content = client.DownloadString(item.Url);
                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("span class=\"_product-details-price-new price-new-js rtl-ltr\"");
                            price = list[n + 1].Trim();
                            client.Dispose();
                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception ex)
                        {
                            Global.SendError(ex);
                        }
                    }
                    else if (item.SiteType == "Kamkobg")
                    {
                        try
                        {
                            WebClient client = new WebClient();
                            client.Encoding = System.Text.Encoding.UTF8;
                            string content = client.DownloadString(item.Url);
                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("span class=\"woocommerce-Price-amount amount\"");
                            if (list[n - 1] == "del")
                            {
                                for (int i = n + 1; i < n + 20; i++)
                                {
                                    if (list[i] == "span class=\"woocommerce-Price-amount amount\"")
                                    {
                                        price = list[i + 1].Replace("&nbsp;", "лв.").Trim();
                                    }
                                }
                            }
                            else
                            {
                                price = list[n + 1].Replace("&nbsp;", "лв.").Trim();
                            }
                            client.Dispose();
                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                            content = String.Empty;
                        }
                        catch (Exception ex)
                        {
                            Global.SendError(ex);
                        }
                    }
                    else if (item.SiteType == "Ikeabg")
                    {
                        try
                        {
                            WebClient client = new WebClient();
                            client.Encoding = System.Text.Encoding.UTF8;
                            string content = client.DownloadString(textBox1.Text);
                            List<string> list = content.Split('>', '<').ToList();
                            list.RemoveAll(x => x.ToString() == "");
                            string price = "0";
                            int n = list.IndexOf("span class=\"price \"");
                            price = list[n + 1].Trim();
                            client.Dispose();
                            content = String.Empty;
                            if (item.Price != price)
                            {
                                try
                                {
                                    globalItems.SendEmailWithChange(item, price);

                                    item.Price = price;
                                    updatePrice(item.SpecialId, item.Url, price);
                                    MySQL_ToDatagridview();
                                }
                                catch (Exception e)
                                {
                                    Global.SendError(e);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Global.SendError(ex);
                        }
                    }
                }
            }
            try
            {
                this.label12.Text = "Последна проверка: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                dbhelper.CompNewUPDATE();
                TakeInfo();
                MySQL_ToDatagridview();
                var memory = 0.0;
                using (Process proc = Process.GetCurrentProcess())
                {
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                }
                if (memory >= 500)
                {
                    Application.Restart();
                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }
        }

        private void updatePrice(string specialid, string url, string price)
        {
            try
            {
                string query = $"UPDATE SitePrice.Price SET price='{price}', date = '{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}' WHERE url='{url}'";

                //Open connection
                if (this.OpenConnection() == true)
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                dbhelper.INSERTnewChangePrice(specialid, price);
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
            finally
            {
                //close connection
                this.CloseConnection();
            }
        }

        private void TakeInfo()
        {
            try
            {
                string query = $"SELECT * FROM SitePrice.info where idnumber = '{idnumber}'";

                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        textBox4.Text = dataReader["timerinfo"].ToString();
                        try
                        {
                            this.hourTimer = int.Parse(textBox4.Text);
                        }
                        catch (Exception e)
                        {
                            Global.SendError(e);
                        }
                        if (dataReader["hasnewupdate"].ToString() == "true")
                        {
                            try
                            {
                                label5.Visible = true;
                                this.URLUpdate = dataReader["UpdateURL"].ToString();
                            }
                            catch (Exception e)
                            {
                                Global.SendError(e);
                            }
                        }
                        else label5.Visible = false;
                    }

                    //close Data Reader
                    dataReader.Close();
                    cmd.Dispose();
                    dataReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
            finally
            {
                //close connection
                this.CloseConnection();
            }
        }

        private void InitializeDB()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectDatabase"].ConnectionString;
                connection = new MySqlConnection(connectionString);
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }
        }

        private void MySQL_ToDatagridview()
        {
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlDataAdapter MyDA = new MySqlDataAdapter();
                    string sqlSelectAll = $"SELECT name as `Заглавие`,site as `Тип`,price as `Сегашна цена`, date as `Дата`, specialid as `Специален номер` from Price where idnumber = '{idnumber}'";
                    MyDA.SelectCommand = new MySqlCommand(sqlSelectAll, connection);
                    string command = $"SELECT * FROM SitePrice.Price where idnumber = '{idnumber}'";
                    DataTable table = new DataTable();
                    MyDA.Fill(table);

                    BindingSource bSource = new BindingSource();
                    bSource.DataSource = table;

                    MySqlCommand cmd = new MySqlCommand(command, connection);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        if (!Sites.Any(x => x.Url == dataReader["url"].ToString()))
                            this.Sites.Add(new Sites(dataReader["name"].ToString(), dataReader["url"].ToString(), dataReader["site"].ToString(), dataReader["price"].ToString(), dataReader["date"].ToString(), dataReader["specialid"].ToString()));
                    }

                    //close Data Reader
                    dataReader.Close();
                    
                    dataGridView1.DataSource = bSource;
                    this.dataGridView1.Columns[4].Visible = false;
                    Control.CheckForIllegalCrossThreadCalls = false; //ne znam dali go ima no ne dava greshka
                }
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }
            finally
            {
                //close connection
                this.CloseConnection();
            }
        }
        private void button1_Click(object sender, EventArgs e)//Checkwebsite button
        {
            try
            {
                Process.Start("chrome.exe", textBox1.Text);
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)//ask button
        {
            if (textBox2.Text != "")
            {
                try
                {
                    SmtpClient client1 = new SmtpClient();
                    client1.Port = 587;
                    client1.Host = "smtp.gmail.com";
                    client1.EnableSsl = true;
                    client1.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client1.UseDefaultCredentials = false;
                    client1.Credentials = new System.Net.NetworkCredential(ConfigurationManager.ConnectionStrings["EmailAdmin"].ConnectionString,
                            ConfigurationManager.ConnectionStrings["PasswordAdmin"].ConnectionString);

                    MailMessage mm1 = new MailMessage("donotreply@domain.com", $"ivoradev14@gmail.com", $"Съобщение по проект", $"{textBox2.Text}");
                    mm1.BodyEncoding = UTF8Encoding.UTF8;
                    mm1.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    client1.Send(mm1);

                    client1.Dispose();
                    MessageBox.Show("Съобщението Ви е успешно изпратено до разработчих!", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox2.Text = null;
                }
                catch (Exception ex)
                {
                    Global.SendError(ex);
                }
            }
            else
            {
                MessageBox.Show($"Моля, първо въведете съобщение!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void label5_Click(object sender, EventArgs e)//Download new version
        {
            try
            {
                Process.Start("chrome.exe", this.URLUpdate);
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != "" && textBox1.Text != "")
            {
                try
                {
                    if (label8.Text != "0" && textBox1.Text != "" && comboBox1.Text != "" && textBox3.Text != "")
                    {
                        string specialId = dbhelper.GenerateIDnumber();
                        string query = $"INSERT INTO Price (name, url, site, price, date, idnumber, specialid) VALUES('{Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(textBox3.Text))}', '{textBox1.Text}', '{comboBox1.Text}', '{Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(label8.Text))}','{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}', '{idnumber}', '{specialId}')";
                        //open connection
                        if (this.OpenConnection() == true)
                        {
                            //create command and assign the query and connection from the constructor
                            MySqlCommand cmd = new MySqlCommand(query, connection);

                            //Execute command
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                        }
                        MessageBox.Show("Успешно добавихте новия линк!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.CloseConnection();
                        MySQL_ToDatagridview();
                        dbhelper.INSERTnewChangePrice(specialId, label8.Text);
                        textBox1.Text = "";
                        comboBox1.Text = "Избери сайт";
                        textBox3.Text = "";
                        label8.Text = "0";
                    }
                    else
                    {
                        MessageBox.Show("Моля, проверете дали не сте изпуснали въвеждането на някои данни!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    Global.SendError(ex);
                }
                finally
                {
                    //close connection
                    this.CloseConnection();
                }
            }
            else
            {
                MessageBox.Show($"Моля, първо въведете съобщение!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Mebelino()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split(' ', '>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                int n = list.IndexOf("class=\"price-group\"");
                label8.Text = list[n + 4] + Encoding.UTF8.GetString(Encoding.UTF8.GetBytes("лв."));
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void MebeliBG()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split(' ', '>').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("class=\"ty-price-num\"");
                price = list[n + 1];
                //428</span
                this.label8.Text = price.Replace("</span", "лв.");
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Мebelilazur()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.ToLower().Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("цена");
                price = list[n + 6] + "лв.";
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Mebelilargo()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split(' ', '>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("price-new-js\"");
                price = list[n + 1] + "лв.";
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Mebelisto()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('\"', '>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("woocommerce-Price-amount amount");
                price = list[n + 1] + "лв.";
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Videnov()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('\"', '>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("product-price");
                price = list[n + 1].Trim();
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Mondomebeli()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split(' ', '>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == "class=\"price-wrapper\"")
                        if (list[i + 2] == "class=\"price\"")
                            price = list[i + 8] + "," + list[i + 11] + "лв.".Trim();
                }
                if (price == "0")
                {
                    int n = list.IndexOf("class=\"price\"");
                    price = list[n + 2] + "," + list[n + 5] + "лв.".Trim();
                }
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Mebelizona()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("span id=\"our_price_display\"");
                price = list[n + 1].Trim();
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Mebidea()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("div class=\"blockElement itemPriceBlock\"");
                price = list[n + 3] + "лв.".Trim();
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Dizmabg()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("div class=\"price\"");
                price = list[n + 3].Trim();
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Ivelibg()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("span class=\"_product-details-price-new price-new-js rtl-ltr\"");
                price = list[n + 1].Trim();
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Kamkobg()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("span class=\"woocommerce-Price-amount amount\"");
                if (list[n - 1] == "del")
                {
                    for (int i = n + 1; i < n + 20; i++)
                    {
                        if (list[i] == "span class=\"woocommerce-Price-amount amount\"")
                        {
                            price = list[i + 1].Replace("&nbsp;", "лв.").Trim();
                        }
                    }
                }
                else
                {
                    price = list[n + 1].Replace("&nbsp;", "лв.").Trim();
                }
                client.Dispose();
                content = String.Empty;
                this.label8.Text = price;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Ikeabg()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);//greshka tuk
                List<string> list = content.Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("span class=\"price \"");
                price = list[n + 1].Trim();
                client.Dispose();
                content = String.Empty;
                this.label8.Text = price;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Krezbg()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split('>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                string price = "0";
                int n = list.IndexOf("span class=\"_product-details-price-new price-new-js rtl-ltr\"");
                price = list[n + 1].Trim();
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void Aiko_bg()
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split(' ', '>', '<').ToList();
                list.RemoveAll(x => x.ToString() == "");
                list.RemoveAll(x => x == "\n");
                string price = "0";
                int n = list.IndexOf("id=\"productPrice\"");
                for (int i = n; i < n + 50; i++)
                {
                    if (list[i] == "id=\"price\"")
                    {
                        price = list[n + 5] + "лв.".Trim();
                    }
                    else if (list[i] == "id=\"promo_price\"")
                    {
                        price = list[n + 13] + "лв.".Trim();
                    }
                }
                this.label8.Text = price;
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void emag()
        {
            try
            {
                WebClient client = new WebClient();
                string content = client.DownloadString(textBox1.Text);
                List<string> list = content.Split(' ').ToList();
                list.RemoveAll(x => x.ToString() == "");
                List<string> prices = new List<string>();

                int n = list.IndexOf("product-page-pricing\"><div");

                for (int i = n; i < n + 24; i++)
                {
                    if (list[i].Contains("лв.") || list[i].Contains("Р»РІ."))//Р»РІ.
                    {
                        prices.Add(list[i - 1]);
                    }
                }

                if (prices.Count > 1)
                    prices.RemoveAt(0);

                string price = prices[0];
                //1&#46;299<sup>99</sup>
                if (price.Contains("&#46;"))
                {
                    price = price.Replace("&#46;", ".");
                    price = price.Replace("<sup>", ",");
                    price = price.Replace("</sup>", "");
                    this.label8.Text = price + "лв.";
                }
                else
                {
                    price = price.Replace("<sup>", ",");
                    price = price.Replace("</sup>", "");
                    this.label8.Text = price + Encoding.UTF8.GetString(Encoding.UTF8.GetBytes("лв."));
                }
                client.Dispose();
                content = String.Empty;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
        }
        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty)
                return;
            try
            { 
                if (comboBox1.Text == "Mebelino")
                {
                    Mebelino();
                }
                else if (comboBox1.Text == "Ikeabg")
                {
                    Ikeabg();
                }
                else if (comboBox1.Text == "MebeliBG")
                {
                    MebeliBG();
                }
                else if (comboBox1.Text == "Мebelilazur")
                {
                    Мebelilazur();
                }
                else if (comboBox1.Text == "Mebelilargo")
                {
                    Mebelilargo();
                }
                else if (comboBox1.Text == "Mebelisto")
                {
                    Mebelisto();
                }
                else if (comboBox1.Text == "Krezbg")
                {
                    Krezbg();
                }
                else if (comboBox1.Text == "Kamkobg")
                {
                    Kamkobg();
                }
                else if (comboBox1.Text == "Dizmabg")
                {
                    Dizmabg();
                }
                else if (comboBox1.Text == "Videnov")
                {
                    Videnov();
                }
                else if (comboBox1.Text == "Ivelibg")
                {
                    Ivelibg();
                }
                else if (comboBox1.Text == "Mebidea")
                {
                    Mebidea();
                }
                else if (comboBox1.Text == "Mebelizona")
                {
                    Mebelizona();
                }
                else if (comboBox1.Text == "Mondomebeli")
                {
                    Mondomebeli();
                }
                else if (comboBox1.Text == "Aiko-bg")
                {
                    Aiko_bg();
                }
                else if (comboBox1.Text == "eMag")
                {
                    emag();
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }

        }
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Няма връзка със сървъра. Моля, свържете се с администартор!");
                        break;

                    case 1045:
                        MessageBox.Show("Некоректни данни. Моля, опитайте отново!");
                        break;
                }
                Global.SendError(ex);
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Global.SendError(ex);
                return false;
            }
        }

        private void button4_Click(object sender, EventArgs e)//settimer ///////////////////////////
        {
            try
            {
                if (textBox4.Text != "" && !dbhelper.HasSpecialChar(textBox4.Text))
                {
                    string query = $"UPDATE SitePrice.info SET timerinfo='{textBox4.Text}' WHERE idnumber ='{idnumber}'";
                    //Open connection
                    if (this.OpenConnection() == true)
                    {
                        //create mysql command
                        MySqlCommand cmd = new MySqlCommand();
                        //Assign the query using CommandText
                        cmd.CommandText = query;
                        //Assign the connection using Connection
                        cmd.Connection = connection;

                        //Execute query
                        cmd.ExecuteNonQuery();
                        try
                        {
                            this.hourTimer = int.Parse(textBox4.Text);
                        }
                        catch (Exception ex)
                        {
                            Global.SendError(ex);
                        }
                        MessageBox.Show("Часовете са успешно променени!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        cmd.Dispose();
                        timer.Interval = 1000 * hourTimer * 60 * 60;
                    }

                }
                else
                {
                    MessageBox.Show($"Моля, не въвеждайте специални знаци и не натискайте бутона преди да въведете часове!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox4.Text = "";
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
            finally
            {
                //close connection
                this.CloseConnection();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            comboBox1.Text = "Избери сайт";
            if (dbhelper.HasSpecialChar(textBox1.Text))
            {
                MessageBox.Show($"Моля, не въвеждайте специални знаци!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Text = "";
            }
        }

        private void button5_Click(object sender, EventArgs e)//info
        {
            panel4.Visible = true;
            panel4.BringToFront();
            panel6.SendToBack();
            panel5.SendToBack();
            panel7.SendToBack();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
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

        private void button14_Click(object sender, EventArgs e)
        {
            panel5.Visible = true;
            panel5.BringToFront();
            panel6.SendToBack();
            panel4.SendToBack();
            panel7.SendToBack();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            panel7.Visible = true;
            panel7.BringToFront();
            panel6.SendToBack();
            panel4.SendToBack();
            panel5.SendToBack();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            panel6.Visible = true;
            panel6.BringToFront();
            panel5.SendToBack();
            panel4.SendToBack();
            panel7.SendToBack();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            panel6.Visible = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            panel7.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            panel5.Visible = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox5.Text == "")
                {
                    MessageBox.Show($"Моля, първо въведете имейл!", "Email not sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (dbhelper.HasSpecialChar(textBox5.Text))
                {
                    MessageBox.Show($"Моля, въведете валиден имейл без специални знаци!", "Email not sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox5.Text = "";
                }
                else if (!dbhelper.SELECTValidUntil(ConfigurationManager.ConnectionStrings["IDNumber"].ConnectionString))
                {
                    MessageBox.Show($"Абонаментът Ви е изтекъл! Моля, закупете нов!", "Email not sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox5.Text = "";
                }
                else
                {
                    dbhelper.INSERTnewemail(idnumber, textBox5.Text);
                    MessageBox.Show($"Имейлът е успешно добавен!", "Email sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox5.Text = "";
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
            //dobavi
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                string query = $"delete from SitePrice.Price WHERE specialid ='{dataGridView1.CurrentRow.Cells[4].Value.ToString()}'";
                //Open connection
                if (this.OpenConnection() == true)
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
            finally
            {
                //close connection
                this.CloseConnection();
            }
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                dataGridView1.Rows.RemoveAt(row.Index);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (dbhelper.HasSpecialChar(textBox3.Text))
            {
                MessageBox.Show($"Моля, не въвеждайте специални знаци!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox3.Text = "";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (dbhelper.HasSpecialChar(textBox2.Text))
            {
                MessageBox.Show($"Моля, не въвеждайте специални знаци!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox2.Text = "";
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (dbhelper.HasSpecialChar(textBox4.Text))
            {
                MessageBox.Show($"Моля, не въвеждайте специални знаци!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox4.Text = "";
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (dbhelper.HasSpecialChar(textBox5.Text))
            {
                MessageBox.Show($"Моля, не въвеждайте специални знаци!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox5.Text = "";
            }
        }
    }
}
