using Microsoft.Win32;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace Daily_Wallpaper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        public class ListenLoginEvents
        {
            public Action SessionUnlockAction { get; set; }

            public void Start()
            {
                SystemEvents.SessionSwitch += SystemEvents_LoginEvents;
            }

            public void Close()
            {
                SystemEvents.SessionSwitch -= SystemEvents_LoginEvents;
            }
            ~ListenLoginEvents()
            {
                this.Close();
            }

            private void SystemEvents_LoginEvents(object sender, SessionSwitchEventArgs e)
            {
                switch (e.Reason)
                {
                    //用户登录
                    case SessionSwitchReason.SessionLogon:
                        BeginUnlock();
                        break;
                    //解锁屏
                    case SessionSwitchReason.SessionUnlock:
                        BeginUnlock();
                        break;
                    //锁屏
                    case SessionSwitchReason.SessionLock:
                        BeginLock();
                        break;
                    //注销
                    case SessionSwitchReason.SessionLogoff:
                        break;
                }
            }
            private void BeginLock() { }
            private void BeginUnlock()
            {
                Form1 form1 = new Form1();
                form1.ChangeWallpaper();
                form1.Close();
            }
        }
        public static ListenLoginEvents login_events = new ListenLoginEvents();
        public System.Timers.Timer t = new System.Timers.Timer();
        private void Run(int time)
        {
            if (time == 0)
            {
                login_events.Start();
            }
            else if (time == -1)
            {
                login_events.Close();
                t.Stop();
            }
            else
            {
                t = new System.Timers.Timer(time * 60000);
                t.Elapsed += new System.Timers.ElapsedEventHandler(ChangeWallpaper);
                t.AutoReset = true;
                t.Enabled = true;
                t.Start();
            }
        }
        public void ChangeWallpaper(object source = null, System.Timers.ElapsedEventArgs e = null)
        {
            StreamReader sr = new StreamReader("./daily_wallpaper.config", Encoding.Default);
            string time = sr.ReadLine().ToString();
            string ispowerboot = sr.ReadLine().ToString();
            int page = int.Parse(sr.ReadLine());
            sr.Close();
            page++;
            File.WriteAllText("./daily_wallpaper.config", time + "\n" + ispowerboot + "\n" + page);
            if (page > 8)
            {
                File.WriteAllText("./daily_wallpaper.config", time + "\n" + ispowerboot + "\n0");
            }
            // 官方接口有限，每天只能8张
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=" + page.ToString() + "&n=1&mkt=zh-CN");
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            Console.WriteLine(retString);
            myStreamReader.Close();
            myResponseStream.Close();
            string url = "https://cn.bing.com";
            foreach (Match match in Regex.Matches(retString, "\"url\":\"(?<url>.+?)\""))
                url += match.Groups["url"].ToString();
            HttpWebRequest request_2 = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response_2 = request_2.GetResponse() as HttpWebResponse;
            Stream responseStream = response_2.GetResponseStream();
            Stream stream = new FileStream(@"./Wallpaper/bing_wallpaper.jpg", FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            SystemParametersInfo(20, 1, Environment.CurrentDirectory + @"/Wallpaper/bing_wallpaper.jpg", 1);
        }
        private void Form1_Load(object sender = null, EventArgs e = null)
        {

            if (Directory.Exists(@"./Wallpaper/") == false)
            {
                try
                {
                    Directory.CreateDirectory(@"./Wallpaper/");
                }
                catch
                {
                    MessageBox.Show("致命错误：无法创建文件“./Wallpaper/”");
                    Close();
                }
            }
            if (!File.Exists("./daily_wallpaper.config"))
            {
                try
                {
                    File.Create("./daily_wallpaper.config").Close();
                    File.WriteAllText("./daily_wallpaper.config", "0\nTrue\n0");
                }
                catch
                {
                    MessageBox.Show("致命错误：无法创建文件“./daily_wallpaper.config”");
                    Close();
                }
            }
            StreamReader sr = new StreamReader("./daily_wallpaper.config", Encoding.Default);
            int time = int.Parse(sr.ReadLine());
            string ispowerboot = sr.ReadLine().ToString();
            sr.Close();
            if (ispowerboot == "True")
            {
                check_isopen = true;
                pictureBox14_Click();
            }
            else
            {
                check_isopen = false;
                pictureBox14_Click();
            }
            switch (time)
            {
                case 0:
                    {
                        pictureBox7_Click();
                        break;
                    }
                case 3:
                    {
                        pictureBox12_Click();
                        break;
                    }
                case 10:
                    {
                        pictureBox10_Click();
                        break;
                    }
            }
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y);
                leftFlag = true;
            }

        }
        private Point mouseOff;
        private bool leftFlag;
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);
                Location = mouseSet;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;
            }
        }

        private void pictureBox12_Click(object sender = null, EventArgs e = null) // 3mins
        {
            pictureBox12.Image = Properties.Resources.Radio_Button;
            pictureBox7.Image = Properties.Resources.Radio_Button_1;
            pictureBox10.Image = Properties.Resources.Radio_Button_1;
            StreamReader sr = new StreamReader("./daily_wallpaper.config", Encoding.Default);
            sr.ReadLine();
            string ispowerboot = sr.ReadLine().ToString();
            string page = sr.ReadLine().ToString();
            sr.Close();
            File.WriteAllText("./daily_wallpaper.config", "3\n" + ispowerboot + "\n" + page);
            Run(3);
        }

        private void pictureBox10_Click(object sender = null, EventArgs e = null) // 10mins
        {
            pictureBox12.Image = Properties.Resources.Radio_Button_1;
            pictureBox7.Image = Properties.Resources.Radio_Button_1;
            pictureBox10.Image = Properties.Resources.Radio_Button;
            StreamReader sr = new StreamReader("./daily_wallpaper.config", Encoding.Default);
            sr.ReadLine();
            string ispowerboot = sr.ReadLine().ToString();
            string page = sr.ReadLine().ToString();
            sr.Close();
            File.WriteAllText("./daily_wallpaper.config", "10\n" + ispowerboot + "\n" + page);
            Run(10);

        }

        private void pictureBox7_Click(object sender = null, EventArgs e = null) // sign
        {
            pictureBox12.Image = Properties.Resources.Radio_Button_1;
            pictureBox7.Image = Properties.Resources.Radio_Button;
            pictureBox10.Image = Properties.Resources.Radio_Button_1;
            StreamReader sr = new StreamReader("./daily_wallpaper.config", Encoding.Default);
            sr.ReadLine();
            string ispowerboot = sr.ReadLine().ToString();
            string page = sr.ReadLine().ToString();
            sr.Close();
            File.WriteAllText("./daily_wallpaper.config", "0\n" + ispowerboot + "\n" + page);
            Run(0);

        }
        private bool switch_isopen = false;
        private void pictureBox5_Click(object sender, EventArgs e) // is open
        {
            if (switch_isopen)
            {
                switch_isopen = !switch_isopen;
                pictureBox5.Image = Properties.Resources.Switch_Button;
                Form1_Load();
            }
            else
            {
                switch_isopen = !switch_isopen;
                pictureBox5.Image = Properties.Resources.Switch_Button_1;
                Run(-1);
            }
        }
        private bool check_isopen = false;
        private void pictureBox14_Click(object sender = null, EventArgs e = null) // is powerboot
        {
            string path = Application.ExecutablePath;
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (check_isopen)
            {
                check_isopen = !check_isopen;
                pictureBox14.Image = Properties.Resources.Check_Button;
                StreamReader sr = new StreamReader("./daily_wallpaper.config", Encoding.Default);
                string time = sr.ReadLine().ToString();
                sr.ReadLine();
                string page = sr.ReadLine().ToString();
                sr.Close();
                File.WriteAllText("./daily_wallpaper.config", time + "\nTrue\n" + page);
            }
            else
            {
                check_isopen = !check_isopen;
                pictureBox14.Image = Properties.Resources.Check_Button_1;
                StreamReader sr = new StreamReader("./daily_wallpaper.config", Encoding.Default);
                string time = sr.ReadLine().ToString();
                sr.ReadLine();
                string page = sr.ReadLine().ToString();
                sr.Close();
                File.WriteAllText("./daily_wallpaper.config", time + "\nFalse\n" + page);
            }
            rk2.Close();
            rk.Close();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1.BackColor = System.Drawing.Color.DarkGray;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.BackColor = System.Drawing.Color.White;
        }
        private void pictureBox1_Click(object sender, EventArgs e) // hide form
        {
            Hide();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/FengZi-lv/Daily_Wallpaper");
        }

        protected override void WndProc(ref Message m) // hide form
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE)
            {
                this.Hide();
                return;
            }
            base.WndProc(ref m);
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
            string pLocalFilePath = "./Wallpaper/bing_wallpaper.jpg";
            string pSaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString() + @"\Wallpager." + DateTime.Now.ToFileTime().ToString() + ".jpg";
            if (File.Exists(pLocalFilePath))
            {
                File.Copy(pLocalFilePath, pSaveFilePath, true);
            }
        }

        private void 显示主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void 下一张壁纸ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeWallpaper();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            login_events.Close();
            Close();
        }
    }
}
