using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace HydroAnalyzer_v0._4
{
    public partial class frm_Login : Form
    {

        private string conn;
        private MySqlConnection connect;

        public frm_Login()
        {
            InitializeComponent();
        }

        private void db_connection()
        {
            try
            {
                conn = "SERVER=" + this.ip.Text + ";" + 
               "DATABASE=" + this.db.Text + ";" + 
               "UID=" + this.user.Text + ";" + 
               "PASSWORD=" + this.pass.Text + ";"; 
                connect = new MySqlConnection(conn);
                connect.Open();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                Application.Exit();
            }
        }

        private bool validate_login(string user, string pass, string ip, string db)
        {
            db_connection();
            MySqlCommand cmd = new MySqlCommand();

            //За локалната база
            //cmd.CommandText = "Select * from users where username=@user and password=@pass";
            //cmd.Parameters.AddWithValue("@user", user);
            //cmd.Parameters.AddWithValue("@pass", pass);

            cmd.Connection = connect;

           // MySqlDataReader login = cmd.ExecuteReader();

            this.Hide();
            frm_Main fm = new frm_Main(ip, db, user, pass);
            fm.Show();
            connect.Close();
            return true;

            //За локалната база
            /*
                if (login.Read())
            {
                this.Hide();
                frm_Main fm = new frm_Main("195.96.238.43", "hydrodb", "dobri", "dobri_hydrosector");
                fm.Show();
                connect.Close();
                return true;
            }
            else
            {
                connect.Close();
                return false;
            }*/
        }

        private void btn_Submit_Click(object sender, EventArgs e)
        {
            string user = this.user.Text;
            string pass = this.pass.Text;
            string ip = this.ip.Text;
            string db = this.db.Text;
            if (user == "" || pass == "" || ip == "" || db == "")
            {
                MessageBox.Show("Не сте попълнили всички полета !");
                return;
            }
            bool r = validate_login(user, pass, ip, db);
            if (r == false)
                MessageBox.Show("Грешен потребител или парола !");
        }
    }
}

       