using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing.Printing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using ADODB;

namespace HydroAnalyzer_v0._4
{
    public partial class frm_Main : Form
    {

        //Декларирам 5 променливи
        private MySqlConnection connection;
        private string ip;
        private string db;
        private string user;
        private string pass;

        public frm_Main(string ip, string db, string user, string pass)
        {
            InitializeComponent();

            //Връзка към БД
            string MyConString = "SERVER=" + ip + ";" +
               "DATABASE=" + db + ";" +
               "UID=" + user + ";" +
               "PASSWORD=" + pass + ";Character Set=utf8;";
            connection = new MySqlConnection(MyConString);
            connection.Close();

            //Правя ги глобални
            this.ip = ip;
            this.db = db;
            this.user = user;
            this.pass = pass;

        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            //Увеличение на трите графики спрямо максимума по "Y"
            /*
            chart1.ChartAreas[0].AxisY.Maximum = Double.NaN;
            chart2.ChartAreas[0].AxisY.Maximum = Double.NaN;
            chart3.ChartAreas[0].AxisY.Maximum = Double.NaN;
            */
            label3.Hide();
            label9.Hide();
            label8.ForeColor = Color.Black;
            label8.Text = "IP на БД: " + ip.ToString();
            label10.ForeColor = Color.Black;
            label10.Text = "Име на БД: " + db.ToString();

            //Формата се заключва и не може да се прави switch с alt+tab на прозорците
            /*
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            */
            this.WindowState = FormWindowState.Maximized;

            //Импорт на номер на станция от базата.
            string command = "select Station, NasMesto from spisakhyd";
            //Подсказва когато въведеш дадено число, кое число е следващото
            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;

            comboBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox2.AutoCompleteSource = AutoCompleteSource.ListItems;

            //Връзка с базата
            MySqlDataAdapter da = new MySqlDataAdapter(command, connection);
            DataTable dt = new DataTable();
            da.Fill(dt);
            foreach (DataRow row in dt.Rows)
            {
                string rowz = string.Format("{0}", row.ItemArray[0]);
                comboBox1.Items.Add(rowz);
                comboBox1.AutoCompleteCustomSource.Add(row.ItemArray[0].ToString());
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            string command1 = "select year(Dat) FROM hydgod where Station=" + comboBox1.Text;

            MySqlDataAdapter da1 = new MySqlDataAdapter(command1, connection);
            DataTable dt1 = new DataTable();
            da1.Fill(dt1);

            comboBox2.Items.Clear();
            comboBox2.SelectedItem = -1;
 

            foreach (DataRow row in dt1.Rows)
            {
                string rowz = string.Format("{0}", row.ItemArray[0]);
                comboBox2.Items.Add(rowz);
                comboBox2.AutoCompleteCustomSource.Add(row.ItemArray[0].ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(comboBox1.Text) && String.IsNullOrEmpty(comboBox2.Text))
            {
                MessageBox.Show("Моля изберете година");
            }
            else
            {
                DataGridViewRow row = this.dataGridView1.RowTemplate;
                row.DefaultCellStyle.BackColor = Color.Bisque;
                row.Height = 25;
               

                string yearDate = comboBox2.SelectedItem.ToString();

                CalculateData(yearDate);
            }
        }


        private void CalculateData(string yearDate)
        {    

            label3.ForeColor = Color.Red;
            label3.Text = "Годишни стойности за година: " + yearDate;
            label3.Show();

            string command2 = "select God_MinQ,God_AverQ,God_MaxQ,year(Dat) from hydgod where station='"
                + comboBox1.SelectedItem.ToString() + "' and year(Dat) = '"
                + yearDate + "' order by Dat";

            string command3 = "select VkolMin,VkolSre,VkolMax,year(Dat) from hydmes where station='"
                + comboBox1.SelectedItem.ToString() + "' and year(Dat) = '"
                + yearDate + "' order by Dat";

            string command4 = "select vkol from hyddnev where station='"
                + comboBox1.SelectedItem.ToString() + "' and year(Dat) = '"
                + yearDate + "' order by Dat";

            string command6 = "select 'Въведени' AS 'Въведени/Изчислени', Station AS '№ на станция', year(Dat) AS 'Година', CAST(God_MinQ AS DECIMAL(7,3)) 'Мин. Q', CAST(God_AverQ AS DECIMAL(7,3)) 'Ср. Q', CAST(God_MaxQ AS DECIMAL(7,3)) 'Макс. Q' from hydgod where Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = '" + comboBox2.SelectedItem.ToString() + "'"
            + " UNION"
            + " SELECT 'Изчислени', Station, year(Dat), CAST(min(vkol) AS DECIMAL(7,3)), CAST(avg(vkol) AS DECIMAL(7,3)), CAST(max(vkol) AS DECIMAL(7,3)) from hyddnev where Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = '" + comboBox2.SelectedItem.ToString() + "'";

            //За Data Grid View от hyddnev    
            int year1 = int.Parse(yearDate);
            int year2 = int.Parse(yearDate);

            string command5 = "";
            for (int y = year1; y <= year2; y++)
            {
                if (y > year1)
                {
                    command5 += " UNION ";
                }


                command5 += "SELECT 'Въведени' AS 'Въведени/Изчислени', Station AS '№ на станция', year(Dat) AS 'Година', 'НМ' AS 'НМ/СР/НГ', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=1) AS 'Януари', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=2) AS 'Февруари', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=3) AS 'Март', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=4) AS 'Април', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=5) AS 'Май', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=6) AS 'Юни', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=7) AS 'Юли', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=8) AS 'Август', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=9) AS 'Септември', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=10) AS 'Октомври', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=11) AS 'Ноември', (SELECT CAST(vkolmin AS DECIMAL(7,3)) FROM hydmes WHERE Station= '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat)=12) AS 'Декември'"
                + "FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y
                + "  UNION"
                + "  SELECT 'Изчислени', Station, year(Dat), 'НМ', (SELECT CAST(min(vkol) AS DECIMAL(7,3))  FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 1 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 2 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 3 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 4 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 5 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 6 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 7 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 8 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 9 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 10 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 11 LIMIT 1), (SELECT CAST(min(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 12 LIMIT 1)"
                + "FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y
                + "  UNION"
                + "  SELECT 'Въведени', Station, year(Dat), 'СР', (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 1), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 2), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 3), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 4), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 5), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 6), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 7), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 8), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 9), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 10), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 11), (SELECT CAST(vkolsre AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 12)"
                + "FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y
                + "  UNION"
                + "  SELECT 'Изчислени', Station, year(Dat), 'СР', (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 1 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 2 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 3 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 4 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 5 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 6 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 7 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 8 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 9 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 10 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 11 LIMIT 1), (SELECT CAST(avg(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 12 LIMIT 1)"
                + "FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y
                + "  UNION"
                + "  SELECT 'Въведени', Station, year(Dat), 'НГ', (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 1), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 2), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 3), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 4), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 5), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 6), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 7), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 8), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 9), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 10), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 11), (SELECT CAST(vkolmax AS DECIMAL(7,3)) FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 12)"
                + "FROM hydmes WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y
                + "  UNION"
                + "  SELECT 'Изчислени', Station, year(Dat), 'НГ', (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 1 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 2 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 3 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 4 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 5 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 6 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 7 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 8 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 9 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 10 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 11 LIMIT 1), (SELECT CAST(max(vkol) AS DECIMAL(7,3)) FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y + " and month(dat) = 12 LIMIT 1)"
                + "FROM hyddnev WHERE Station = '" + comboBox1.SelectedItem.ToString() + "' and year(Dat) = " + y;

            }
            command5 += "  group by year(dat)";

            label9.ForeColor = Color.Red;
            label9.Show();
            label9.Text = "Избрали сте година: " + yearDate;

            chart1.Series["Avg"].Points.Clear();
            chart1.Series["Min"].Points.Clear();
            chart1.Series["Max"].Points.Clear();



            chart3.Series["Avg"].Points.Clear();

            var StartDate = yearDate;
            var EndDate = yearDate;
            var eDate = Convert.ToInt32(EndDate);
            var sDate = Convert.ToInt32(StartDate);

            MySqlDataAdapter da3 = new MySqlDataAdapter(command3, connection);
            DataTable dt3 = new DataTable();
            da3.Fill(dt3);

            string s1 = "";

            chart1.ChartAreas[0].AxisX.Minimum = double.NaN;
            chart1.ChartAreas[0].AxisX.Maximum = double.NaN;
            chart1.ChartAreas[0].RecalculateAxesScale();

            chart1.Series["Min"].BorderWidth = 3;
            chart1.Series["Avg"].BorderWidth = 3;
            chart1.Series["Max"].BorderWidth = 3;

            int i = 0;
            int startYear = 0;
            int.TryParse(StartDate, out startYear);
            DateTime dtStart = new DateTime(startYear, 01, 01);
            chart1.Series["Min"].XValueType = ChartValueType.DateTime;
            chart1.Series["Avg"].XValueType = ChartValueType.DateTime;
            chart1.Series["Max"].XValueType = ChartValueType.DateTime;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd";
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Months;
            chart1.ChartAreas[0].AxisX.IntervalOffset = 0;

            foreach (DataRow row in dt3.Rows)
            {
                chart1.Series["Avg"].Points.AddXY(dtStart.AddMonths(i), row.ItemArray[1]);
                chart1.Series["Max"].Points.AddXY(dtStart.AddMonths(i), row.ItemArray[2]);
                chart1.Series["Min"].Points.AddXY(dtStart.AddMonths(i), row.ItemArray[0]);

                i++;

                string rowz = string.Format("Min Q - {0}" + Environment.NewLine + "Avg Q - {1}"
                   + Environment.NewLine + "Max Q - {2}" + Environment.NewLine + "Year - {3}" + Environment.NewLine
                   + Environment.NewLine,
                   row.ItemArray[0], row.ItemArray[1], row.ItemArray[2],
                   row.ItemArray[3]);
                s1 += "" + rowz;
            }

            MySqlDataAdapter da2 = new MySqlDataAdapter(command2, connection);
            DataTable dt2 = new DataTable();
            da2.Fill(dt2);

            //string s = "";

            i = 0;

            int startYear1 = 0;
            int.TryParse(StartDate, out startYear1);
            DateTime dtStart1 = new DateTime(startYear1, 01, 01);


            MySqlDataAdapter da4 = new MySqlDataAdapter(command4, connection);
            DataTable dt4 = new DataTable();
            da4.Fill(dt4);


            string s2 = "";
            i = 0;


            chart3.Series["Avg"].Color = Color.Blue;
            chart3.Series["Avg"].BorderWidth = 2;
            int startYear2 = 0;
            int.TryParse(StartDate, out startYear2);
            DateTime dtStart2 = new DateTime(startYear2, 01, 01);
            chart3.Series["Avg"].XValueType = ChartValueType.DateTime;
            chart3.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd";
            chart3.ChartAreas[0].AxisX.Interval = 7;
            chart3.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            chart3.ChartAreas[0].AxisX.IntervalOffset = 1;

            chart3.Series["Avg"].MarkerSize = 10;

            foreach (DataRow row in dt4.Rows)
            {
                chart3.Series["Avg"].Points.AddXY(dtStart2.AddDays(i), row.ItemArray[0]);
                i++;

                string rowz = string.Format("Avg Q - {0}" + Environment.NewLine, row.ItemArray[0]);
                s2 += "" + rowz;
            }


            // zoom на третата графика
            // Axis ax = chart3.ChartAreas[0].AxisX;
            // ax.ScaleView.Size = 30;
            // ax.ScaleView.Position = 30;
            // chart3.Show();


            //comboBox2.Text = "";

            MySqlDataAdapter da5 = new MySqlDataAdapter(command5, connection);
            using (DataTable dt5 = new DataTable())
            {
                da5.Fill(dt5);
                dataGridView1.DataSource = dt5.DefaultView;
            }

            MySqlDataAdapter da6 = new MySqlDataAdapter(command6, connection);
            using (DataTable dt6 = new DataTable())
            {
                da6.Fill(dt6);
                dataGridView2.DataSource = dt6.DefaultView;
            }

            /*
            for (int rows = 0; rows < dataGridView1.Rows.Count; rows++)
            {
                for (int col = 0; col < dataGridView1.Rows[rows].Cells.Count; col++)
                {
                    string value = dataGridView1.Rows[rows].Cells[col].Value.ToString();
                    MessageBox.Show(value);

                }
            } 
            */
            string errorInfo = "";
            string[] months = { "Януари", "Февруари", "Март", "Април", "Май", "Юни", "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември" };
            for (int k = 0; k <= dataGridView1.Rows.Count - 3; k += 2)
            {
                var row1 = dataGridView1.Rows[k];
                var row2 = dataGridView1.Rows[k + 1];

                for (int j = 4; j < row1.Cells.Count; j++)
                {
                    if (k == 0)
                    {

                        if (float.Parse(row1.Cells[j].Value.ToString()) > float.Parse(row2.Cells[j].Value.ToString()))
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има въведена максимална стойност, която е по-малка от изчислената за месец " + months[j - 4] + ".\n";

                        }
                    }
                    if (k == 4)
                    {
                        if (float.Parse(row1.Cells[j].Value.ToString()) < float.Parse(row2.Cells[j].Value.ToString()))
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има въведена минимална стойност, която е по-голяма от изчислената за месец " + months[j - 4] + ".\n";
                        }
                    }
                    if (k == 2)
                    {
                        float x = float.Parse(row1.Cells[j].Value.ToString());
                        float y = float.Parse(row2.Cells[j].Value.ToString());
                        if (y > x * 1.05 || y < x * 0.95 || x < y * 0.95 || x > y * 1.05)
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има разлика с повече от 5 процента между въведената и изчислената средни стойности за месец " + months[j - 4] + ".\n";
                        }
                    }
                }

            }

