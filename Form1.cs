using SpeedMonitorUtil;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Resources;
using System.Text;
using ZipArchiveMaker.src;

namespace ZipArchiveMaker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Mode.SelectedIndex = 0;
            CommonZIp.Visible = true;//init
            HuhMode.SelectedIndex = HuhListMode.SelectedIndex = 0;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CheckForIllegalCrossThreadCalls = false;
            SM.speedMonitor.Set(Speed, ProgressBar);
            Text += " Ver.20250726";
        }
        private void SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "请选择文件";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string file in dialog.FileNames)
                {
                    Files.Items.Add(file);
                }
            }
        }

        private void Files_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }

            }
        }

        private void Files_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                String[]? files = e.Data.GetData(DataFormats.FileDrop, false) as String[];
                if (files == null) return;
                foreach (string f in files)
                {
                    Files.Items.Add(f);
                }
            }
        }

        private void Files_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (Files.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem i in Files.SelectedItems) Files.Items.Remove(i);
                }
            }
            else if (e.Control == true && e.KeyCode == Keys.V)
            {
                try
                {
                    IDataObject? iData = Clipboard.GetDataObject();
                    if (iData == null) return;
                    //
                    string basepath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
                    if (iData.GetDataPresent(DataFormats.Text))
                    {
                        string txt = basepath + ".txt";
                        File.WriteAllText(txt, iData.GetData(DataFormats.Text, false)?.ToString());
                        Files.Items.Add(txt);

                    }
                    else if (iData.GetDataPresent(DataFormats.Bitmap))
                    {
                        string bm8 = basepath + ".bmp";
                        Bitmap bmp = (Bitmap)iData.GetData(DataFormats.Bitmap, false)!;
                        bmp.Save(bm8, ImageFormat.Png);
                        Files.Items.Add(bm8);


                    }
                    else if (iData.GetDataPresent(DataFormats.FileDrop))
                    {
                        DragEventArgs dragEventArgs = new(iData, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
                        Files_DragDrop(sender, dragEventArgs);
                    }
                    else //huh?
                    {
                        string bin = basepath + ".bin";
                        File.WriteAllBytes(bin, (byte[])iData.GetData(DataFormats.Text, false)!);
                        Files.Items.Add(bin);


                    }
                    //
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control == true && e.KeyCode == Keys.C)
            {
                string fs = string.Empty;
                if (Files.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem i in Files.SelectedItems) fs += i.Text + "\r\n";
                }
                try
                {
                    Clipboard.SetDataObject(fs, true, 10, 200);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法复制:\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Files_DoubleClick(object sender, EventArgs e)
        {
            if (Files.SelectedItems.Count > 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = Files.SelectedItems[0].Text,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private string[] I2S()
        {
            List<string> list = new List<string>();
            foreach (ListViewItem item in Files.Items)
            {
                list.Add(item.Text);
            }
            return [.. list];
        }

        private void CZ_SelectItem_Click(object sender, EventArgs e)
        {
            CZ_SavePath.Text = CommonZip.Select();
        }

        private void SZ_Select_Click(object sender, EventArgs e)
        {
            SZ_Folder.Text = SingleZip1.Select();
        }
        private void SZ1_Select_Click(object sender, EventArgs e)
        {
            SZ1_Folder.Text = SingleZip1.Select();
        }
        private void FS_Select_Click(object sender, EventArgs e)
        {
            FS_Path.Text = FakeSplit1.Select();
        }

        private void US_Select_Click(object sender, EventArgs e)
        {
            if (US_TreatAsZip.Checked)
            {
                US_Path.Text = SingleZip1.Select();
            }
            else
            {
                US_Path.Text = FakeSplit1.Select();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            EZ_Path.Text = CommonZip.Select();
        }
        private void Mode_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            IZ_Path.Text = ImageZip.Select();
        }


        private void button7_Click(object sender, EventArgs e)
        {
            SFZip.Text = ImageZip.Select();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            IZ_Img.Text = ImageZip.SelectImg();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TwoLayerZip.Text = CommonZip.Select();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            HuhZip_Path.Text = CommonZip.Select();
        }
        private void button9_Click(object sender, EventArgs e)
        {
            XZ_Path.Text = CommonZip.Select();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            SFImg1.Text = ImageZip.SelectImg();
        }
        private void button11_Click(object sender, EventArgs e)
        {
            Password.Text = HuhZip.GetRandomChar((int)W.Value);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            E_CHPath.Text = CommonZip.Select();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("""
                伪文件头将会在文件开头额外添加0x504B0304（Zip文件头标识），并在接下来的4字节内写入子Zip的总数量。
                其后再根据设置项写入插入的数据。
                然后紧跟各个子Zip，每个Zip之间不再插入数据。
                文件的结尾处则会写入一个long[]数组，写入每个子Zip的Offset。数组长度等于子Zip的总数量。
                数值均为小端序。

                编写第三方代码时，根据上述结构进行操作。
                """);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("""
                仅修改Zip的文件头（0x504B0304）部分，不影响其余数据。
                注意：此修改会将修改处的Offset单独列出表格（单独的.lst文件，整个文件为一个long[]数组，每个long代表被修改的部分的Offset），而不是将修改存储在文件内。
                数组数值为小端序。

                7-zip等常规软件均不支持读取自定义文件头的Zip，故需要自行考虑如何打开此种文件。
                """);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            E_OZPath.Text = CommonZip.Select();
        }
        private void FalseAll()//edit
        {
            SingleZip.Visible = CommonZIp.Visible = SZstd.Visible = FakeSplit.Visible = UnevenSpilt.Visible = EncryptZip.Visible = ImgZip.Visible = HuhZ.Visible
            = TLZ.Visible = SF.Visible = XorZip.Visible = E_OffsetZip.Visible = E_CH.Visible = false;
            //Thread.Sleep(1);
        }
        private void Mode_SelectedIndexChanged(object sender, EventArgs e)//edit
        {
            FalseAll();
            switch (Mode.SelectedIndex)
            {
                case 0:
                    CommonZIp.Visible = true;
                    break;
                case 1:
                    SingleZip.Visible = true;
                    break;
                case 2:
                    SZstd.Visible = true;
                    break;
                case 3:
                    FakeSplit.Visible = true;
                    break;
                case 4:
                    UnevenSpilt.Visible = true;
                    break;
                case 5:
                    EncryptZip.Visible = true;
                    break;
                case 6:
                    ImgZip.Visible = true;
                    break;
                case 7:
                    HuhZ.Visible = true;
                    break;
                case 8:
                    TLZ.Visible = true;
                    break;
                case 9:
                    SF.Visible = true;
                    break;
                case 10:
                    XorZip.Visible = true;
                    break;
                case 11:
                    E_OffsetZip.Visible = true;
                    break;
                case 12:
                    E_CH.Visible = true;
                    break;
                default:
                    break;
            }
            Application.DoEvents();
        }
        private async void Run_Click(object sender, EventArgs e)//edit
        {
            try
            {
                Run.Enabled = false;
                switch (Mode.SelectedIndex)
                {
                    case 0:
                        await CommonZip.Run(I2S(), CZ_SavePath.Text, ProgressBar, (int)CZ_Level.Value);
                        break;
                    case 1:
                        await SingleZip1.Run(I2S(), SZ_Folder.Text, ProgressBar, (int)SZ_Level.Value, SZ_KeepExt.Checked);
                        break;
                    case 2:
                        await SingleZstd1.Run(I2S(), SZ1_Folder.Text, ProgressBar, (int)SZ1_Level.Value, SZ1_KeepExt.Checked);
                        break;
                    case 3:
                        await FakeSplit1.Run(I2S(), FS_Path.Text, ProgressBar, (int)FZ_Level.Value, FakeSplit1.GetSize(FS_StrSize.Text));
                        break;
                    case 4:
                        await Uneven.Run(I2S(), US_Path.Text, ProgressBar, (int)ULevel.Value, US_TreatAsZip.Checked, FakeSplit1.GetSize(UMin.Text), FakeSplit1.GetSize(UMax.Text));
                        break;
                    case 5:
                        await EncryptZip1.Run(I2S(), EZ_Path.Text, ProgressBar, (int)Elevel.Value, Password.Text);
                        break;
                    case 6:
                        await ImageZip.Run(I2S(), IZ_Path.Text, ProgressBar, (int)IZ_Level.Value, IZ_Img.Text, [.. Enumerable.Repeat((byte)HEX.Value, (int)HEXLength.Value)]);
                        break;
                    case 7:
                        await HuhZip.Run(I2S(), HuhZip_Path.Text, ProgressBar, (int)Huh_Level.Value, HuhMode.SelectedIndex, HuhListMode.SelectedIndex);
                        break;
                    case 8:
                        await TwoLayerZip1.Run(I2S(), TwoLayerZip.Text, ProgressBar, (int)TLZLevel.Value);
                        break;
                    case 9:
                        await FakeSplitAndImage.Run(I2S(), SFZip.Text, ProgressBar, (int)SFL.Value, FakeSplit1.GetSize(SFMin.Text), SFImg1.Text, [.. Enumerable.Repeat((byte)SFHex.Value, (int)SFHexLen.Value)]);
                        break;
                    case 10:
                        await XorZ.Run(I2S(), XZ_Path.Text, ProgressBar, (int)XZ_Level.Value, FakeSplit1.GetSize(XZ_Split.Text), (byte)XZ_Key.Value);
                        break;
                    case 11:
                        await OffsetZip.Run(I2S(), E_OZPath.Text, ProgressBar, (int)E_OZLevel.Value, [.. Enumerable.Repeat((byte)E_OZHEX.Value, (int)E_OZHEXLEN.Value)], (int)E_OZPerZip.Value);
                        break;
                    case 12:
                        await ChangeHeader.Run(I2S(),E_CHPath.Text,ProgressBar,(int)E_CHLevel.Value,BitConverter.GetBytes((uint)E_CHH.Value));
                        break;
                    default:
                        break;

                }
                //

                MessageBox.Show("执行完毕！", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Run.Enabled = true;
                ProgressBar.Value = ProgressBar.Minimum = ProgressBar.Maximum = 0;
                SM.speedMonitor.Total = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Run.Enabled = true;
                SM.speedMonitor.Total = 0;
                ProgressBar.Value = ProgressBar.Minimum = ProgressBar.Maximum = 0;
            }
        }
    }

    public static class SM
    {
        public static SpeedMonitor speedMonitor = new SpeedMonitor();
    }
}
