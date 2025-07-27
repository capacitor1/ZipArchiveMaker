using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal class SingleZip1
    {
        public static string Select()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return "";
        }
        public static byte[] buffer = new byte[1048576];
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level,bool keepext)
        {
            if (save == string.Empty) return;
            save = Path.Combine(save, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ffff") + ".szip");
            if (File.Exists(save))
            {
                MessageBox.Show("存在同名文件！", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (Directory.Exists(save))
            {
                DialogResult dr = MessageBox.Show("目标文件夹已经存在，是否覆盖更新其内容？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes)
                {
                    //
                }
                else
                {
                    return;
                }
            }

            Directory.CreateDirectory(save);
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    string pth = keepext ? Path.GetFileName(item) : Path.GetFileNameWithoutExtension(item);
                    await CreateZip(item,Path.Combine(save, pth + ".zip"),level);
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
                        string allpath = Path.Combine(save, epath);
                        string? dn = Path.GetDirectoryName(allpath);
                        if (dn != null) Directory.CreateDirectory(dn);
                        allpath = keepext ? allpath : Path.Combine(dn == null ? "" : dn, Path.GetFileNameWithoutExtension(allpath)); 
                        await CreateZip(a,allpath + ".zip",level);
                        progressBar.Value++;
                    }
                }
            }
        }
        private static async Task CreateZip(string item, string outpath, int level)
        {
            if(File.Exists(outpath)) File.Delete(outpath);
            ZipOutputStream zipStream = new ZipOutputStream(new FileStream(outpath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            var f = new ZipEntry(Path.GetFileName(item));
            if (level == 0) f.CompressionMethod = CompressionMethod.Stored;
            zipStream.PutNextEntry(f);
            using (var fs = File.OpenRead(item))
            {
                while (fs.Position < fs.Length)
                {
                    int r = await fs.ReadAsync(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, r);
                    SM.speedMonitor.Total = zipStream.Position;
                }
            }
            await zipStream.DisposeAsync();
        }
    }
}
