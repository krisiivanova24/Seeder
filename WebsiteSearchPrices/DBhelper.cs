using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebsiteSearchPrices
{
    public class DBhelper
    {
        #region MEMBERS
        private MySqlConnection connection;
        private string today = null;
        private List<string> macList = new List<string>();
        private List<string> IPList = new List<string>();
        private List<string> siteList = new List<string>();
        private List<string> IDnumberList = new List<string>();
        private bool whetherIsNew;
        private DateTime validuntil;
        private string check;
        private List<string> invalidChars = new List<string>() { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "<", ">", ",", "\"", "\'", "+", "=", "}", "{", "]", "[" };
        #endregion

        #region INITIALIZE
        public DBhelper()
        {
            InitializeDB();
        }

        public void InitializeDB()
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
                        MessageBox.Show("Няма връзка със сървъра. Моля, свържете се с администратор!");
                        break;

                    case 1045:
                        MessageBox.Show("Невалидни данни. Моля, опитайте отново!");
                        break;
                }
                Global.SendError(ex);
                return false;
            }
        }

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

        #endregion

        #region UPDATE
        public void UPDATEcompinfo(string id, string thisMACaddess, string thisIPaddress)
        {
            try
            {
                var memory = 0.0;
                using (Process proc = Process.GetCurrentProcess())
                {
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                }
                string query = $"UPDATE comp SET idnumber = '{id}',  lastlogin='{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}'  WHERE ipaddress='{thisIPaddress}' and macaddress = '{thisMACaddess}'";

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
        }
        public void CompNewUPDATE()
        {
            try
            {
                var memory = 0.0;
                using (Process proc = Process.GetCurrentProcess())
                {
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                }
                string query = $"UPDATE comp SET ramuse='{memory}', lastcheck='{DateTime.Now}' WHERE macaddress='{ConfigurationManager.ConnectionStrings["MAC Address"].ConnectionString}'";

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
        }

        #endregion

        #region INSERT

        public void INSERTnewChangePrice(string specialId, string newPrice)
        {
            try
            {
                this.today = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

                string query = $"INSERT INTO SitePrice.changes (specialid, updateprice, changedate) VALUES('{specialId}', '{newPrice}', '{this.today}')";

                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery();
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

        public void INSERTcompinfo(string thisIDnumber, string thisMACaddess, string thisIPaddress) //works
        {
            try
            {
                this.today = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                var memory = 0.0;
                using (Process proc = Process.GetCurrentProcess())
                {
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                }
                string query = $"INSERT INTO comp (lastlogin, ramuse, lastcheck, idnumber, macaddress, ipaddress) VALUES('{today}', '{memory}', '{0}', '{thisIDnumber}', '{thisMACaddess}', '{thisIPaddress}')";

                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery();
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

        public void INSERTnewemail(string thisIDnumber, string email)
        {
            try
            {
                string query = $"INSERT INTO emails (idnumber, email) VALUES ('{thisIDnumber}', '{email}')";

                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery();
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
        #endregion

        #region SELECT
        public List<string> SELECTemails(string idnumber)
        {
            List<string> emails = new List<string>();
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand($"SELECT email FROM emails where idnumber= '{idnumber}';", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        emails.Add(reader["email"].ToString());
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
            return emails;
        }

        public bool SELECTValidUntil(string thisIDnumber)
        {
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand($"SELECT validuntil FROM abonaments where idnumber= '{thisIDnumber}';", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        List<string> validList = reader.GetValue(0).ToString().Split('.').ToList();
                        //validuntil = DateTime.Parse(reader.GetValue(0).ToString());
                        validuntil = new DateTime(int.Parse(validList[2]), int.Parse(validList[1]), int.Parse(validList[0]));
                    }
                    reader.Close();
                }
            }

            catch (Exception e)
            {
                Global.SendError(e);
            }
            finally
            {
                this.CloseConnection();
            }
            if (validuntil >= DateTime.Parse(DateTime.Now.ToString()))
            {
                whetherIsNew = true;
            }
            else
            {
                whetherIsNew = false;
            }
            return whetherIsNew;
            /*try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand($"SELECT validuntil FROM abonaments where idnumber= '{thisIDnumber}';", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        string format = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern;
                        string result = DateTime.ParseExact(reader["validuntil"].ToString(), "dd.MM.yyyy",
                        CultureInfo.InvariantCulture).ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern);
                        validuntil = DateTime.Parse(result.ToString());
                    }
                    reader.Close();
                }
            }

            catch (Exception e)
            {
                Global.SendError(e);
            }
            finally
            {
                this.CloseConnection();
            }
            if (validuntil >= DateTime.Now)
            {
                whetherIsNew = true;
            }
            else
            {
                whetherIsNew = false;
            }
            return whetherIsNew;*/
        }

        public bool SELECTIDnumber(string thisIDnumber) //works
        {
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand("SELECT idnumber FROM abonaments;", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        IDnumberList.Add(reader.GetValue(0).ToString());
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Global.SendError(e);
            }
            finally
            {
                this.CloseConnection();
            }
            if (IDnumberList.Contains(thisIDnumber))
            {
                whetherIsNew = false;
            }
            else
            {
                whetherIsNew = true;
            }
            return whetherIsNew;
        }

        public List<string> SELECTsites(string thisidnumber)
        {
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand($"SELECT sitename FROM SitePrice.sites where idnumber = '{thisidnumber}';", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        siteList.Add(reader.GetValue(0).ToString());
                    }
                    reader.Close();
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
            return siteList;
        }

        public bool SELECTrestartapp(string thisdnumber)
        {
            whetherIsNew = false;
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand($"SELECT restartapp FROM restart where idnumber = '{thisdnumber}';", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        check = reader.GetValue(0).ToString();
                    }
                    reader.Close();
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
            if (check == "1")
            {
                whetherIsNew = true;
            }
            else
            {
                whetherIsNew = false;
            }
            return whetherIsNew;
        }

        public bool SELECTclosetapp(string thisdnumber)
        {
            whetherIsNew = false;
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand($"SELECT closeapp FROM restart where idnumber = '{thisdnumber}';", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        check = reader.GetValue(0).ToString();
                    }
                    reader.Close();
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
            if (check == "1")
            {
                whetherIsNew = true;
            }
            else
            {
                whetherIsNew = false;
            }
            return whetherIsNew;
        }

        public bool SELECTMac(string thisMac)
        {
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand("SELECT macaddress FROM comp;", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        macList.Add(reader.GetValue(0).ToString());
                    }
                    reader.Close();
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
            if (macList.Contains(thisMac))
            {
                whetherIsNew = false;
            }
            else
            {
                whetherIsNew = true;
            }
            return whetherIsNew;
        }

        public bool SELECTMacIP(string thisMac, string thisIP) //works
        {
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand("SELECT macaddress FROM comp;", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        macList.Add(reader.GetValue(0).ToString());
                    }
                    reader.Close();
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
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand comm = new MySqlCommand("SELECT ipaddress FROM comp;", connection);
                    MySqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        IPList.Add(reader.GetValue(0).ToString());
                    }
                    reader.Close();
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
            if (macList.Contains(thisMac) && IPList.Contains(thisIP))
            {
                whetherIsNew = false;
            }
            else
            {
                whetherIsNew = true;
            }
            return whetherIsNew;
        }
        #endregion

        public string GenerateIDnumber() => Guid.NewGuid().ToString();

        public bool HasSpecialChar(string input)
        {
            whetherIsNew = false;
            try
            {
                string specialChar = @"\""\|*'";
                foreach (var item in specialChar)
                {
                    if (input.Contains(item))
                    { whetherIsNew = true; break; }
                }

                return whetherIsNew;
            }
            catch (Exception ex)
            {
                Global.SendError(ex);
            }
            return whetherIsNew;
        }

    }
}
