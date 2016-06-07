using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using WinForms = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections;

namespace WPFplayer
{
    public partial class mp3player : Window
    {
        int d = 0;
        bool g = false;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private bool mediaPlayerIsPaused = false;

        ArrayList pl = new ArrayList();
        ListBoxItem Temp = new ListBoxItem();

        public mp3player()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            mePlayer.Volume = 1;

            Hooks.KBDHook.KeyDown += new Hooks.KBDHook.HookKeyPress(KBDHook_KeyDown);
            Hooks.KBDHook.LocalHook = false;
            Hooks.KBDHook.InstallHook();

            if (File.Exists("settings"))
            {
                using (StreamReader sr = new StreamReader("settings", System.Text.Encoding.Default))
                {
                    pbVolume.Value = Convert.ToInt32(sr.ReadLine());
                    checkBox.IsChecked = Convert.ToBoolean(sr.ReadLine());
                    checkBox1.IsChecked = Convert.ToBoolean(sr.ReadLine());
                    if (File.Exists("playlist"))
                    {
                        string[] pls = File.ReadAllLines("playlist", System.Text.Encoding.Default);
                        foreach (string line in pls)
                        {
                            listBox.Items.Add(line);
                        }

                        listBox.SelectedIndex = Convert.ToInt32(sr.ReadLine());
                        listBox_MouseDoubleClick(this,null);
                        label.Content = "Tracks: " + listBox.Items.Count;
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }

            if ((mePlayer.Source != null) && (sliProgress.Value == sliProgress.Maximum))
            {
                if (listBox.SelectedIndex != listBox.Items.Count - 1 || checkBox.IsChecked == true || checkBox1.IsChecked == true)
                    button4_Click(this, null);
                else
                    button3_Click(this, null);
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();
            g = false;

            var dialog = new WinForms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                if (Directory.Exists(dialog.SelectedPath))
                {
                    var dir = new DirectoryInfo(dialog.SelectedPath);
                    foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (file.FullName.EndsWith("mp3"))
                            listBox.Items.Add(file.FullName);
                    }
                }
                else
                {
                    var file = new FileInfo(dialog.SelectedPath);
                    if (file.FullName.EndsWith("mp3"))
                        listBox.Items.Add(file.FullName);
                }

                listBox.SelectedIndex = 0;

                listBox_MouseDoubleClick(this, null);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                if (mediaPlayerIsPaused )
                {
                    mePlayer.Play();
                    mediaPlayerIsPlaying = true;
                    mediaPlayerIsPaused = false;
                }
                else
                {
                    mePlayer.Pause();
                    mediaPlayerIsPaused = true;
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex == d)
            {
                mePlayer.Stop();
                mediaPlayerIsPlaying = false;

                int t = listBox.SelectedIndex;
                listBox.Items.RemoveAt(listBox.SelectedIndex);
                listBox.SelectedIndex = t;
                g = false;

                listBox_MouseDoubleClick(this, null);
            }
            else
            {
                listBox.Items.RemoveAt(listBox.SelectedIndex);
                if (listBox.SelectedIndex < d)
                    d--;

                label.Content = "Tracks: " + listBox.Items.Count;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void listBox_Drop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            int f;
            for (f = 0; f < s.Length; f++)
            {
                if (Directory.Exists(s[f]))
                {
                    var dir = new DirectoryInfo(s[f]);
                    foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (file.FullName.EndsWith("mp3"))
                            listBox.Items.Add(file.FullName);
                    }
                }
                else
                {
                    var file = new FileInfo(s[f]);
                    if (file.FullName.EndsWith("mp3"))
                        listBox.Items.Add(file.FullName);
                }
            }

            label.Content = "Tracks: " + listBox.Items.Count;

            if (mediaPlayerIsPlaying == false)
            {
                g = false;
                listBox.SelectedIndex = 0;
                listBox_MouseDoubleClick(this, null);
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                Random S = new Random();
                listBox.SelectedIndex = S.Next(0, listBox.Items.Count - 1);
            }
            else
            {
                if (listBox.SelectedIndex != listBox.Items.Count - 1 || checkBox.IsChecked == true)
                    listBox.SelectedIndex = d + 1;
                else
                    if (checkBox1.IsChecked == true)
                        listBox.SelectedIndex = 0;
            }

            listBox_MouseDoubleClick(this, null);

            listBox.ScrollIntoView(listBox.SelectedItem);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Hooks.KBDHook.UnInstallHook();
        }

        void KBDHook_KeyDown(Hooks.LLKHEventArgs e)
        {
            if (e.Keys == WinForms.Keys.MediaPlayPause)
            {
                button1_Click(this, null);
            }
            if (e.Keys == WinForms.Keys.MediaNextTrack)
            {
                button4_Click(this, null);
            }
            if (e.Keys == WinForms.Keys.MediaStop)
            {
                button3_Click(this, null);
            }
            if (e.Keys == WinForms.Keys.MediaPreviousTrack)
            {
                button5_Click(this, null);
            }
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                if (listBox.SelectedItem != listBox.Items[d] || g == false)
                {
                    label.Content = "Tracks: " + listBox.Items.Count;
                    this.Title = listBox.SelectedItem.ToString();
                    pl.Add(listBox.SelectedIndex);
                    d = listBox.SelectedIndex;

                    mePlayer.Source = new Uri(listBox.SelectedItem.ToString());

                    mePlayer.Play();
                    mediaPlayerIsPlaying = true;


                    for (int i = 0; i < listBox.Items.Count; i++)
                    {
                        if (listBox.Items[i].ToString() == Temp.ToString())
                            listBox.Items[i] = Temp.Content;
                    }

                    ListBoxItem item = new ListBoxItem();
                    item.Content = listBox.SelectedItem;
                    item.Foreground = Brushes.Red;
                    item.Background = Brushes.Navy;

                    Temp.Content = listBox.SelectedItem;

                    listBox.Items[listBox.SelectedIndex] = item;
                    listBox.SelectedIndex = d;

                    g = true;
                }
                else
                {
                    mePlayer.Stop();
                    mePlayer.Play();
                }
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (pl.Count > 1)
            {
                mePlayer.Stop();
                pl.RemoveAt(pl.Count - 1);
                listBox.SelectedIndex = Convert.ToInt32(pl[pl.Count - 1]);
                listBox_MouseDoubleClick(this, null);
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
            else
            {
                mePlayer.Stop();
                mePlayer.Play();
            }

        }

        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                button2_Click(this, null);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            button3_Click(this, null);
            this.Title = "mp3player";
            listBox.Items.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using (StreamWriter sw = new StreamWriter("settings", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(pbVolume.Value);
                sw.WriteLine(checkBox.IsChecked);
                sw.WriteLine(checkBox1.IsChecked);
                mePlayer.Stop();

                if (listBox.Items.Count > 0)
                {
                    sw.WriteLine(d);

                    using (StreamWriter swp = new StreamWriter("playlist", false, System.Text.Encoding.Default))
                    {
                        foreach (var ite in listBox.Items)
                        {
                            swp.WriteLine(ite.ToString().Replace("System.Windows.Controls.ListBoxItem: ", ""));
                        }
                    }
                }
                else
                {
                    if (File.Exists("playlist"))
                        File.Delete("playlist");
                }
                
            }
        }
    }
}