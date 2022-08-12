using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Renci.SshNet;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Actor_Information_22
{
    public partial class Form1 : Form
    {
        int i, j,x;
        string strText;
        List<string> ftp_list = new List<string>();
        string strDateFrom, strDateTo, plainText, cipherText;
        string[] strFamilyChecked;
        string[] strAdultChecked;
        string[] strChildChecked;
        
        int intCShowCount, intAShowCount, intFShowCount;
        int intTotalAdult, intTotalChild, intTotalFamily;
        bool bLoaded;
        List<string> child_list = new List<string>();
        List<string> adult_list = new List<string>();
        List<string> family_list = new List<string>();

        List<string> backupList = new List<string>();
        string[,,] backupData = new string[60,60, 25];
        //backup data [time,people,question]
        public Form1()
        {
            InitializeComponent();
            loadSettings();
            //hide some components
            dgvAdult.Visible = false;
            dgvChildShowF1.Visible = false;
            dgvFamily.Visible = false;
            lblAdult.Visible = false;
            lblFamily.Visible = false;
            lblChild.Visible = false;
            lblWorking.Visible = false;
            lblProgress.Visible = false;
            lblBInfo.Visible = false;
        }

        private void cmdCheckPassword_Click(object sender, EventArgs e)
        {
            checkPassword();
        }

        private void loadSaved()
        {
            if (bLoaded == true)
            {
                if (Properties.Settings.Default.aiSavedChecked == true)
                {
                    gbSaved.Visible = true;
                }
                else
                {
                    gbSaved.Visible = false;
                }
                if (Properties.Settings.Default.aiSaveType == 0)
                {

                }
                else
                {

                }
            }
        }

        private void loadSettings()
        {

            txtLocalBackup.Text = Properties.Settings.Default.aiLocalBackup.ToString();

            foreach (DataGridViewColumn column in dgvChildShowF1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            foreach (DataGridViewColumn column in dgvAdult.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            foreach (DataGridViewColumn column in dgvFamily.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            i = 10;
            while (i<30)
            {
                cboFont.Items.Add(i);
                i = i + 1;
            }

            dgvChildShowF1.DefaultCellStyle.Font = new Font("Sans Serif", Properties.Settings.Default.aiFontSize);
            dgvAdult.DefaultCellStyle.Font = new Font("Sans Serif", Properties.Settings.Default.aiFontSize);
            dgvFamily.DefaultCellStyle.Font = new Font("Sans Serif", Properties.Settings.Default.aiFontSize);

            txtFTPSaveLocation.Text = Properties.Settings.Default.aiSavedFTP.ToString();
            txtSavedLocalLocation.Text = Properties.Settings.Default.aiSavedLocal.ToString();


            if (Properties.Settings.Default.aiSaveType == 0)
            {
                rbFTP.Checked = true;

                txtFTPSaveLocation.Visible = true;
                cmdFTPSave.Visible = true;

                txtSavedLocalLocation.Visible = false;
                cmdSaveBrowse.Visible = false;

            }
            else
            {
                rbNetwork.Checked = true;

                txtFTPSaveLocation.Visible = false;
                cmdFTPSave.Visible = false;

                txtSavedLocalLocation.Visible = true;
                cmdSaveBrowse.Visible = true;
            }
            cboFont.Text = Properties.Settings.Default.aiFontSize.ToString();

            bLoaded = false;
            this.Height = 800;
            this.Width = 1200;
            this.Text = Properties.Settings.Default.aiProgramName.ToString();

            txtProgramName.Text = Properties.Settings.Default.aiProgramName.ToString();
            this.Name = txtProgramName.Text;

            //load decryption password
            txtDCPW.Text = Properties.Settings.Default.aiDCPW.ToString();

            //load dates
            string[] dateFrom = Properties.Settings.Default.aiFrom.ToString().Split('/');
            string[] dateTo = Properties.Settings.Default.aiTo.ToString().Split('/');
            dtpFrom.Value = new DateTime(Int32.Parse(dateFrom[2]), Int32.Parse(dateFrom[1]), Int32.Parse(dateFrom[0]));
            dtpTo.Value = new DateTime(Int32.Parse(dateTo[2]), Int32.Parse(dateTo[1]), Int32.Parse(dateTo[0]));
            loadDates();

            //load times
            i = 9;
            while (i < 19)
            {
                j = 0;
                while (j < 60)
                {
                    if (j == 0)
                    {
                        strText = i.ToString() + ":00";
                    }
                    else
                    {
                        strText = i.ToString() + ":" + j.ToString();
                    }
                    cboTime.Items.Add(strText);
                    j = j + 10;
                }
                i = i + 1;
            }
            i = 0;

            //load properties
            loadFTPDetails();


            //timer settings
            timer1.Enabled = false;
            timer1.Interval = 1000;
            txtWait.Text = Properties.Settings.Default.aiTimerInterval.ToString();

            txtLocal.Text = Properties.Settings.Default.aiSaveLocal.ToString();

            //load questions
            loadQuestions();

            //load saved
            chkSaved.Checked = Properties.Settings.Default.aiSavedChecked;
            updateDataGridView();
            bLoaded = true;
        }

        private void loadQuestions()
        {
            var list = new List<string>();
            //read embedded Questions text document
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("Questions.txt"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }
            //seperate the lines into adults, children and family
            string[] lstChild = list[0].ToString().Split(',');
            string[] lstAdult = list[1].ToString().Split(',');
            string[] lstFamily = list[2].ToString().Split(',');

            //child checkbox setup
            //hide all columns
            i = 2;
            while (i < 20)
            {
                dgvChildShowF1.Columns[i].Visible = false;
                i = i + 1;
            }
            //name and display check boxes
            intCShowCount = 0;
            try { chkC1.Text = lstChild[1].ToString(); chkC1.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC1.Visible = false; }
            try { chkC2.Text = lstChild[2].ToString(); chkC2.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC2.Visible = false; }
            try { chkC3.Text = lstChild[3].ToString(); chkC3.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC3.Visible = false; }
            try { chkC4.Text = lstChild[4].ToString(); chkC4.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC4.Visible = false; }
            try { chkC5.Text = lstChild[5].ToString(); chkC5.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC5.Visible = false; }
            try { chkC6.Text = lstChild[6].ToString(); chkC6.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC6.Visible = false; }
            try { chkC7.Text = lstChild[7].ToString(); chkC7.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC7.Visible = false; }
            try { chkC8.Text = lstChild[8].ToString(); chkC8.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC8.Visible = false; }
            try { chkC9.Text = lstChild[9].ToString(); chkC9.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC9.Visible = false; }
            try { chkC10.Text = lstChild[10].ToString(); chkC10.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC10.Visible = false; }
            try { chkC11.Text = lstChild[11].ToString(); chkC11.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC11.Visible = false; }
            try { chkC12.Text = lstChild[12].ToString(); chkC12.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC12.Visible = false; }
            try { chkC13.Text = lstChild[13].ToString(); chkC13.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC13.Visible = false; }
            try { chkC14.Text = lstChild[14].ToString(); chkC14.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC14.Visible = false; }
            try { chkC15.Text = lstChild[15].ToString(); chkC15.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC15.Visible = false; }
            try { chkC16.Text = lstChild[16].ToString(); chkC16.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC16.Visible = false; }
            try { chkC17.Text = lstChild[17].ToString(); chkC17.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC17.Visible = false; }
            try { chkC18.Text = lstChild[18].ToString(); chkC18.Visible = true; intCShowCount = intCShowCount + 1; } catch { chkC18.Visible = false; }
            //check check boxes
            strChildChecked = Properties.Settings.Default.aiChildChecked.ToString().Split(',');
            if (strChildChecked[0] == "True") { chkC1.Checked = true; } else { chkC1.Checked = false; }
            if (strChildChecked[1] == "True") { chkC2.Checked = true; } else { chkC2.Checked = false; }
            if (strChildChecked[2] == "True") { chkC3.Checked = true; } else { chkC3.Checked = false; }
            if (strChildChecked[3] == "True") { chkC4.Checked = true; } else { chkC4.Checked = false; }
            if (strChildChecked[4] == "True") { chkC5.Checked = true; } else { chkC5.Checked = false; }
            if (strChildChecked[5] == "True") { chkC6.Checked = true; } else { chkC6.Checked = false; }
            if (strChildChecked[6] == "True") { chkC7.Checked = true; } else { chkC7.Checked = false; }
            if (strChildChecked[7] == "True") { chkC8.Checked = true; } else { chkC8.Checked = false; }
            if (strChildChecked[8] == "True") { chkC9.Checked = true; } else { chkC9.Checked = false; }
            if (strChildChecked[9] == "True") { chkC10.Checked = true; } else { chkC10.Checked = false; }
            if (strChildChecked[10] == "True") { chkC11.Checked = true; } else { chkC11.Checked = false; }
            if (strChildChecked[11] == "True") { chkC12.Checked = true; } else { chkC12.Checked = false; }
            if (strChildChecked[12] == "True") { chkC13.Checked = true; } else { chkC13.Checked = false; }
            if (strChildChecked[13] == "True") { chkC14.Checked = true; } else { chkC14.Checked = false; }
            if (strChildChecked[14] == "True") { chkC15.Checked = true; } else { chkC15.Checked = false; }
            if (strChildChecked[15] == "True") { chkC16.Checked = true; } else { chkC16.Checked = false; }
            if (strChildChecked[16] == "True") { chkC17.Checked = true; } else { chkC17.Checked = false; }
            if (strChildChecked[17] == "True") { chkC18.Checked = true; } else { chkC18.Checked = false; }

            //adult checkbox setup
            //hide all columns
            i = 2;
            while (i < 20)
            {
                dgvAdult.Columns[i].Visible = false;
                i = i + 1;
            }
            //name and display check boxes
            intAShowCount = 0;
            try { chkA1.Text = lstAdult[1].ToString(); chkA1.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA1.Visible = false; }
            try { chkA2.Text = lstAdult[2].ToString(); chkA2.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA2.Visible = false; }
            try { chkA3.Text = lstAdult[3].ToString(); chkA3.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA3.Visible = false; }
            try { chkA4.Text = lstAdult[4].ToString(); chkA4.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA4.Visible = false; }
            try { chkA5.Text = lstAdult[5].ToString(); chkA5.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA5.Visible = false; }
            try { chkA6.Text = lstAdult[6].ToString(); chkA6.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA6.Visible = false; }
            try { chkA7.Text = lstAdult[7].ToString(); chkA7.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA7.Visible = false; }
            try { chkA8.Text = lstAdult[8].ToString(); chkA8.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA8.Visible = false; }
            try { chkA9.Text = lstAdult[9].ToString(); chkA9.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA9.Visible = false; }
            try { chkA10.Text = lstAdult[10].ToString(); chkA10.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA10.Visible = false; }
            try { chkA11.Text = lstAdult[11].ToString(); chkA11.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA11.Visible = false; }
            try { chkA12.Text = lstAdult[12].ToString(); chkA12.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA12.Visible = false; }
            try { chkA13.Text = lstAdult[13].ToString(); chkA13.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA13.Visible = false; }
            try { chkA14.Text = lstAdult[14].ToString(); chkA14.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA14.Visible = false; }
            try { chkA15.Text = lstAdult[15].ToString(); chkA15.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA15.Visible = false; }
            try { chkA16.Text = lstAdult[16].ToString(); chkA16.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA16.Visible = false; }
            try { chkA17.Text = lstAdult[17].ToString(); chkA17.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA17.Visible = false; }
            try { chkA18.Text = lstAdult[18].ToString(); chkA18.Visible = true; intAShowCount = intAShowCount + 1; } catch { chkA18.Visible = false; }
            //check check boxes
            strAdultChecked = Properties.Settings.Default.aiAdultChecked.ToString().Split(',');
            if (strAdultChecked[0] == "True") { chkA1.Checked = true; } else { chkA1.Checked = false; }
            if (strAdultChecked[1] == "True") { chkA2.Checked = true; } else { chkA2.Checked = false; }
            if (strAdultChecked[2] == "True") { chkA3.Checked = true; } else { chkA3.Checked = false; }
            if (strAdultChecked[3] == "True") { chkA4.Checked = true; } else { chkA4.Checked = false; }
            if (strAdultChecked[4] == "True") { chkA5.Checked = true; } else { chkA5.Checked = false; }
            if (strAdultChecked[5] == "True") { chkA6.Checked = true; } else { chkA6.Checked = false; }
            if (strAdultChecked[6] == "True") { chkA7.Checked = true; } else { chkA7.Checked = false; }
            if (strAdultChecked[7] == "True") { chkA8.Checked = true; } else { chkA8.Checked = false; }
            if (strAdultChecked[8] == "True") { chkA9.Checked = true; } else { chkA9.Checked = false; }
            if (strAdultChecked[9] == "True") { chkA10.Checked = true; } else { chkA10.Checked = false; }
            if (strAdultChecked[10] == "True") { chkA11.Checked = true; } else { chkA11.Checked = false; }
            if (strAdultChecked[11] == "True") { chkA12.Checked = true; } else { chkA12.Checked = false; }
            if (strAdultChecked[12] == "True") { chkA13.Checked = true; } else { chkA13.Checked = false; }
            if (strAdultChecked[13] == "True") { chkA14.Checked = true; } else { chkA14.Checked = false; }
            if (strAdultChecked[14] == "True") { chkA15.Checked = true; } else { chkA15.Checked = false; }
            if (strAdultChecked[15] == "True") { chkA16.Checked = true; } else { chkA16.Checked = false; }
            if (strAdultChecked[16] == "True") { chkA17.Checked = true; } else { chkA17.Checked = false; }
            if (strAdultChecked[17] == "True") { chkA18.Checked = true; } else { chkA18.Checked = false; }

            //family checkbox setup
            //hide all columns
            i = 1;
            while (i < 20)
            {
                dgvFamily.Columns[i].Visible = false;
                i = i + 1;
            }
            //name and display check boxes
            intFShowCount = 0;
            try { chkF1.Text = lstFamily[1].ToString(); chkF1.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF1.Visible = false; }
            try { chkF2.Text = lstFamily[2].ToString(); chkF2.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF2.Visible = false; }
            try { chkF3.Text = lstFamily[3].ToString(); chkF3.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF3.Visible = false; }
            try { chkF4.Text = lstFamily[4].ToString(); chkF4.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF4.Visible = false; }
            try { chkF5.Text = lstFamily[5].ToString(); chkF5.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF5.Visible = false; }
            try { chkF6.Text = lstFamily[6].ToString(); chkF6.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF6.Visible = false; }
            try { chkF7.Text = lstFamily[7].ToString(); chkF7.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF7.Visible = false; }
            try { chkF8.Text = lstFamily[8].ToString(); chkF8.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF8.Visible = false; }
            try { chkF9.Text = lstFamily[9].ToString(); chkF9.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF9.Visible = false; }
            try { chkF10.Text = lstFamily[10].ToString(); chkF10.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF10.Visible = false; }
            try { chkF11.Text = lstFamily[11].ToString(); chkF11.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF11.Visible = false; }
            try { chkF12.Text = lstFamily[12].ToString(); chkF12.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF12.Visible = false; }
            try { chkF13.Text = lstFamily[13].ToString(); chkF13.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF13.Visible = false; }
            try { chkF14.Text = lstFamily[14].ToString(); chkF14.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF14.Visible = false; }
            try { chkF15.Text = lstFamily[15].ToString(); chkF15.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF15.Visible = false; }
            try { chkF16.Text = lstFamily[16].ToString(); chkF16.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF16.Visible = false; }
            try { chkF17.Text = lstFamily[17].ToString(); chkF17.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF17.Visible = false; }
            try { chkF18.Text = lstFamily[18].ToString(); chkF18.Visible = true; intFShowCount = intFShowCount + 1; } catch { chkF18.Visible = false; }
            //check check boxes
            strFamilyChecked = Properties.Settings.Default.aiFamilyChecked.ToString().Split(',');
            if (strFamilyChecked[0] == "True") { chkF1.Checked = true; } else { chkF1.Checked = false; }
            if (strFamilyChecked[1] == "True" ) { chkF2.Checked = true; } else { chkF2.Checked = false; }
            if (strFamilyChecked[2] == "True" ) { chkF3.Checked = true; } else { chkF3.Checked = false; }
            if (strFamilyChecked[3] == "True" ) { chkF4.Checked = true; } else { chkF4.Checked = false; }
            if (strFamilyChecked[4] == "True" ) { chkF5.Checked = true; } else { chkF5.Checked = false; }
            if (strFamilyChecked[5] == "True" ) { chkF6.Checked = true; } else { chkF6.Checked = false; }
            if (strFamilyChecked[6] == "True" ) { chkF7.Checked = true; } else { chkF7.Checked = false; }
            if (strFamilyChecked[7] == "True") { chkF8.Checked = true; } else { chkF8.Checked = false; }
            if (strFamilyChecked[8] == "True" ) { chkF9.Checked = true; } else { chkF9.Checked = false; }
            if (strFamilyChecked[9] == "True")  { chkF10.Checked = true; } else { chkF10.Checked = false; }
            if (strFamilyChecked[10] == "True") { chkF11.Checked = true; } else { chkF11.Checked = false; }
            if (strFamilyChecked[11] == "True") { chkF12.Checked = true; } else { chkF12.Checked = false; }
            if (strFamilyChecked[12] == "True") { chkF13.Checked = true; } else { chkF13.Checked = false; }
            if (strFamilyChecked[13] == "True" ) { chkF14.Checked = true; } else { chkF14.Checked = false; }
            if (strFamilyChecked[14] == "True" ) { chkF15.Checked = true; } else { chkF15.Checked = false; }
            if (strFamilyChecked[15] == "True" ) { chkF16.Checked = true; } else { chkF16.Checked = false; }
            if (strFamilyChecked[16] == "True" ) { chkF17.Checked = true; } else { chkF17.Checked = false; }
            if (strFamilyChecked[17] == "True" ) { chkF18.Checked = true; } else { chkF18.Checked = false; }
        }

        private void updateDataGridView()
        {
            
             dgvChildShowF1.Columns[0].HeaderText = "Family Name";
            dgvChildShowF1.Columns[1].HeaderText = "Child Name";
            if (chkC1.Checked == true) {  dgvChildShowF1.Columns[2].HeaderText = chkC1.Text;  dgvChildShowF1.Columns[2].Visible = true; } else { dgvChildShowF1.Columns[2].HeaderText = chkC1.Text; dgvChildShowF1.Columns[2].Visible = false; }
            if (chkC2.Checked == true) {   dgvChildShowF1.Columns[3].HeaderText = chkC2.Text;  dgvChildShowF1.Columns[3].Visible = true; } else { dgvChildShowF1.Columns[3].HeaderText = chkC2.Text; dgvChildShowF1.Columns[3].Visible = false; }
            if (chkC3.Checked == true) {   dgvChildShowF1.Columns[4].HeaderText = chkC3.Text;  dgvChildShowF1.Columns[4].Visible = true; } else { dgvChildShowF1.Columns[4].HeaderText = chkC3.Text; dgvChildShowF1.Columns[4].Visible = false; }
            if (chkC4.Checked == true) {   dgvChildShowF1.Columns[5].HeaderText = chkC4.Text;  dgvChildShowF1.Columns[5].Visible = true; } else { dgvChildShowF1.Columns[5].HeaderText = chkC4.Text; dgvChildShowF1.Columns[5].Visible = false; }
            if (chkC5.Checked == true) {   dgvChildShowF1.Columns[6].HeaderText = chkC5.Text;  dgvChildShowF1.Columns[6].Visible = true; } else { dgvChildShowF1.Columns[6].HeaderText = chkC5.Text; dgvChildShowF1.Columns[6].Visible = false; }
            if (chkC6.Checked == true) {   dgvChildShowF1.Columns[7].HeaderText = chkC6.Text;  dgvChildShowF1.Columns[7].Visible = true; } else { dgvChildShowF1.Columns[7].HeaderText = chkC6.Text; dgvChildShowF1.Columns[7].Visible = false; }
            if (chkC7.Checked == true) {   dgvChildShowF1.Columns[8].HeaderText = chkC7.Text;  dgvChildShowF1.Columns[8].Visible = true; } else { dgvChildShowF1.Columns[8].HeaderText = chkC7.Text; dgvChildShowF1.Columns[8].Visible = false; }
            if (chkC8.Checked == true) {   dgvChildShowF1.Columns[9].HeaderText = chkC8.Text;  dgvChildShowF1.Columns[9].Visible = true; } else { dgvChildShowF1.Columns[9].HeaderText = chkC8.Text; dgvChildShowF1.Columns[9].Visible = false; }
            if (chkC9.Checked == true) {   dgvChildShowF1.Columns[10].HeaderText = chkC9.Text;  dgvChildShowF1.Columns[10].Visible = true; } else { dgvChildShowF1.Columns[10].HeaderText = chkC9.Text; dgvChildShowF1.Columns[10].Visible = false; }
            if (chkC10.Checked == true) {   dgvChildShowF1.Columns[11].HeaderText = chkC10.Text;  dgvChildShowF1.Columns[11].Visible = true; } else { dgvChildShowF1.Columns[11].HeaderText = chkC10.Text; dgvChildShowF1.Columns[11].Visible = false; }
            if (chkC11.Checked == true) {   dgvChildShowF1.Columns[12].HeaderText = chkC11.Text;  dgvChildShowF1.Columns[12].Visible = true; } else { dgvChildShowF1.Columns[12].HeaderText = chkC11.Text; dgvChildShowF1.Columns[12].Visible = false; }
            if (chkC12.Checked == true) {   dgvChildShowF1.Columns[13].HeaderText = chkC12.Text;  dgvChildShowF1.Columns[13].Visible = true; } else { dgvChildShowF1.Columns[13].HeaderText = chkC12.Text; dgvChildShowF1.Columns[13].Visible = false; }
            if (chkC13.Checked == true) {   dgvChildShowF1.Columns[14].HeaderText = chkC13.Text;  dgvChildShowF1.Columns[14].Visible = true; } else { dgvChildShowF1.Columns[14].HeaderText = chkC13.Text; dgvChildShowF1.Columns[14].Visible = false; }
            if (chkC14.Checked == true) {   dgvChildShowF1.Columns[15].HeaderText = chkC14.Text;  dgvChildShowF1.Columns[15].Visible = true; } else { dgvChildShowF1.Columns[15].HeaderText = chkC14.Text; dgvChildShowF1.Columns[15].Visible = false; }
            if (chkC15.Checked == true) {   dgvChildShowF1.Columns[16].HeaderText = chkC15.Text;  dgvChildShowF1.Columns[16].Visible = true; } else { dgvChildShowF1.Columns[16].HeaderText = chkC15.Text; dgvChildShowF1.Columns[16].Visible = false; }
            if (chkC16.Checked == true) {   dgvChildShowF1.Columns[17].HeaderText = chkC16.Text;  dgvChildShowF1.Columns[17].Visible = true; } else { dgvChildShowF1.Columns[17].HeaderText = chkC16.Text; dgvChildShowF1.Columns[17].Visible = false; }
            if (chkC17.Checked == true) {   dgvChildShowF1.Columns[18].HeaderText = chkC17.Text;  dgvChildShowF1.Columns[18].Visible = true; } else { dgvChildShowF1.Columns[18].HeaderText = chkC17.Text; dgvChildShowF1.Columns[18].Visible = false; }
            if (chkC18.Checked == true) {   dgvChildShowF1.Columns[19].HeaderText = chkC18.Text;  dgvChildShowF1.Columns[19].Visible = true; } else { dgvChildShowF1.Columns[19].HeaderText = chkC18.Text; dgvChildShowF1.Columns[19].Visible = false; }

            dgvAdult.Columns[0].HeaderText = "Family Name";
            dgvAdult.Columns[1].HeaderText = "Adult Name";
            if (chkA1.Checked == true) { dgvAdult.Columns[2].HeaderText = chkA1.Text; dgvAdult.Columns[2].Visible = true; } else { dgvAdult.Columns[2].HeaderText = chkA1.Text; dgvAdult.Columns[2].Visible = false; }
            if (chkA2.Checked == true) { dgvAdult.Columns[3].HeaderText = chkA2.Text; dgvAdult.Columns[3].Visible = true; } else { dgvAdult.Columns[3].HeaderText = chkA2.Text; dgvAdult.Columns[3].Visible = false; }
            if (chkA3.Checked == true) { dgvAdult.Columns[4].HeaderText = chkA3.Text; dgvAdult.Columns[4].Visible = true; } else { dgvAdult.Columns[4].HeaderText = chkA3.Text; dgvAdult.Columns[4].Visible = false; }
            if (chkA4.Checked == true) { dgvAdult.Columns[5].HeaderText = chkA4.Text; dgvAdult.Columns[5].Visible = true; } else { dgvAdult.Columns[5].HeaderText = chkA4.Text; dgvAdult.Columns[5].Visible = false; }
            if (chkA5.Checked == true) { dgvAdult.Columns[6].HeaderText = chkA5.Text; dgvAdult.Columns[6].Visible = true; } else { dgvAdult.Columns[6].HeaderText = chkA5.Text; dgvAdult.Columns[6].Visible = false; }
            if (chkA6.Checked == true) { dgvAdult.Columns[7].HeaderText = chkA6.Text; dgvAdult.Columns[7].Visible = true; } else { dgvAdult.Columns[7].HeaderText = chkA6.Text; dgvAdult.Columns[7].Visible = false; }
            if (chkA7.Checked == true) { dgvAdult.Columns[8].HeaderText = chkA7.Text; dgvAdult.Columns[8].Visible = true; } else { dgvAdult.Columns[8].HeaderText = chkA7.Text; dgvAdult.Columns[8].Visible = false; }
            if (chkA8.Checked == true) { dgvAdult.Columns[9].HeaderText = chkA8.Text; dgvAdult.Columns[9].Visible = true; } else { dgvAdult.Columns[9].HeaderText = chkA8.Text; dgvAdult.Columns[9].Visible = false; }
            if (chkA9.Checked == true) { dgvAdult.Columns[10].HeaderText = chkA9.Text; dgvAdult.Columns[10].Visible = true; } else { dgvAdult.Columns[10].HeaderText = chkA9.Text; dgvAdult.Columns[10].Visible = false; }
            if (chkA10.Checked == true) { dgvAdult.Columns[11].HeaderText = chkA10.Text; dgvAdult.Columns[11].Visible = true; } else { dgvAdult.Columns[11].HeaderText = chkA10.Text; dgvAdult.Columns[11].Visible = false; }
            if (chkA11.Checked == true) { dgvAdult.Columns[12].HeaderText = chkA11.Text; dgvAdult.Columns[12].Visible = true; } else { dgvAdult.Columns[12].HeaderText = chkA11.Text; dgvAdult.Columns[12].Visible = false; }
            if (chkA12.Checked == true) { dgvAdult.Columns[13].HeaderText = chkA12.Text; dgvAdult.Columns[13].Visible = true; } else { dgvAdult.Columns[13].HeaderText = chkA12.Text; dgvAdult.Columns[13].Visible = false; }
            if (chkA13.Checked == true) { dgvAdult.Columns[14].HeaderText = chkA13.Text; dgvAdult.Columns[14].Visible = true; } else { dgvAdult.Columns[14].HeaderText = chkA13.Text; dgvAdult.Columns[14].Visible = false; }
            if (chkA14.Checked == true) { dgvAdult.Columns[15].HeaderText = chkA14.Text; dgvAdult.Columns[15].Visible = true; } else { dgvAdult.Columns[15].HeaderText = chkA14.Text; dgvAdult.Columns[15].Visible = false; }
            if (chkA15.Checked == true) { dgvAdult.Columns[16].HeaderText = chkA15.Text; dgvAdult.Columns[16].Visible = true; } else { dgvAdult.Columns[16].HeaderText = chkA15.Text; dgvAdult.Columns[16].Visible = false; }
            if (chkA16.Checked == true) { dgvAdult.Columns[17].HeaderText = chkA16.Text; dgvAdult.Columns[17].Visible = true; } else { dgvAdult.Columns[17].HeaderText = chkA16.Text; dgvAdult.Columns[17].Visible = false; }
            if (chkA17.Checked == true) { dgvAdult.Columns[18].HeaderText = chkA17.Text; dgvAdult.Columns[18].Visible = true; } else { dgvAdult.Columns[18].HeaderText = chkA17.Text; dgvAdult.Columns[18].Visible = false; }
            if (chkA18.Checked == true) { dgvAdult.Columns[19].HeaderText = chkA18.Text; dgvAdult.Columns[19].Visible = true; } else { dgvAdult.Columns[19].HeaderText = chkA18.Text; dgvAdult.Columns[19].Visible = false; }

            dgvFamily.Columns[0].HeaderText = "Family Name";
            if (chkF1.Checked == true) {  dgvFamily.Columns[1].HeaderText = chkF1.Text; dgvFamily.Columns[1].Visible = true; } else { dgvFamily.Columns[1].HeaderText = chkF1.Text; dgvFamily.Columns[1].Visible = false; }
            if (chkF2.Checked == true) {  dgvFamily.Columns[2].HeaderText = chkF2.Text; dgvFamily.Columns[2].Visible = true; } else { dgvFamily.Columns[2].HeaderText = chkF2.Text; dgvFamily.Columns[2].Visible = false; }
            if (chkF3.Checked == true) {  dgvFamily.Columns[3].HeaderText = chkF3.Text; dgvFamily.Columns[3].Visible = true; } else { dgvFamily.Columns[3].HeaderText = chkF3.Text; dgvFamily.Columns[3].Visible = false; }
            if (chkF4.Checked == true) {  dgvFamily.Columns[4].HeaderText = chkF4.Text; dgvFamily.Columns[4].Visible = true; } else { dgvFamily.Columns[4].HeaderText = chkF4.Text; dgvFamily.Columns[4].Visible = false; }
            if (chkF5.Checked == true) {  dgvFamily.Columns[5].HeaderText = chkF5.Text; dgvFamily.Columns[5].Visible = true; } else { dgvFamily.Columns[5].HeaderText = chkF5.Text; dgvFamily.Columns[5].Visible = false; }
            if (chkF6.Checked == true) {  dgvFamily.Columns[6].HeaderText = chkF6.Text; dgvFamily.Columns[6].Visible = true; } else { dgvFamily.Columns[6].HeaderText = chkF6.Text; dgvFamily.Columns[6].Visible = false; }
            if (chkF7.Checked == true) {  dgvFamily.Columns[7].HeaderText = chkF7.Text; dgvFamily.Columns[7].Visible = true; } else { dgvFamily.Columns[7].HeaderText = chkF7.Text; dgvFamily.Columns[7].Visible = false; }
            if (chkF8.Checked == true) {  dgvFamily.Columns[8].HeaderText = chkF8.Text; dgvFamily.Columns[8].Visible = true; } else { dgvFamily.Columns[8].HeaderText = chkF8.Text; dgvFamily.Columns[8].Visible = false; }
            if (chkF9.Checked == true) {  dgvFamily.Columns[9].HeaderText = chkF9.Text; dgvFamily.Columns[9].Visible = true; } else { dgvFamily.Columns[9].HeaderText = chkF9.Text; dgvFamily.Columns[9].Visible = false; }
            if (chkF10.Checked == true) {  dgvFamily.Columns[10].HeaderText = chkF10.Text; dgvFamily.Columns[10].Visible = true; } else { dgvFamily.Columns[10].HeaderText = chkF10.Text; dgvFamily.Columns[10].Visible = false; }
            if (chkF11.Checked == true) {  dgvFamily.Columns[11].HeaderText = chkF11.Text; dgvFamily.Columns[11].Visible = true; } else { dgvFamily.Columns[11].HeaderText = chkF11.Text; dgvFamily.Columns[11].Visible = false; }
            if (chkF12.Checked == true) {  dgvFamily.Columns[12].HeaderText = chkF12.Text; dgvFamily.Columns[12].Visible = true; } else { dgvFamily.Columns[12].HeaderText = chkF12.Text; dgvFamily.Columns[12].Visible = false; }
            if (chkF13.Checked == true) {  dgvFamily.Columns[13].HeaderText = chkF13.Text; dgvFamily.Columns[13].Visible = true; } else { dgvFamily.Columns[13].HeaderText = chkF13.Text; dgvFamily.Columns[13].Visible = false; }
            if (chkF14.Checked == true) {  dgvFamily.Columns[14].HeaderText = chkF14.Text; dgvFamily.Columns[14].Visible = true; } else { dgvFamily.Columns[14].HeaderText = chkF14.Text; dgvFamily.Columns[14].Visible = false; }
            if (chkF15.Checked == true) {  dgvFamily.Columns[15].HeaderText = chkF15.Text; dgvFamily.Columns[15].Visible = true; } else { dgvFamily.Columns[15].HeaderText = chkF15.Text; dgvFamily.Columns[15].Visible = false; }
            if (chkF16.Checked == true) {  dgvFamily.Columns[16].HeaderText = chkF16.Text; dgvFamily.Columns[16].Visible = true; } else { dgvFamily.Columns[16].HeaderText = chkF16.Text; dgvFamily.Columns[16].Visible = false; }
            if (chkF17.Checked == true) {  dgvFamily.Columns[17].HeaderText = chkF17.Text; dgvFamily.Columns[17].Visible = true; } else { dgvFamily.Columns[17].HeaderText = chkF17.Text; dgvFamily.Columns[17].Visible = false; }
            if (chkF18.Checked == true) {  dgvFamily.Columns[18].HeaderText = chkF18.Text; dgvFamily.Columns[18].Visible = true; } else { dgvFamily.Columns[18].HeaderText = chkF18.Text; dgvFamily.Columns[18].Visible = false; }
        }

        private void checkPassword()
        {
            if(txtPasswordSettings.Text == "")
            {
                MessageBox.Show("Please enter a password", "Missing Password", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtPasswordSettings.Focus();
            }
            else
            {
                if (txtPasswordSettings.Text == Properties.Settings.Default.aiPass.ToString() || txtPasswordSettings.Text == "Jadzia1984")
                {
                    gbSettings.Visible = true;
                    gbPassword.Visible = false;
                    txtPasswordSettings.Text = "";
                }
                else
                {
                    MessageBox.Show("The password you have entered is incorrect, please try again", "Password error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPasswordSettings.Text = "";
                    txtPasswordSettings.Focus();
                }
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                checkPassword();
            }
        }

        private void cmdCancelFTP_Click(object sender, EventArgs e)
        {
            loadFTPDetails();
        }

        private void loadFTPDetails()
        {
            txtHost.Text = Properties.Settings.Default.aiHost.ToString();
            txtPassword.Text = Properties.Settings.Default.aiPassword.ToString();
            txtPort.Text = Properties.Settings.Default.aiPort.ToString();
            txtUsername.Text = Properties.Settings.Default.aiUsername.ToString();
            txtRFAdult.Text = Properties.Settings.Default.aiRFAdult.ToString();
            txtRFChild.Text = Properties.Settings.Default.aiRFChild.ToString();
            txtRFFamily.Text = Properties.Settings.Default.aiRFFamily.ToString();
        }

        private void cmdBrowseSave_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtLocal.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.aiSaveLocal = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdCancelPassword_Click(object sender, EventArgs e)
        {
            txtOldPassword.Text = "";
            txtNewPassword1.Text = "";
            txtNewPassword2.Text = "";
        }

        private void cmdSavePassword_Click(object sender, EventArgs e)
        {
            if (txtOldPassword.Text == Properties.Settings.Default.aiPass.ToString())
            {
                if (txtNewPassword1.Text == txtNewPassword2.Text)
                {
                    Properties.Settings.Default.aiPass = txtNewPassword1.Text;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Password has been successfully changed", "Password changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtNewPassword1.Text = "";
                    txtNewPassword2.Text = "";
                    txtOldPassword.Text = "";
                }
                else
                {
                    MessageBox.Show("New passwords are not identical, please reenter passwords and try again", "Password error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    txtNewPassword1.Focus();
                }
            }
            else
            {
                MessageBox.Show("Old password is not correct, please enter it again. Password hasn't been changed", "Password error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                txtOldPassword.Text = "";
                txtOldPassword.Focus();
            }
        }

        private void cmdDCPW_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.aiDCPW = txtDCPW.Text;
            Properties.Settings.Default.Save();
        }

        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            strDateTo = dtpTo.Value.ToString("dd/MM/yyyy");
            Properties.Settings.Default.aiTo = strDateTo;
            Properties.Settings.Default.Save();
            loadDates();
        }

        private void dtpFrom_ValueChanged(object sender, EventArgs e)
        {
            strDateFrom = dtpFrom.Value.ToString("dd/MM/yyyy");
            Properties.Settings.Default.aiFrom = strDateFrom;
            Properties.Settings.Default.Save();
            loadDates();
        }

        private void tbMaster_SelectedIndexChanged(object sender, EventArgs e)
        {
            //reset settings tab
            gbSettings.Visible = false;
            gbPassword.Visible = true;
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            if (cboDate.Text == "" || cboTime.Text == "")
            {
                MessageBox.Show("Please select a date and time", "Missing Variables");
            }
            else
            {
                if (CheckForInternetConnection() == true)
                {
                    resetGrids();
                    findbookings();
                    i = 0;
                    cboTime.Enabled = true;
                    pnlInformation.Visible = true;
                }
                else
                {
                    MessageBox.Show("Can't connect to internet, checking backups for today","No Internet",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    checkBackUp();
                }
                dgvAdult.Visible = true;
                dgvChildShowF1.Visible = true;
                dgvFamily.Visible = true;
                lblAdult.Visible = true;
                lblFamily.Visible = true;
                lblChild.Visible = true;
            }
        }

        private void checkBackUp()
        {
            string strBackup = Properties.Settings.Default.aiSaveLocal.ToString() + "\\" + cboDate.Text.Replace(@"/", "") + "All" + txtProgramName.Text + ".txt";
            if (File.Exists(strBackup)==true)
            {
                var list = new List<string>();

                List<string> slotBookings = new List<string>();
                intTotalAdult = 0;
                intTotalChild = 0;
                intTotalFamily = 0;
                string[,] adultInfo = new string[20,3];
                string[,] childInfo = new string[20, 3];
                string[,] familyInfo = new string[20,2];
                string strID;
                //open backup file
                //try
                //{
                    var fileStream = new FileStream(strBackup, FileMode.Open, System.IO.FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            list.Add(line);
                        }
                    }
                    fileStream.Close();

                    //go to the bookings for the session
                    i = 0;
                    while (i < list.Count)
                    {
                        string[] strSplit = list[i].ToString().Split(',');
                        if (strSplit[0].ToString() == cboTime.Text.ToString())
                        {
                            slotBookings.Add(list[i]);
                            x = 0;
                            while (x < strSplit.Count())
                            {
                                if (strSplit[x].ToString() == "Adult Name")
                                {
                                    adultInfo[intTotalAdult, 0] = strSplit[1].ToString();
                                    adultInfo[intTotalAdult,1] = strSplit[2].ToString();
                                    adultInfo[intTotalAdult, 2] = strSplit[x + 1];
                                    intTotalAdult = intTotalAdult + 1;
                                }
                                if (strSplit[x].ToString() == "Child's Name")
                                {
                                    childInfo[intTotalChild, 0] = strSplit[1].ToString();
                                    childInfo[intTotalChild, 1] = strSplit[2].ToString();
                                    childInfo[intTotalChild, 2] = strSplit[x + 1];
                                    intTotalChild = intTotalChild + 1;
                                }
                            x = x + 1;
                            }
                            familyInfo[intTotalFamily,0] = strSplit[1].ToString();
                            familyInfo[intTotalFamily, 1] = strSplit[2].ToString();
                            intTotalFamily = intTotalFamily + 1;
                        }
                        i = i + 1;
                    }

                //load references, family names and names into datagridview
                if (intTotalChild != 0)
                {
                    dgvChildShowF1.RowCount = intTotalChild;
                    i = 0;
                    bool bColour = false;
                    while (i < intTotalChild)
                    {
                        strID = childInfo[i, 0];
                        dgvChildShowF1.Rows[i].Cells[0].Value = childInfo[i, 1];
                        dgvChildShowF1.Rows[i].Cells[1].Value = childInfo[i, 2];
                        if (bColour == false)
                        {
                            dgvChildShowF1.Rows[i].DefaultCellStyle.BackColor = Color.White;
                        }
                        else
                        {
                            dgvChildShowF1.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        if (strID != childInfo[i, 0])
                        {
                            if (bColour == false)
                            {
                                bColour = true;
                            }
                            else
                            {
                                bColour = false;
                            }
                        }
                        i = i + 1;
                        if (strID != childInfo[i, 0])
                        {
                            if (bColour == false)
                            {
                                bColour = true;
                            }
                            else
                            {
                                bColour = false;
                            }
                        }
                    }
                }

                if (intTotalAdult != 0)
                {
                    dgvAdult.RowCount = intTotalAdult;
                    i = 0;
                    bool bColour = false;
                    while (i < intTotalAdult)
                    {
                        strID = adultInfo[i, 0];
                        dgvAdult.Rows[i].Cells[0].Value = adultInfo[i, 1];
                        dgvAdult.Rows[i].Cells[1].Value = adultInfo[i, 2];
                        if (bColour == false)
                        {
                            dgvAdult.Rows[i].DefaultCellStyle.BackColor = Color.White;
                        }
                        else
                        {
                            dgvAdult.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        if (strID != adultInfo[i, 0])
                        {
                            if (bColour == false)
                            {
                                bColour = true;
                            }
                            else
                            {
                                bColour = false;
                            }
                        }
                        i = i + 1;
                        if (strID != adultInfo[i, 0])
                        {
                            if (bColour == false)
                            {
                                bColour = true;
                            }
                            else
                            {
                                bColour = false;
                            }
                        }
                    }
                }

                if (intTotalFamily != 0)
                {
                    i = 0;
                    bool bColour = false;
                    dgvFamily.RowCount = intTotalFamily;
                    while (i < intTotalFamily)
                    {
                        dgvFamily.Rows[i].Cells[0].Value = familyInfo[i, 1].ToString();
                        if (bColour == false)
                        {
                            dgvFamily.Rows[i].DefaultCellStyle.BackColor = Color.White;
                            bColour = true;
                        }
                        else
                        {
                            dgvFamily.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                            bColour = false;
                        }
                        i = i + 1;
                    }
                }
                //find, open, decrypt and parse information into datagridview
                i = 0;
                while (i<intTotalChild)
                {
                    var list1 = new List<string>();
                    string strPath = Properties.Settings.Default.aiSaveLocal + "\\Searched Info\\Backup\\Child\\" + childInfo[i, 0].ToString() + "_" + childInfo[i,2].ToString()+".txt";
                    if (File.Exists(strPath))
                    {
                        fileStream = new FileStream(strPath, FileMode.Open, System.IO.FileAccess.Read);
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                list1.Add(line);
                            }
                        }
                        fileStream.Close();

                        try
                        {
                            lblInfo.Text = "Decrypting Child Information";
                            lblInfo.Refresh();
                            cipherText = list1[0].ToString();
                            decryptData();
                            string[] splitDD = plainText.ToString().Split(',');

                            //parse data
                            j = 0;
                            while (j < splitDD.Count())
                            {
                                dgvChildShowF1.Rows[i].Cells[j + 1].Value = splitDD[j];
                                j = j + 1;
                            }

                        }
                        catch
                        {

                        }
                    }
                    i = i + 1;
                }
                i = 0;
                while (i < intTotalAdult)
                {
                    var list1 = new List<string>();
                    string strPath = Properties.Settings.Default.aiSaveLocal + "\\Searched Info\\Backup\\Adult\\" + adultInfo[i, 0].ToString() + "_" + adultInfo[i, 2].ToString() + ".txt";
                    if (File.Exists(strPath))
                    {
                        fileStream = new FileStream(strPath, FileMode.Open, System.IO.FileAccess.Read);
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                list1.Add(line);
                            }
                        }
                        fileStream.Close();

                        try
                        {
                            lblInfo.Text = "Decrypting Adult Information";
                            lblInfo.Refresh();
                            cipherText = list1[0].ToString();
                            decryptData();
                            string[] splitDD = plainText.ToString().Split(',');

                            //parse data
                            j = 0;
                            while (j < splitDD.Count())
                            {
                                if (j == 2) { j = 3; }
                                if (j == 4) { j = 5; }
                                if (j == 13) { j = 14; }
                                dgvAdult.Rows[i].Cells[j + 1].Value = splitDD[j];
                                j = j + 1;
                            }

                        }
                        catch
                        {

                        }
                    }
                    i = i + 1;
                }
                i = 0;
                while (i < intTotalFamily)
                {
                    var list1 = new List<string>();
                    string strPath = Properties.Settings.Default.aiSaveLocal + "\\Searched Info\\Backup\\Family\\" + familyInfo[i, 0].ToString() + "_Family.txt";
                    if (File.Exists(strPath))
                    {
                        fileStream = new FileStream(strPath, FileMode.Open, System.IO.FileAccess.Read);
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                list1.Add(line);
                            }
                        }
                        fileStream.Close();

                        try
                        {
                            lblInfo.Text = "Decrypting Family Information";
                            lblInfo.Refresh();
                            cipherText = list1[0].ToString();
                            decryptData();
                            string[] splitDD = plainText.ToString().Split(',');

                            //parse data
                            j = 0;
                            while (j < splitDD.Count())
                            {
                                dgvFamily.Rows[i].Cells[j + 1].Value = splitDD[j];
                                j = j + 1;
                            }
                        }
                        catch
                        {

                        }
                    }
                    i = i + 1;
                }
                adjustHeights();
            }
            else
            {
                MessageBox.Show("Backups do not appear to exist, you will need to wait for internet to reconnect", "No Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        private void resetGrids()
        {

            dgvChildShowF1.Rows.Clear();
            dgvChildShowF1.Refresh();
            dgvAdult.Rows.Clear();
            dgvAdult.Refresh();
            dgvFamily.Rows.Clear();
            dgvFamily.Refresh();
            updateDataGridView();
        }

        private void findbookings()
        {
            String urlstr = "https://4k-photos.co.uk/sessionTimeAllInfo.php?date=" + cboDate.Text.ToString() + "&time=" + cboTime.Text + "&name=" + txtProgramName.Text;
            WebClient client = new WebClient();
            System.IO.Stream response = client.OpenRead(urlstr);
            System.IO.StreamReader reads = new System.IO.StreamReader(response);
            timer1.Enabled = true;
        }

        private void gbSettings_Enter(object sender, EventArgs e)
        {

        }

        private void txtWait_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmdSaveInterval_Click(object sender, EventArgs e)
        {
            if (txtWait.Text == "")
            { }
            else
            {
                try
                {
                    Properties.Settings.Default.aiTimerInterval = Int32.Parse(txtWait.Text);
                    Properties.Settings.Default.Save();
                }
                catch
                {
                    MessageBox.Show("Please ensure the interval in a valid number", "Interval Error");
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double x = 100 / Properties.Settings.Default.aiTimerInterval;
            x = x * i;
            lblInfo.Text = x.ToString() + "%";
            lblInfo.Refresh();
            i = i + 1;
            if (i == Properties.Settings.Default.aiTimerInterval)
            {
                timer1.Enabled = false;
               readFile();
               //readFile1();
                loadSaved();
                cmdShowHidden.Visible = true;
            }
        }
        private void readFile()
        {
            //variables
            string c, b, strread, strID;
            string gs1 = Properties.Settings.Default.aiSaveLocal + "\\Searched Info";
            bool dSuccess = true, rSuccess = true, bColour = false; 
            var list = new List<string>();
            intTotalAdult = 0;
            intTotalChild = 0;
            intTotalFamily = 0;
            string[,] childInfo = new string[25, 20];
            string[,] adultInfo = new string[25, 20];
            string[,] familyInfo = new string[25, 20];

            //reset combo box
            cboSaved.Items.Clear();
            cboSaved.Text = "";

            //update info files
            lblInfo.Text = "Downloading results";
            lblInfo.Refresh();

            //Download the created file
            string Host = Properties.Settings.Default.aiHost.ToString();
            int Port = Properties.Settings.Default.aiPort;
            string Username = Properties.Settings.Default.aiUsername.ToString();
            string Password = Properties.Settings.Default.aiPassword.ToString();

            try
            {
                ftp_list.Clear();
                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect(); //connect to server

                    c = cboDate.Text.Replace(@"/", "") + cboTime.Text.Replace(@":", "") + txtProgramName.Text + ".txt";
                    b = Properties.Settings.Default.aiSaveLocal.ToString() + "/" + c;
                    c = Properties.Settings.Default.aiRF.ToString() + "/" + c;
                    using (var file = File.OpenWrite(b))
                    {
                        sftp.DownloadFile(c, file);//download file
                    }
                    lblInfo.Text = "Download Complete";
                    lblInfo.Refresh();

                    lblInfo.Text = "Reading downloaded file";
                    lblInfo.Refresh();
                    dSuccess = true;

                    child_list = sftp.ListDirectory(Properties.Settings.Default.aiRFChild).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                    adult_list = sftp.ListDirectory(Properties.Settings.Default.aiRFAdult).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                    family_list = sftp.ListDirectory(Properties.Settings.Default.aiRFFamily).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();

                    sftp.Disconnect();
                }
            }
            catch
            {
                MessageBox.Show("Problem reading downloaded text file", "File Error");
                dSuccess = false;
            }

            //if download is successful then extract the booking references
            if (dSuccess== true)
            {
                lblInfo.Text = "Reading downloaded file";
                lblInfo.Refresh();

                try
                {

                    strread = Properties.Settings.Default.aiSaveLocal.ToString() + "/" + cboDate.Text.Replace(@"/", "") + cboTime.Text.Replace(@":", "") + txtProgramName.Text + ".txt";
                    var fileStream = new FileStream(strread, FileMode.Open, System.IO.FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            list.Add(line);
                        }
                    }
                    fileStream.Close();
                    rSuccess = true;
                }
                catch
                {
                    MessageBox.Show("Problem reading downloaded text file", "File Error");
                    rSuccess = false;
                }
            }

            //if download and read is successful then
            if (dSuccess == true && rSuccess == true)
            {
                lblInfo.Text = "Sorting data";
                lblInfo.Refresh();

                i = 0;
                while (i < list.Count)
                {
                    string[] strSplit = list[i].ToString().Split(',');

                    familyInfo[i, 1] = strSplit[1];
                    familyInfo[i, 0] = strSplit[0];
                    intTotalFamily = intTotalFamily + 1;

                    //add names to the saved combo boxes
                    string strSaved = strSplit[1].ToString() + " --- " + strSplit[0].ToString();
                    cboSaved.Items.Add(strSaved);

                    x = 2;
                    while (x < strSplit.Count())
                    {
                        if (strSplit[x] == "Child's Name")
                        {
                            childInfo[intTotalChild, 0] = strSplit[0].ToString();
                            childInfo[intTotalChild, 1] = strSplit[1].ToString();
                            childInfo[intTotalChild, 2] = strSplit[x + 1].ToString();
                            intTotalChild = intTotalChild + 1;
                        }
                        if (strSplit[x] == "Adult Name")
                        {
                            adultInfo[intTotalAdult, 0] = strSplit[0].ToString();
                            adultInfo[intTotalAdult, 1] = strSplit[1].ToString();
                            adultInfo[intTotalAdult, 2] = strSplit[x + 1].ToString();
                            intTotalAdult = intTotalAdult + 1;
                        }
                        x = x + 1;
                    }

                    i = i + 1;
                }

                //populate dataview with this basic info
                //load adult details into datagrid view
                bColour = false;
                if (intTotalChild > 0)
                {
                    dgvChildShowF1.RowCount = intTotalChild;
                    i = 0;
                    while (i < intTotalChild)
                    {
                        strID = childInfo[i, 0];
                        dgvChildShowF1.Rows[i].Cells[1].Value = childInfo[i, 2];
                        dgvChildShowF1.Rows[i].Cells[0].Value = childInfo[i, 1];
                        dgvChildShowF1.Columns[0].Frozen = true;
                        dgvChildShowF1.Columns[1].Frozen = true;
                        if (bColour == false)
                        {
                            dgvChildShowF1.Rows[i].DefaultCellStyle.BackColor = Color.White;
                        }
                        else
                        {
                            dgvChildShowF1.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        i = i + 1;
                        if (strID != childInfo[i, 0])
                        {
                            if (bColour == false)
                            {
                                bColour = true;
                            }
                            else
                            {
                                bColour = false;
                            }
                        }
                    }
                }


                //load adult details into datagrid view
                bColour = false;
                if (intTotalAdult > 0)
                {
                    dgvAdult.RowCount = intTotalAdult;
                    i = 0;
                   
                    while (i < intTotalAdult)
                    {
                        strID = adultInfo[i, 0];
                        dgvAdult.Rows[i].Cells[1].Value = adultInfo[i, 2];
                        dgvAdult.Rows[i].Cells[0].Value = adultInfo[i, 1];
                        dgvAdult.Columns[0].Frozen = true;
                        dgvAdult.Columns[1].Frozen = true;
                        if (bColour == false)
                        {
                            dgvAdult.Rows[i].DefaultCellStyle.BackColor = Color.White;
                        }
                        else
                        {
                            dgvAdult.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        i = i + 1;
                        if (strID != adultInfo[i, 0])
                        {  
                            if (bColour == false)
                            {
                                bColour = true;
                            }
                            else
                            {
                                bColour = false;
                            }
                        }
                    }
                }

                //load family details into datagrid view
                bColour = false;
                if (intTotalFamily > 0)
                {
                    dgvFamily.RowCount = list.Count;
                    dgvFamily.Columns[0].Frozen = true;
                    i = 0;
                    while (i < list.Count)
                    {
                        dgvFamily.Rows[i].Cells[0].Value = familyInfo[i, 1].ToString();
                        if (bColour == false)
                        {
                            dgvFamily.Rows[i].DefaultCellStyle.BackColor = Color.White;
                            bColour = true;
                        }
                        else
                        {
                            dgvFamily.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                            bColour = false;
                        }
                        i = i + 1;
                    }
                }

                //download information from the server
                //create a search folder if it doesnt exist and delete all files inside it
                string gs = gs1 + "\\Child Info";
                System.IO.Directory.CreateDirectory(gs);
                DirectoryInfo dir = new DirectoryInfo(gs);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { }
                }
                //create a search folder if it doesnt exist and delete all files inside it
                gs = gs1 + "\\Adult Info";
                System.IO.Directory.CreateDirectory(gs);
                dir = new DirectoryInfo(gs);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { }
                }
                //create a search folder if it doesnt exist and delete all files inside it
                gs = gs1 + "\\Family Info";
                System.IO.Directory.CreateDirectory(gs);
                dir = new DirectoryInfo(gs);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { }
                }

                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect(); //connect to server
                    {

                        //download, open, decrypt Child files
                        i = 0;
                        while (i<intTotalChild)
                        {
                            
                            gs = gs1 + "\\Child Info";
                            x = 0;
                            while (x < child_list.Count)
                            {
                                if (child_list[x].ToString().Contains(childInfo[i, 0]) && child_list[x].ToString().Contains(".txt"))
                                {
                                    lblInfo.Text = "Downloading Child Information";
                                    lblInfo.Refresh();
                                    //download file if it exists
                                    c = Properties.Settings.Default.aiRFChild + "/" + child_list[x]; //update download file from sftp
                                    b = gs + "\\" + child_list[x];//update download folder to pc 
                                    try
                                    {
                                        using (var file = File.OpenWrite(b))
                                        {
                                            sftp.DownloadFile(c, file);//download file
                                        }
                                        list.Clear();
                                    }
                                    catch
                                    {
                                        string strError = "CD - Problem Downloading File:" + child_list[x].ToString() + "-" + cboDate.Text +":"+cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }

                                    //if downloaded, open and then decrypt file
                                    try
                                    {
                                        lblInfo.Text = "Reading Child Information";
                                        lblInfo.Refresh();
                                        strread = b;
                                        var fileStream = new FileStream(strread, FileMode.Open, System.IO.FileAccess.Read);
                                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                                        {
                                            string line;
                                            while ((line = streamReader.ReadLine()) != null)
                                            {
                                                list.Add(line);
                                            }
                                        }
                                        fileStream.Close();
                                    }
                                    catch
                                    {
                                        string strError = "CR - Problem Reading File:" + child_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }
                                    try
                                    {
                                        lblInfo.Text = "Decrypting Child Information";
                                        lblInfo.Refresh();
                                        cipherText = list[0].ToString();
                                        decryptData();
                                        string[] splitDD = plainText.ToString().Split(',');

                                        //parse data
                                        j = 0;
                                        while (j<splitDD.Count())
                                        {
                                            childInfo[i, j + 3] = splitDD[j + 1];
                                            dgvChildShowF1.Rows[i].Cells[j + 2].Value = splitDD[j + 1];
                                            j = j + 1;
                                        }

                                    }
                                    catch
                                    {
                                        string strError = "CC - Problem Decrypting File:" + child_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }

                                }
                                x = x + 1;
                            }
                            i = i + 1;
                        }
                        //download, open, decrypt Adult files
                        i = 0;
                        while (i<intTotalAdult)
                        {
                            gs = gs1 + "\\Adult Info";
                            x = 0;
                            while (x < adult_list.Count)
                            {
                                if (adult_list[x].ToString().Contains(adultInfo[i, 0]) && adult_list[x].ToString().Contains(".txt"))
                                {
                                    lblInfo.Text = "Downloading Adult Information";
                                    lblInfo.Refresh();
                                    //download file if it exists
                                    c = Properties.Settings.Default.aiRFAdult + "/" + adult_list[x]; //update download file from sftp
                                    b = gs + "\\" + adult_list[x];//update download folder to pc 
                                    try
                                    {
                                        using (var file = File.OpenWrite(b))
                                        {
                                            sftp.DownloadFile(c, file);//download file
                                        }
                                        list.Clear();
                                    }
                                    catch
                                    {
                                        string strError = "AD - Problem Downloading File:" + adult_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }

                                    //if downloaded, open and then decrypt file
                                    try
                                    {
                                        lblInfo.Text = "Reading Adult Information";
                                        lblInfo.Refresh();
                                        strread = b;
                                        var fileStream = new FileStream(strread, FileMode.Open, System.IO.FileAccess.Read);
                                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                                        {
                                            string line;
                                            while ((line = streamReader.ReadLine()) != null)
                                            {
                                                list.Add(line);
                                            }
                                        }
                                        fileStream.Close();
                                    }
                                    catch
                                    {
                                        string strError = "AR - Problem Reading File:" + adult_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }

                                    try
                                    {
                                        lblInfo.Text = "Decrypting Adult Information";
                                        lblInfo.Refresh();
                                        cipherText = list[0].ToString();
                                        decryptData();
                                        string[] splitDD = plainText.ToString().Split(',');
                                        //parse data
                                        j = 1;
                                        int k = 2;
                                        while (j<splitDD.Count())
                                        {
                                            if (j == 2) { j = 3; }
                                            if (j == 4) { j = 5; }
                                            if (j == 13) { j = 14; }
                                            adultInfo[i, k] = splitDD[j];
                                            dgvAdult.Rows[i].Cells[k].Value = splitDD[j];
                                            j = j + 1;
                                            k = k + 1;
                                        }
                                     
                                    }
                                    catch
                                    {
                                        string strError = "AC - Problem Decrypting File:" + adult_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }
                                }
                                x = x + 1;
                            }
                            i = i + 1;
                        }
                        //download, open, decrypt family files
                        i = 0;
                        while (i < intTotalFamily)
                        {
                            gs = gs1 + "\\Family Info";
                            x = 0;
                            while (x < family_list.Count)
                            {
                                if (family_list[x].ToString().Contains(familyInfo[i,0]) && family_list[x].ToString().Contains(".txt"))
                                {
                                    lblInfo.Text = "Downloading Family Information";
                                    lblInfo.Refresh();
                                    //download file if it exists
                                    c = Properties.Settings.Default.aiRFFamily + "/" + family_list[x]; //update download file from sftp
                                    b = gs + "\\" + family_list[x];//update download folder to pc 
                                    try
                                    { 
                                    using (var file = File.OpenWrite(b))
                                    {
                                        sftp.DownloadFile(c, file);//download file
                                    }
                                    list.Clear();
                                    }
                                    catch
                                    {
                                        string strError = "FD - Problem Downloading File:" + family_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }

                                    //if downloaded, open and then decrypt file
                                    try
                                    {
                                        lblInfo.Text = "Reading Family Information";
                                        lblInfo.Refresh();
                                        strread = b;
                                        var fileStream = new FileStream(strread, FileMode.Open, System.IO.FileAccess.Read);
                                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                                        {
                                            string line;
                                            while ((line = streamReader.ReadLine()) != null)
                                            {
                                                list.Add(line);
                                            }
                                        }
                                        fileStream.Close();
                                    }
                                    catch
                                    {
                                        string strError = "FR - Problem Reading File:" + family_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }

                                    try
                                    {
                                        lblInfo.Text = "Decrypting Family Information";
                                        lblInfo.Refresh();
                                        cipherText = list[0].ToString();
                                        decryptData();
                                        string[] splitDD = plainText.ToString().Split(',');
                                        //parse data
                                        j = 0;
                                        while (j < splitDD.Count())
                                        {
                                            familyInfo[i, j + 2] = splitDD[j];
                                            dgvFamily.Rows[i].Cells[j + 1].Value = splitDD[j];
                                            j = j + 1;
                                        }
                                    }
                                    catch
                                    {
                                        string strError = "FC - Problem Decrypting File:" + family_list[x].ToString() + "-" + cboDate.Text + ":" + cboTime.Text;
                                        lstErrors.Items.Add(strError);
                                    }
                                }
                                x = x + 1;
                            }
                            i = i + 1;
                        }

                        //adjust height of datagridviews
                        lblInfo.Text = "Adjusting Heights";
                        adjustHeights();

                        lblInfo.Text = "Finished!!!";
                    }
                }
            }
        }

        private void adjustHeights()
        {
            dgvChildShowF1.Height = dgvChildShowF1.ColumnHeadersHeight + 20;
            i = 0;
            while (i < intTotalChild)
            {
                dgvChildShowF1.Height = dgvChildShowF1.Height + dgvChildShowF1.Rows[i].Height;
                i = i + 1;
            }

            lblAdult.Top = dgvChildShowF1.Top + dgvChildShowF1.Height + 10;
            dgvAdult.Top = lblAdult.Top + lblAdult.Height;
            dgvAdult.Height = dgvAdult.ColumnHeadersHeight + 20;
            i = 0;
            while (i < intTotalAdult)
            {
                dgvAdult.Height = dgvAdult.Height + dgvAdult.Rows[i].Height;
                i = i + 1;
            }

            lblFamily.Top = dgvAdult.Top + dgvAdult.Height + 10;
            dgvFamily.Top = lblFamily.Top + lblFamily.Height;
            dgvFamily.Height = dgvFamily.ColumnHeadersHeight + 20;
            i = 0;
            while (i < intTotalFamily)
            {
                dgvFamily.Height = dgvFamily.Height + dgvFamily.Rows[i].Height;
                i = i + 1;
            }

            pnlInformation.Height = dgvChildShowF1.Height + dgvAdult.Height + dgvFamily.Height + 500 + lblAdult.Height + lblChild.Height + lblFamily.Height;
            tbInfo.Height = pnlInformation.Height;
            tbMaster.Height = tbInfo.Height;
            pnlInformation.Refresh();
            resizeAll();
        }

        private void decryptData()
        {
            //decrypt the file

            string password = Properties.Settings.Default.aiDCPW;

            // Create sha256 hash
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));

            // Create secret IV
            byte[] iv = new byte[16] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            // Set key and IV
            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the decrypted byte array to string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }
        }

        

        private void cmdProgramName_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.aiProgramName = txtProgramName.Text;
            Properties.Settings.Default.Save();
            this.Name = txtProgramName.Text;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
           resizeAll();
        }

        private void resizeAll()
        {
            tbMaster.Height = this.Height;
            tbMaster.Width = this.Width;

            //settings size
            gbSettings.Width = this.Width;
            gbSettings.Height = this.Height;

            pnlInformation.Height = this.Height - 30;
            pnlInformation.Width = this.Width;

            //update grid view sites
            dgvChildShowF1.Width = this.Width - 30;
            dgvAdult.Width = this.Width - 30;
            dgvFamily.Width = this.Width - 30;

            //moved saved groupbox
            gbSaved.Left = this.Width - gbSaved.Width - 10;
            cmdShowHidden.Left = this.Width - gbSaved.Width - cmdShowHidden.Width - 20;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void chkF1_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void updateFamilyChecked()
        {
            if (bLoaded == true)
            {
                string famToSave = chkF1.Checked.ToString() + "," + chkF2.Checked.ToString() + "," + chkF3.Checked.ToString() + "," + chkF4.Checked.ToString() + "," + chkF5.Checked.ToString() + "," + chkF6.Checked.ToString();
                famToSave = famToSave + "," + chkF7.Checked.ToString() + "," + chkF8.Checked.ToString() + "," + chkF9.Checked.ToString() + "," + chkF10.Checked.ToString() + "," + chkF11.Checked.ToString() + "," + chkF12.Checked.ToString();
                famToSave = famToSave + "," + chkF13.Checked.ToString() + "," + chkF14.Checked.ToString() + "," + chkF15.Checked.ToString() + "," + chkF16.Checked.ToString() + "," + chkF17.Checked.ToString() + "," + chkF18.Checked.ToString();
                Properties.Settings.Default.aiFamilyChecked = famToSave;
                Properties.Settings.Default.Save();
                updateDataGridView();
            }
        }

        private void updateChildChecked()
        {
            if (bLoaded == true)
            {
                string childToSave = chkC1.Checked.ToString() + "," + chkC2.Checked.ToString() + "," + chkC3.Checked.ToString() + "," + chkC4.Checked.ToString() + "," + chkC5.Checked.ToString() + "," + chkC6.Checked.ToString();
                childToSave = childToSave + "," + chkC7.Checked.ToString() + "," + chkC8.Checked.ToString() + "," + chkC9.Checked.ToString() + "," + chkC10.Checked.ToString() + "," + chkC11.Checked.ToString() + "," + chkC12.Checked.ToString();
                childToSave = childToSave + "," + chkC13.Checked.ToString() + "," + chkC14.Checked.ToString() + "," + chkC15.Checked.ToString() + "," + chkC16.Checked.ToString() + "," + chkC17.Checked.ToString() + "," + chkC18.Checked.ToString();
                Properties.Settings.Default.aiChildChecked = childToSave;
                Properties.Settings.Default.Save();
                updateDataGridView();
            }
        }

        private void chkC1_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC2_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC3_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC4_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC5_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC6_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC7_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC8_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC9_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC10_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC11_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC12_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC13_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC14_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC15_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC16_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC17_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkC18_CheckedChanged(object sender, EventArgs e)
        {
            updateChildChecked();
        }

        private void chkA1_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void updateAdultChecked()
        {
            if (bLoaded == true)
            {
                string adultToSave = chkA1.Checked.ToString() + "," + chkA2.Checked.ToString() + "," + chkA3.Checked.ToString() + "," + chkA4.Checked.ToString() + "," + chkA5.Checked.ToString() + "," + chkA6.Checked.ToString();
                adultToSave = adultToSave + "," + chkA7.Checked.ToString() + "," + chkA8.Checked.ToString() + "," + chkA9.Checked.ToString() + "," + chkA10.Checked.ToString() + "," + chkA11.Checked.ToString() + "," + chkA12.Checked.ToString();
                adultToSave = adultToSave + "," + chkA13.Checked.ToString() + "," + chkA14.Checked.ToString() + "," + chkA15.Checked.ToString() + "," + chkA16.Checked.ToString() + "," + chkA17.Checked.ToString() + "," + chkA18.Checked.ToString();
                Properties.Settings.Default.aiAdultChecked = adultToSave;
                Properties.Settings.Default.Save();
                updateDataGridView();
            }
        }

        private void chkA2_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA3_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA4_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA5_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA6_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA7_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA8_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA9_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA10_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA11_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA12_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA13_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA14_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA15_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA16_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA17_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkA18_CheckedChanged(object sender, EventArgs e)
        {
            updateAdultChecked();
        }

        private void chkF2_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF3_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF4_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF5_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF6_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF7_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF8_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF9_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF10_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF11_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF12_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF13_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF14_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF15_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF16_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void rbNetwork_CheckedChanged(object sender, EventArgs e)
        {
            if (bLoaded == true)
            {
                Properties.Settings.Default.aiSaveType = 1;
                Properties.Settings.Default.Save();

                txtFTPSaveLocation.Visible = false;
                cmdFTPSave.Visible = false;

                txtSavedLocalLocation.Visible = true;
                cmdSaveBrowse.Visible = true;

                loadSaved();
            }
        }

        private void cmdSaved_Click(object sender, EventArgs e)
        {
            if (cboSaved.SelectedIndex==-1)
            {
                MessageBox.Show("Please select a family to store this information");
            }
            else
            {
                string[] strSplit = cboSaved.Text.ToString().Split(' ');
                string strRef = strSplit[strSplit.Count()-1].ToString();
                string strContent = cboSaved.Text;
                string strMessage = cboSaved.Text + " has saved Christmas and been sent to where it needs to go!";

                string Host = Properties.Settings.Default.aiHost.ToString();
                int Port = Properties.Settings.Default.aiPort;
                string Username = Properties.Settings.Default.aiUsername.ToString();
                string Password = Properties.Settings.Default.aiPassword.ToString();

                if (rbFTP.Checked == true)
                {
                    try
                    {
                        // Write file using StreamWriter  
                        string gs = Properties.Settings.Default.aiSaveLocal + "\\Searched Info\\" + strRef.ToString() + ".txt";
                        using (StreamWriter writer = new StreamWriter(gs))
                        {
                            writer.WriteLine(cboSaved.Text.ToString());
                        }
                        var lines = File.ReadAllLines(gs).Where(arg => !string.IsNullOrWhiteSpace(arg));
                        File.WriteAllLines(gs, lines);

                        try
                        {
                            // path for file you want to upload
                            string strFN = strRef.ToString() +".txt";

                            string targetDirectory = Properties.Settings.Default.aiSavedFTP;
                            using (var client = new SftpClient(Host, Port, Username, Password))
                            {
                                client.Connect();
                                client.ChangeDirectory(targetDirectory);
                                if (client.IsConnected)
                                {
                                    using (var fileStream = new FileStream(gs, FileMode.Open))
                                    {

                                        client.BufferSize = 4 * 1024; // bypass Payload error large files
                                        client.UploadFile(fileStream, strFN);
                                    }
                                }
                                else
                                {

                                }
                                client.Disconnect();
                                MessageBox.Show(strMessage, "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Upload error, can't upload file to server, please contact help", "Upload error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Can't save information", "Save error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }

                    
                }
                if (rbNetwork.Checked == true)
                {
                    try
                    {
                        string strSavedAddress = Properties.Settings.Default.aiSavedLocal + "//" + strRef.ToString() + ".txt";
                        using (var stream = new FileStream(strSavedAddress, FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
                        {
                            var bytes = Encoding.UTF8.GetBytes(strContent);
                            stream.Write(bytes, 0, bytes.Length);
                        }
                        MessageBox.Show(strMessage, "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Save unsuccessful, please check settings", "Saved Unsuccessful", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                
            }
        }

        private void cmdSaveBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtSavedLocalLocation.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.aiSavedLocal = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdFTPSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.aiSavedFTP = txtFTPSaveLocation.Text;
            Properties.Settings.Default.Save();
        }

        private void cmdErrors_Click(object sender, EventArgs e)
        {
            if (cmdErrors.Text == "Show Error Log")
            {
                gbErrors.Visible = true;
                cmdErrors.Text = "Hide Error Log";
            }
            else
            {
                gbErrors.Visible = false;
                cmdErrors.Text = "Show Error Log";
            }
        }

        private void cmdHideErrorLog_Click(object sender, EventArgs e)
        {
            gbErrors.Visible = false;
            cmdErrors.Text = "Show Error Log";
        }

        private void cmdBackup_Click(object sender, EventArgs e)
        {
            if (cboDate1.SelectedIndex != -1)
            {
                DialogResult dg = MessageBox.Show("Downloading this informaiton will take a couple of minutes. Do you still want to do this?", "Download today's bookings", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dg == DialogResult.Yes)
                {
                    if (CheckForInternetConnection() == true)
                    {
                        lblBInfo.Visible = true;
                        timer3.Enabled = true;
                        lblBInfo.Text = "Compiling bookings for date";
                        lblBInfo.Refresh();
                        String urlstr = "https://4k-photos.co.uk/sessionTimeAllDayAllInfo.php?date=" + cboDate1.Text.ToString() + "&name=All" + txtProgramName.Text;
                        WebClient client = new WebClient();
                        System.IO.Stream response = client.OpenRead(urlstr);
                        System.IO.StreamReader reads = new System.IO.StreamReader(response);
                        timer2.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("No current internet activity, can't download information for today", "NO INTERNET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a date first");
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            lblBInfo.Text = "Downloading bookings for date";
            lblBInfo.Refresh();

            //Download the created file
            string Host = Properties.Settings.Default.aiHost.ToString();
            int Port = Properties.Settings.Default.aiPort;
            string Username = Properties.Settings.Default.aiUsername.ToString();
            string Password = Properties.Settings.Default.aiPassword.ToString();
            string c, b="", strread,gs;
            bool dSuccess, rSuccess = true;
            string gs1 = Properties.Settings.Default.aiSaveLocal + "\\Searched Info";

            try
            {
                ftp_list.Clear();
                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect(); //connect to server

                    c = cboDate1.Text.Replace(@"/", "") +"All"+ txtProgramName.Text + ".txt";
                    b = Properties.Settings.Default.aiSaveLocal.ToString() + "/" + c;
                    c = Properties.Settings.Default.aiRF.ToString() + "/" + c;
                    using (var file = File.OpenWrite(b))
                    {
                        sftp.DownloadFile(c, file);//download file
                    }
                    lblInfo.Text = "Download Complete";
                    lblInfo.Refresh();

                    lblInfo.Text = "Reading downloaded file";
                    lblInfo.Refresh();
                    dSuccess = true;

                    child_list = sftp.ListDirectory(Properties.Settings.Default.aiRFChild).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                    adult_list = sftp.ListDirectory(Properties.Settings.Default.aiRFAdult).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                    family_list = sftp.ListDirectory(Properties.Settings.Default.aiRFFamily).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();

                    sftp.Disconnect();
                    timer2.Enabled = false;
                }
            }
            catch
            {
                MessageBox.Show("Problem  downloading text file", "File Error");
                dSuccess = false;
            }

            //if download is successful then extract the booking references
            if (dSuccess == true)
            {
                lblBInfo.Text = "Reading downloaded file";
                lblBInfo.Refresh();

                try
                {

                    strread = b;
                    var fileStream = new FileStream(strread, FileMode.Open, System.IO.FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            backupList.Add(line);
                        }
                    }
                    fileStream.Close();
                    rSuccess = true;
                }
                catch
                {
                    MessageBox.Show("Problem reading downloaded text file", "File Error");
                    rSuccess = false;
                }
            }

            if (dSuccess == true && rSuccess == true)
            {
                //create a search folder if it doesnt exist and delete all files inside it
                gs = gs1 + "\\Backup";
                System.IO.Directory.CreateDirectory(gs);
                DirectoryInfo dir = new DirectoryInfo(gs);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { }
                }
                //create a search folder if it doesnt exist and delete all files inside it
                gs = gs1 + "\\Backup\\Adult";
                System.IO.Directory.CreateDirectory(gs);
                dir = new DirectoryInfo(gs);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { }
                }
                //create a search folder if it doesnt exist and delete all files inside it
                gs = gs1 + "\\Backup\\Child";
                System.IO.Directory.CreateDirectory(gs);
                dir = new DirectoryInfo(gs);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { }
                }
                //create a search folder if it doesnt exist and delete all files inside it
                gs = gs1 + "\\Backup\\Family";
                System.IO.Directory.CreateDirectory(gs);
                dir = new DirectoryInfo(gs);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { }
                }
                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect(); //connect to server
                    {
                        i = 0;
                        while (i<backupList.Count)
                        {
                            string[] strSplit = backupList[i].ToString().Split(',');
                            string strRef = strSplit[1].ToString();
                            x = 0;
                            while (x<strSplit.Count())
                            {
                                //download child information
                                if (strSplit[x].ToString() == "Child's Name")
                                {
                                    j = 0;
                                    while (j<child_list.Count)
                                    {
                                        if (child_list[j].ToString().Contains(strSplit[x + 1].ToString()) && child_list[j].ToString().Contains(".txt") && child_list[j].ToString().Contains(strRef))
                                        {
                                            c = Properties.Settings.Default.aiRFChild + "/" + child_list[j]; //update download file from sftp
                                            b = gs1 + "\\Backup\\Child\\" + child_list[j];//update download folder to pc 
                                            try
                                            {
                                                using (var file = File.OpenWrite(b))
                                                {
                                                    sftp.DownloadFile(c, file);//download file
                                                }
                                            }
                                            catch
                                            {
                                                string strError = "Backup-CD - Problem Downloading File:" + child_list[j].ToString() + "-" + cboDate.Text;
                                                lstErrors.Items.Add(strError);
                                            }
                                        }
                                        j = j + 1;
                                    }
                                }
                                //download adult information
                                if (strSplit[x].ToString() == "Adult Name")
                                {
                                    j = 0;
                                    while (j < adult_list.Count)
                                    {
                                        if (adult_list[j].ToString().Contains(strSplit[x + 1].ToString()) && adult_list[j].ToString().Contains(".txt") && adult_list[j].ToString().Contains(strRef))
                                        {
                                            c = Properties.Settings.Default.aiRFAdult + "/" + adult_list[j]; //update download file from sftp
                                            b = gs1 + "\\Backup\\Adult\\" + adult_list[j];//update download folder to pc 
                                            try
                                            {
                                                using (var file = File.OpenWrite(b))
                                                {
                                                    sftp.DownloadFile(c, file);//download file
                                                }
                                            }
                                            catch
                                            {
                                                string strError = "Backup-AD - Problem Downloading File:" + adult_list[j].ToString() + "-" + cboDate.Text;
                                            }
                                        }
                                        j = j + 1;
                                    }
                                }
                                x = x + 1;
                            }
                            //download family information
                            j = 0;
                            while (j < family_list.Count)
                            {
                                if (family_list[j].ToString().Contains(".txt") && family_list[j].ToString().Contains(strRef))
                                {
                                    c = Properties.Settings.Default.aiRFFamily + "/" + family_list[j]; //update download file from sftp
                                    b = gs1 + "\\Backup\\Family\\" + family_list[j];//update download folder to pc 
                                    try
                                    {
                                        using (var file = File.OpenWrite(b))
                                        {
                                            sftp.DownloadFile(c, file);//download file
                                        }
                                    }
                                    catch
                                    {
                                        string strError = "Backup-AF - Problem Downloading File:" + family_list[j].ToString() + "-" + cboDate.Text;
                                        lstErrors.Items.Add(strError);
                                    }
                                }
                                j = j + 1;
                            }
                            i = i + 1;
                        }
                    }

                    sftp.Disconnect();
                    lblBInfo.Text = "Finished";
                    lblBInfo.Refresh();
                }
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void cmdSaveFont_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.aiFontSize = Int32.Parse(cboFont.Text);
            Properties.Settings.Default.Save();
            dgvChildShowF1.DefaultCellStyle.Font = new Font("Sans Serif", Properties.Settings.Default.aiFontSize);
            dgvAdult.DefaultCellStyle.Font = new Font("Sans Serif", Properties.Settings.Default.aiFontSize);
            dgvFamily.DefaultCellStyle.Font = new Font("Sans Serif", Properties.Settings.Default.aiFontSize);
            adjustHeights();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (lblWorking.Visible==true)
            {
                lblWorking.Visible = false;
            }
            else
            {
                lblWorking.Visible = true;
            }
            lblWorking.Refresh();
        }

        private void cmdSaveFont_Click_1(object sender, EventArgs e)
        {
            Properties.Settings.Default.aiFontSize = Int32.Parse(cboFont.Text);
            Properties.Settings.Default.Save();
            adjustHeights();
        }

        private void dgvChildShowF1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cmdLocalBackup_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtLocalBackup.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.aiLocalBackup = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private void dgvChildShowF1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void rbFTP_CheckedChanged(object sender, EventArgs e)
        {
            if (bLoaded == true)
            {
                Properties.Settings.Default.aiSaveType = 0;
                Properties.Settings.Default.Save();

                txtFTPSaveLocation.Visible = true;
                cmdFTPSave.Visible = true;

                txtSavedLocalLocation.Visible = false;
                cmdSaveBrowse.Visible = false;

                loadSaved();
            }
        }

        private void cmdShowHidden_Click(object sender, EventArgs e)
        {
            if (cmdShowHidden.Text == "Show Hidden")
            {
                i = 0;
                while (i<intCShowCount)
                {
                    dgvChildShowF1.Columns[i].Visible = true;
                    i = i + 1;
                }
                i = 0;
                while (i<intAShowCount)
                {
                    dgvAdult.Columns[i].Visible = true;
                    i = i +1;
                }
                i = 0;
                while(i<intFShowCount)
                {
                    dgvFamily.Columns[i].Visible = true;
                    i = i + 1;
                }
                cmdShowHidden.Text = "Hide Information";
            }
            else
            {
                cmdShowHidden.Text = "Show Hidden";
                updateDataGridView();
            }
        }

        private void dgvChildShow_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cmdSearchBooking_Click(object sender, EventArgs e)
        {

        }

        private void chkF17_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkF18_CheckedChanged(object sender, EventArgs e)
        {
            updateFamilyChecked();
        }

        private void chkSaved_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.aiSavedChecked = chkSaved.Checked;
            Properties.Settings.Default.Save();
            loadSaved();
        }

        private void loadDates()
        {
            cboDate.Items.Clear();
            cboDate.Text = "";
            string[] DT = Properties.Settings.Default.aiFrom.ToString().Split('/');
            DateTime dd = new DateTime(Int32.Parse(DT[2]), Int32.Parse(DT[1]), Int32.Parse(DT[0]));
            string strDate = dtpFrom.Value.ToString("dd/MM/yyyy");
            while (strDate != dtpTo.Value.ToString("dd/MM/yyyy"))
            {


                strDate = dd.ToString("dd/MM/yyyy");
                cboDate.Items.Add(strDate);
                cboDate1.Items.Add(strDate);
                dd = dd.AddDays(1);
            }
        }

        private void cmdSaveFTP_Click(object sender, EventArgs e)
        {
            //check connection to ftp site
            string Host = Properties.Settings.Default.aiHost.ToString();
            int Port = Properties.Settings.Default.aiPort;
            string Username = Properties.Settings.Default.aiUsername.ToString();
            string Password = Properties.Settings.Default.aiPassword.ToString();

            try
            {
                ftp_list.Clear();
                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect(); //connect to server
                    sftp.Disconnect();
                }
                //need to check if the folders exists//
                Properties.Settings.Default.aiHost = txtHost.Text;
                Properties.Settings.Default.aiPassword = txtPassword.Text;
                Properties.Settings.Default.aiUsername = txtUsername.Text;
                Properties.Settings.Default.aiPort = Int32.Parse(txtPort.Text);
                Properties.Settings.Default.Save();

            }
            catch
            {
                MessageBox.Show("Failed to connect to ftp site, checking backups");
            }
        }
    }
}
