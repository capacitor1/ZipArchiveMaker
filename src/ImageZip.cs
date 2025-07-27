using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal class ImageZip
    {
        public static string Select()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "All|*.*";
            saveFileDialog1.Title = "保存到...";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog1.FileName;
            }
            return "";
        }
        public static string SelectImg()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件";
            dialog.Filter = "All|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return  dialog.FileName;
            }
            return "";
        }
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level,string imgpath, byte[] insert)
        {
            if (save == string.Empty) return;
            if (!File.Exists(imgpath)) return;

            save += Path.GetExtension(imgpath);
            if (File.Exists(save))
            {
                DialogResult dr = MessageBox.Show("目标文件已经存在，是否覆盖？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes)
                {
                    File.Delete(save);
                }
                else
                {
                    return;
                }
            }
            var zips = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using(var img = File.OpenRead(imgpath))
            {
                await img.CopyToAsync(zips);
            }
            SM.speedMonitor.Total = zips.Position;
            string cmt = string.Empty;
            await zips.FlushAsync();
            long imglen = zips.Position - 1;
            cmt += $"DataLength = {imglen}\r\n";

            if(insert.Length > 0)
            {
                await zips.WriteAsync(insert);
                await zips.FlushAsync();
                cmt += $"Insert = 0x{insert[0]:X2}\r\nInsertLength = {zips.Position - imglen - 1}\r\n";
            }
            cmt += $"ZipOffset = {zips.Position}";
            SM.speedMonitor.Total = zips.Position;
            ZipOutputStream zipStream = new ZipOutputStream(zips);
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    await CommonZip.AddEntry(item, Path.GetFileName(item), zipStream, level);
                    progressBar.Value++;
                }
                else if (Directory.Exists(item))
                {
                    string[] thisadd = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories);
                    progressBar.Maximum += thisadd.Length;
                    foreach (string a in thisadd)
                    {
                        var p = Directory.GetParent(item);
                        string epath = p == null ? a : Path.GetRelativePath(p.FullName, a);
                        await CommonZip.AddEntry(a, epath, zipStream, level);
                        progressBar.Value++;
                    }
                }
            }
            zipStream.SetComment(cmt);
            await zipStream.DisposeAsync();
            await zips.DisposeAsync();//test
        }
    }
}