            if (errorInfo != "")
            {
                MessageBox.Show(errorInfo);
            }
        }

        

        private void datagridview_CellValidating(object sender, EventArgs e)
        {
            /*string errorInfo = "";
            string[] months = { "Януари", "Февруари", "Март", "Април", "Май", "Юни", "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември" };
            for (int k = 0; k < dataGridView1.Rows.Count - 3; k++)
            {
                var row1 = dataGridView1.Rows[k];
                var row2 = dataGridView1.Rows[k + 1];

                for (int j = 4; j < row1.Cells.Count; j++)
                {
                    if (k == 0)
                    {
                        if (float.Parse(row1.Cells[j].Value.ToString()) < float.Parse(row2.Cells[j].Value.ToString()))
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има въведена максимална стойност, която е по-малка от изчислената за месец " + months[j - 4] + ".\n";

                        }
                    }
                    if (k == 1)
                    {
                        if (float.Parse(row1.Cells[j].Value.ToString()) > float.Parse(row2.Cells[j].Value.ToString()))
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има въведена минимална стойност, която е по-голяма от изчислената за месец " + months[j - 4] + ".\n";
                        }
                    }
                    if (k == 2)
                    {
                        float x = float.Parse(row1.Cells[j].Value.ToString());
                        float y = float.Parse(row2.Cells[j].Value.ToString());
                        if (y > x * 1.05 || y < x * 0.95 || x < y * 0.95 || x > y * 1.05)
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има разлика с повече от 5 процента между въведената и изчислената средни стойности за месец " + months[j - 4] + ".\n";
                        }
                    }
                }
            }

            if (errorInfo != "")
            {
                MessageBox.Show(errorInfo);
            }  
            */
        }
       
        private System.IO.Stream streamToPrint;

        string streamType;

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern bool BitBlt
        (
            IntPtr hdcDest, // handle to destination DC
            int nXDest, // x-coord of destination upper-left corner
            int nYDest, // y-coord of destination upper-left corner
            int nWidth, // width of destination rectangle
            int nHeight, // height of destination rectangle
            IntPtr hdcSrc, // handle to source DC
            int nXSrc, // x-coordinate of source upper-left corner
            int nYSrc, // y-coordinate of source upper-left corner
            System.Int32 dwRop // raster operation code
        );

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(this.streamToPrint);

            int x = e.MarginBounds.X;
            int y = e.MarginBounds.Y;

            int width = image.Width;
            int height = image.Height;
            if ((width / e.MarginBounds.Width) > (height / e.MarginBounds.Height))
            {
                width = e.MarginBounds.Width;
                height = image.Height * e.MarginBounds.Width / image.Width;
            }
            else
            {
                height = e.MarginBounds.Height;
                width = image.Width * e.MarginBounds.Height / image.Height;
            }
            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(x, y, width, height);
            e.Graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel);
        }

        void PrintImage(object o, PrintPageEventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {
            String filename = System.IO.Path.GetTempFileName();

            Graphics g1 = this.CreateGraphics();
            Image MyImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height, g1);
            Graphics g2 = Graphics.FromImage(MyImage);
            IntPtr dc1 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();
            BitBlt(dc2, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, dc1, 0, 0, 13369376);
            g1.ReleaseHdc(dc1);
            g2.ReleaseHdc(dc2);
            MyImage.Save(filename, ImageFormat.Png);
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StartPrint(fileStream, "Image");
            fileStream.Close();
            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }
        }

        private void printPreviewDialog1_Load(object sender, EventArgs e)
        {

        }

        public void StartPrint(System.IO.Stream streamToPrint, string streamType)
        {

            this.printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);

            this.streamToPrint = streamToPrint;

            this.streamType = streamType;

            System.Windows.Forms.PrintDialog PrintDialog1 = new PrintDialog();

            PrintDialog1.AllowSomePages = true;
            PrintDialog1.ShowHelp = true;
            PrintDialog1.Document = printDocument1;
            DialogResult result = PrintDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                printDocument1.Print();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string folder = @"D:\Comments\";
            folder = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "\\Comments";
            string path = Environment.ExpandEnvironmentVariables(folder + "\\" + comboBox1.SelectedItem.ToString() + ".txt");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine("№ на станция: " + comboBox1.SelectedItem.ToString() + "," + " Дата/час: " + DateTime.Now + "," + " Коментар: " + textBox1.Text + Environment.NewLine);

                    MessageBox.Show("Вашият коментар е записан в директория: " + path);
                }
            }

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void frm_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
            Application.Exit();
        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string errorInfo = "";
            string[] months = { "Януари", "Февруари", "Март", "Април", "Май", "Юни", "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември" };
            for (int k = 0; k < dataGridView1.Rows.Count - 3; k++)
            {
                var row1 = dataGridView1.Rows[k];
                var row2 = dataGridView1.Rows[k + 1];

                for (int j = 4; j < row1.Cells.Count; j++)
                {
                    if (k == 0)
                    {
                        if (float.Parse(row1.Cells[j].Value.ToString()) > float.Parse(row2.Cells[j].Value.ToString()))
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има въведена максимална стойност, която е по-малка от изчислената за месец " + months[j - 4] + ".\n";

                        }
                    }
                    if (k == 1)
                    {
                        if (float.Parse(row1.Cells[j].Value.ToString()) < float.Parse(row2.Cells[j].Value.ToString()))
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има въведена минимална стойност, която е по-голяма от изчислената за месец " + months[j - 4] + ".\n";
                        }
                    }
                    if (k == 2)
                    {
                        float x = float.Parse(row1.Cells[j].Value.ToString());
                        float y = float.Parse(row2.Cells[j].Value.ToString());
                        if (y > x * 1.05 || y < x * 0.95 || x < y * 0.95 || x > y * 1.05)
                        {
                            dataGridView1.Rows[k].Cells[j].Style.BackColor = Color.Red;
                            dataGridView1.Rows[k + 1].Cells[j].Style.BackColor = Color.Red;
                            errorInfo += "Има разлика с повече от 5 процента между въведената и изчислената средни стойности за месец " + months[j - 4] + ".\n";
                        }
                    }
                }

            }
            if (errorInfo != "")
            {
                MessageBox.Show(errorInfo);
            }
            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            string yearDate = comboBox2.SelectedItem.ToString();

            int nextYear = int.Parse(yearDate) + 1;

            comboBox2.SelectedItem = nextYear.ToString();
            comboBox2.Text = nextYear.ToString();

            CalculateData(nextYear.ToString());
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            string yearDate = comboBox2.SelectedItem.ToString();

            int previousYear = int.Parse(yearDate) - 1;

            comboBox2.SelectedItem = previousYear.ToString();
            comboBox2.Text = previousYear.ToString();

            CalculateData(previousYear.ToString());
        }
    }
}

