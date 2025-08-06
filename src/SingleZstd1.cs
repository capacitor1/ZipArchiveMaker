using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace ZipArchiveMaker.src
{
    internal class SingleZstd1
    {
        public static byte[] buffer = new byte[1048576];
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level, bool keepext)
        {
            if (save == string.Empty) return;
            save = Path.Combine(save, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ffff") + ".szst");
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
                    await CreateZst(item, Path.Combine(save, pth + ".zst"), level);
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
                        await CreateZst(a, allpath + ".zst", level);
                        progressBar.Value++;
                    }
                }
            }
        }
        public static async Task CreateZst(string item, string outpath, int level)
        {
            if (File.Exists(outpath)) File.Delete(outpath);
            var output = new FileStream(outpath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            var compressionStream = new CompressionStream(output, level: level);
            var fs = File.OpenRead(item);
            while (fs.Position < fs.Length)
            {
                int r = await fs.ReadAsync(buffer, 0, buffer.Length);
                await compressionStream.WriteAsync(buffer, 0, r);
                SM.speedMonitor.Total = fs.Position;
            }
            await compressionStream.DisposeAsync();
            await fs.DisposeAsync();    
            await output.DisposeAsync();
        }
    }
}
