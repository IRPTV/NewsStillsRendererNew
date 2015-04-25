using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StillsRenderer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label1.Text = ConfigurationSettings.AppSettings["StillsKind"].ToString().Trim();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.ForeColor = Color.White;
            button1.Text = "Started";
            button1.BackColor = Color.Red;

            string[] FilesList = Directory.GetFiles(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());
            foreach (string item in FilesList)
            {
                try
                {
                    if (File.GetLastAccessTime(item) < DateTime.Now.AddHours(-48))
                    {
                        File.Delete(item);
                        richTextBox1.Text += (item) + " *Deleted* \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }
                }
                catch (Exception Exp)
                {
                    richTextBox1.Text += (Exp) + " \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                }

            }
            if (CopyFiles())
            {
                Renderer();
                richTextBox1.Text = "Last Render:" + DateTime.Now.ToString();
            }
            timer1.Enabled = true;
        }
        protected bool CopyFiles()
        {
            try
            {
                timer1.Enabled = false;
                string[] Directories = Directory.GetDirectories(ConfigurationSettings.AppSettings["InputFolder"].ToString().Trim());
                if (Directories.Length > 0)
                {
                    string[] ImageFilesList = Directory.GetFiles(Directories[0] + "\\");
                    foreach (string Image in ImageFilesList)
                    {
                        if (Image.Contains(".jpg"))
                        {
                            File.Copy(Image, ConfigurationSettings.AppSettings["PicturesPath"].ToString().Trim() + "\\" + Path.GetFileName(Image), true);
                            richTextBox1.Text += Path.GetFileName(Image) + " *COPIED* \n";
                            richTextBox1.SelectionStart = richTextBox1.Text.Length;
                            richTextBox1.ScrollToCaret();
                            Application.DoEvents();
                        }
                        if (Image.Contains(".xml"))
                        {
                            File.Copy(Image, ConfigurationSettings.AppSettings["XmlPath"].ToString().Trim(), true);
                            richTextBox1.Text += Path.GetFileName(Image) + " *COPIED* \n";
                            richTextBox1.SelectionStart = richTextBox1.Text.Length;
                            richTextBox1.ScrollToCaret();
                            Application.DoEvents();
                        }
                    }
                    Directory.Delete(Directories[0], true);
                    richTextBox1.Text += (Directories[0]) + " *DELETED* \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception EXP)
            {
                richTextBox1.Text += EXP.Message;
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                return false;
            }
        }
        protected void Renderer()
        {
            timer1.Enabled = false;
            Process proc = new Process();
            proc.StartInfo.FileName = "\"" + ConfigurationSettings.AppSettings["AeRenderPath"].ToString().Trim() + "\"";
            string DateTimeStr = string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "_" + string.Format("{0:00}", DateTime.Now.Hour) + "-" + string.Format("{0:00}", DateTime.Now.Minute) + "-" + string.Format("{0:00}", DateTime.Now.Second);

            DirectoryInfo Dir = new DirectoryInfo(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());

            if (!Dir.Exists)
            {
                Dir.Create();
            }

            proc.StartInfo.Arguments = " -project " + "\"" + ConfigurationSettings.AppSettings["AeProjectFile"].ToString().Trim() + "\"" + "   -comp   \"" + ConfigurationSettings.AppSettings["Composition"].ToString().Trim() + "\" -output " + "\"" + ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4" + "\"";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.EnableRaisingEvents = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            if (!proc.Start())
            {
                return;
            }

            proc.PriorityClass = ProcessPriorityClass.Normal;
            StreamReader reader = proc.StandardOutput;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                richTextBox1.Text += (line) + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }
            proc.Close();

            try
            {
                string StaticDestFileName = ConfigurationSettings.AppSettings["ScheduleDestFileName"].ToString().Trim();
                // File.Delete(StaticDestFileName);
                File.Copy(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4", StaticDestFileName, true);
                richTextBox1.Text += "COPY FINAL:" + StaticDestFileName + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }
            catch (Exception Ex)
            {
                richTextBox1.Text += Ex.Message + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }

            timer1.Enabled = true;
            button1.ForeColor = Color.White;
            button1.Text = "Start";
            button1.BackColor = Color.Navy;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            button1_Click(new object(), new EventArgs());
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Interval = int.Parse(ConfigurationSettings.AppSettings["RenderIntervalSec"].ToString().Trim()) * 1000;
        }
    }
}
