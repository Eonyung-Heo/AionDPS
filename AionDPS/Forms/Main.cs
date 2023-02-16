using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AionDPS
{
    public partial class Main : Form
    {
        string log2;
        string log;

        public static Main form;

        public Main()
        {
            InitializeComponent();

            LogAnalyzer.Initialize();


        }

        private void Main_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            fortressComboBox.SelectedIndex = 0;

            form = this;

            getSettings();

            checkBox2.Checked = true;
            checkBox3.Checked = true;
        }

        private void fortressComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            guardianComboBox.Items.Clear();

            if (fortressComboBox.Text == "용계")
            {
                guardianComboBox.Items.Add("천족 수호신장");
                guardianComboBox.Items.Add("마족 수호신장");
                guardianComboBox.Items.Add("용족 수호신장");
            }
            if (fortressComboBox.Text == "어비스")
            {
                guardianComboBox.Items.Add("천족 중급 수호신장");
                guardianComboBox.Items.Add("천족 상급 수호신장");
                guardianComboBox.Items.Add("마족 중급 수호신장");
                guardianComboBox.Items.Add("마족 상급 수호신장");
                guardianComboBox.Items.Add("용족 중급 수호신장");
                guardianComboBox.Items.Add("용족 상급 수호신장");
            }
            if (fortressComboBox.Text == "심층")
            {
                guardianComboBox.Items.Add("에레슈키갈 제1 수호신장");
            }

            guardianComboBox.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serverComboBox.Text == "")
            {
                MessageBox.Show("아이온 서버 선택 해주세요");
                return;
            }

            if (guardianComboBox.Text == "")
            {
                MessageBox.Show("수호신장을 선택해 주세요");
                return;
            }
            if (textBox1.Text == "")
            {
                textBox1.Text = "<당신>";
            }

            setSettings();

            openFileDialog1.Filter = "Chat.log 파일|*.log";
            openFileDialog1.ShowDialog();


            int i = 0;
            ResultForm resultForm = new ResultForm();

            try
            {
                this.button1.Text = "집계중입니다.";
                this.button1.Enabled = false;

                this.button1.BackColor = Color.White;

                using (StreamReader sr = new StreamReader(openFileDialog1.OpenFile(), Encoding.Default, true))
                {
                    while ((log = sr.ReadLine()) != null)
                    {
                        log2 = log;
                        if (log2 == "")
                            continue;
                        LogAnalyzer.Instance.Analyze(log, guardianComboBox.Text);

                    }



                }

                resultForm.Show();
                this.Hide();

            }
            catch (Exception ex)
            {
                this.button1.BackColor = Color.FromName("ActiveCaption");

                this.button1.Text = "집 계";
                this.button1.Enabled = true;

                Console.WriteLine(ex);
                Console.WriteLine(log2);
            }

            //this.Hide();
            
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //FileOpened(openFileDialog1.OpenFile());
        }

        private void FileOpened(Stream logFile)
        {

        }

        private void 아이온폴더열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string AionFolder = Properties.Settings.Default.AionFolder;

            if (AionFolder == "")
            {
                MessageBox.Show("아이온 폴더를 찾을 수 없습니다. 파일 - 설정 메뉴에서 수동으로 지정해주세요.");
                return;
            }

            System.Diagnostics.Process.Start(AionFolder);
        }


    }
}
