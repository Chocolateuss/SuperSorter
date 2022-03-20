using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using ExifLib;

namespace Super_Sorter
{
    public partial class Form1 : Form
    {

        public static List<Photo> photosScaned = new List<Photo>();
        public static List<Photo> videosScaned = new List<Photo>();
        bool videoSortFinished;
        bool photoSortFinished;
        delegate void labelTextDelegate(string text);
        int filesScaned = 0;
        int sorted;
        public Form1()
        {
            InitializeComponent();
        }
        string GetSeason(int month)
        {
            if (month >= 1 && month <= 2 || month == 12)
                return "Зима";
            if (month >= 3 && month <= 5)
                return "Весна";
            if (month >= 6 && month <= 8)
                return "Лето";
            if (month >= 9 && month <= 11)
                return "Осень";
            return "";
        }
        void Sort()
        {
            BeginInvoke(new Action(() =>
            {
                listView1.Items.Clear();
                progressBar1.Visible = true;
            }));
            videoSortFinished = false;
            photoSortFinished = false;
            foreach (string scanfolder in listBox1.Items)
            {
                filesScaned = 0;
                string[] files = Directory.GetFiles(scanfolder, "*", SearchOption.AllDirectories);
                
                foreach (string filename in files)
                {
                    filesScaned++;
                    BeginInvoke(new Action(() =>
                    {
                        progressBar1.Value = filesScaned / files.Length * 100;
                        status.Text = scanfolder + " / " + filename;
                    }));
                    Photo p = new Photo();
                    p.filename = filename;
                    FileInfo info = new FileInfo(filename);
                    p.basename = info.Name;
                    try
                    {
                        using (ExifReader reader = new ExifReader(filename))
                        {
                            DateTime datePictureTaken;
                            reader.GetTagValue<DateTime>(ExifTags.DateTimeOriginal, out datePictureTaken);
                            p.year = datePictureTaken.Year;
                            p.season = GetSeason(datePictureTaken.Month);
                        }
                    }
                    catch
                    {
                        p.year = info.CreationTime.Year;
                        p.season = GetSeason(info.CreationTime.Month);
                    }
                    if (p.year == 0 || p.year == 1)
                    {
                        p.year = info.CreationTime.Year;
                        p.season = GetSeason(info.CreationTime.Month);
                    }
                    if (info.Extension.ToLower() == ".jpg" ||
                        info.Extension.ToLower() == ".png" ||
                        info.Extension.ToLower() == ".gif" ||
                        info.Extension.ToLower() == ".jpeg" ||
                        info.Extension.ToLower() == ".bmp" ||
                        info.Extension.ToLower() == ".tif")
                    {
                        photosScaned.Add(p);
                    }
                    if (info.Extension.ToLower() == ".mp4" ||
                        info.Extension.ToLower() == ".mov" ||
                        info.Extension.ToLower() == ".wmv" ||
                        info.Extension.ToLower() == ".avi" ||
                        info.Extension.ToLower() == ".flv" ||
                        info.Extension.ToLower() == ".mkv" ||
                        info.Extension.ToLower() == ".3gp")
                    {
                        videosScaned.Add(p);
                    }
                }
            }
            if (checkBox1.Checked)
            {
                Directory.CreateDirectory(textBox2.Text + "\\Фотографии");
                Directory.CreateDirectory(textBox2.Text + "\\Видео");
            }
            LabelText("Состояние: сортировка");
            Thread videosThread = new Thread(videoSort);
            Thread photosThread = new Thread(photoSort);
            videosThread.Start();
            photosThread.Start();
        }
        void videoSort()
        {
            string videosDir = textBox2.Text + "\\Видео";
            foreach (Photo file in videosScaned)
            {
                sorted++;
                BeginInvoke(new Action(() =>
                {
                    status.Text = file.filename;
                    progressBar1.Value = sorted / (videosScaned.Count + photosScaned.Count) * 100;
                }));
                try
                {
                    if (checkBox2.Checked)
                    {
                        if (checkBox1.Checked)
                        {
                            if (!Directory.Exists(videosDir + "\\" + file.year + "\\" + file.season))
                                Directory.CreateDirectory(videosDir + "\\" + file.year + "\\" + file.season);
                            File.Copy(file.filename, videosDir + "\\" + file.year + "\\" + file.season + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                        else
                        {
                            if (!Directory.Exists(textBox2.Text + "\\" + file.year + "\\" + file.season))
                                Directory.CreateDirectory(textBox2.Text + "\\" + file.year + "\\" + file.season);
                            File.Copy(file.filename, textBox2.Text + "\\" + file.year + "\\" + file.season + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                    }
                    else
                    {
                        if (checkBox1.Checked)
                        {
                            if (!Directory.Exists(videosDir + "\\" + file.year))
                                Directory.CreateDirectory(videosDir + "\\" + file.year);
                            File.Copy(file.filename, videosDir + "\\" + file.year + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                        else
                        {
                            if (!Directory.Exists(textBox2.Text + "\\" + file.year))
                                Directory.CreateDirectory(textBox2.Text + "\\" + file.year);
                            File.Copy(file.filename, textBox2.Text + "\\" + file.year + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                    }
                }
                catch
                {

                }
            }
            videoSortFinished = true;
            if (photoSortFinished && videoSortFinished)
            {
                BeginInvoke(new Action(() =>
                {
                    progressBar1.Visible = false;
                    LabelText("Состояние: завершено");
                    MessageBox.Show("Сортировка файлов завершена успешно", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    status.Text = "";
                    if (checkBox4.Checked)
                        View();
                }));
            }
        }
        void photoSort()
        {
            string photosDir = textBox2.Text + "\\Фотографии";
            foreach (Photo file in photosScaned)
            {
                sorted++;
                BeginInvoke(new Action(() =>
                {
                    status.Text = file.filename;
                    progressBar1.Value = sorted / (videosScaned.Count + photosScaned.Count) * 100;
                }));
                try
                {
                    if (checkBox2.Checked)
                    {
                        if (checkBox1.Checked)
                        {
                            if (!Directory.Exists(photosDir + "\\" + file.year + "\\" + file.season))
                                Directory.CreateDirectory(photosDir + "\\" + file.year + "\\" + file.season);
                            File.Copy(file.filename, photosDir + "\\" + file.year + "\\" + file.season + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                        else
                        {
                            if (!Directory.Exists(textBox2.Text + "\\" + file.year + "\\" + file.season))
                                Directory.CreateDirectory(textBox2.Text + "\\" + file.year + "\\" + file.season);
                            File.Copy(file.filename, textBox2.Text + "\\" + file.year + "\\" + file.season + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                    }
                    else
                    {
                        if (checkBox1.Checked)
                        {
                            if (!Directory.Exists(photosDir + "\\" + file.year))
                                Directory.CreateDirectory(photosDir + "\\" + file.year);
                            File.Copy(file.filename, photosDir + "\\" + file.year + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                        else
                        {
                            if (!Directory.Exists(textBox2.Text + "\\" + file.year))
                                Directory.CreateDirectory(textBox2.Text + "\\" + file.year);
                            File.Copy(file.filename, textBox2.Text + "\\" + file.year + "\\" + file.basename, true);
                            if (!checkBox3.Checked)
                                File.Delete(file.filename);
                        }
                    }
                }
                catch
                {

                }
            }
            photoSortFinished = true;
            if (photoSortFinished && videoSortFinished)
            {
                BeginInvoke(new Action(() =>
                {
                    progressBar1.Visible = false;
                    LabelText("Состояние: завершено");
                    MessageBox.Show("Сортировка файлов завершена успешно", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    status.Text = "";
                    if (checkBox4.Checked)
                        View();
                }));
            }
        }
        private void LabelText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new labelTextDelegate(LabelText), new object[] { text });
                return;
            }
            else
            {
                label1.Text = text;
            }
        }
        void Button1Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath != "")
                listBox1.Items.Add(folderBrowserDialog1.SelectedPath);
        }
        void Button2Click(object sender, EventArgs e)
        {
            folderBrowserDialog2.ShowDialog();
            if(folderBrowserDialog2.SelectedPath != "")
                textBox2.Text = folderBrowserDialog2.SelectedPath;
        }
        void Button3Click(object sender, EventArgs e)
        {
            LabelText("Состояние: сканирование");
            Thread t = new Thread(Sort);
            t.Start();
        }
        public struct Photo
        {
            public int year;
            public string filename;
            public string basename;
            public string season;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex != -1)
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        void View()
        {
            BeginInvoke(new Action(() =>
            {
                int image = 0;
                listView1.Items.Clear();
                ImageList images = new ImageList();
                images.ImageSize = new Size(64, 64);
                foreach (Photo file in photosScaned)
                {
                    try
                    {
                        images.Images.Add(new Bitmap(file.filename));
                        ListViewItem item = new ListViewItem(new string[] { "", file.basename, file.year.ToString() });
                        item.ImageIndex = image;
                        item.Tag = file.filename;
                        image++;
                        listView1.SmallImageList = images;
                        listView1.Items.Add(item);
                    }
                    catch
                    {

                    }
                }
                foreach (Photo file in videosScaned)
                {
                    try
                    {
                        images.Images.Add(new Bitmap(Properties.Resources.not_available));
                        ListViewItem item = new ListViewItem(new string[] { "", file.basename, file.year.ToString() });
                        item.ImageIndex = image;
                        item.Tag = file.filename;
                        image++;
                        listView1.SmallImageList = images;
                        listView1.Items.Add(item);
                    }
                    catch
                    {

                    }
                }
            }));
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = listView1.SelectedItems[0].Tag.ToString();
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }
    }
}
