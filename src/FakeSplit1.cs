using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZipArchiveMaker.src
{
    internal class FakeSplit1
    {
        public static string Select()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ZIP|*.zip|All|*.*";
            saveFileDialog1.Title = "保存到...";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog1.FileName;
            }
            return "";
        }
        public static byte[] buffer = new byte[1048576];
        public static long GetSize(string str)
        {
            if (str.StartsWith("None",StringComparison.OrdinalIgnoreCase)) return long.MaxValue;
            if (long.TryParse(str, out long size))
            {
                return size;
            }
            else
            {
            str:
                if (str.Last() == 'K')
                {
                    if (long.TryParse(str[..^1], out long kb)) { return kb * 1024; }
                    else return -1;
                }
                else if (str.Last() == 'M')
                {
                    if (long.TryParse(str[..^1], out long mb)) { return mb * 1024 * 1024; }
                    else return -1;
                }
                else if (str.Last() == 'G')
                {
                    if (long.TryParse(str[..^1], out long gb)) { return gb * 1024 * 1024 * 1024; }
                    else return -1;
                }
                else if (str.Last() == 'B')
                {
                    str = str[..^1];
                    goto str;
                }
            }
            return -1;
        }
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level,long splitsize)
        {
            if (save == string.Empty) return;
            if (splitsize == long.MaxValue)
            {
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
            }
            else
            {
                if (File.Exists(save + ".part001"))
                {
                    DialogResult dr = MessageBox.Show("目标文件已经存在，是否覆盖？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dr == DialogResult.Yes)
                    {
                        int i = 1;
                        while (File.Exists(save + ".part" + i.ToString("D3")))
                        {
                            File.Delete(save + ".part" + i.ToString("D3"));
                            i++;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            long exists = 0;
            int now = 1;
            ZipOutputStream zipStream;
            if (splitsize == long.MaxValue)
            {
                zipStream = new ZipOutputStream(new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
            }
            else
            {
                zipStream = new ZipOutputStream(new FileStream(save + ".part" + now.ToString("D3"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
            }
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    if(exists >= splitsize)
                    {
                        zipStream.SetComment($"PartIndex = {now}");
                        await zipStream.DisposeAsync();
                        now++;
                        exists = 0;
                        zipStream = new ZipOutputStream(new FileStream(save + ".part" + now.ToString("D3"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
                        zipStream.SetLevel(level);
                        zipStream.UseZip64 = UseZip64.On;
                    }
                    exists += await AddEntry(item, Path.GetFileName(item), zipStream, level);

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
                        if (exists >= splitsize)
                        {
                            zipStream.SetComment($"PartIndex = {now}");
                            await zipStream.DisposeAsync();
                            now++;
                            exists = 0;
                            zipStream = new ZipOutputStream(new FileStream(save + ".part" + now.ToString("D3"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
                            zipStream.SetLevel(level);
                            zipStream.UseZip64 = UseZip64.On;
                        }
                        exists += await AddEntry(a, epath, zipStream, level);
                        progressBar.Value++;
                    }
                }
            }
            if (splitsize < long.MaxValue) zipStream.SetComment($"PartIndex = {now}");
            await zipStream.DisposeAsync();
        }
        public static async Task<long> AddEntry(string item, string entrypath, ZipOutputStream zipStream, int level)
        {
            var f = new ZipEntry(entrypath);
            if (level == 0) f.CompressionMethod = CompressionMethod.Stored;
            zipStream.PutNextEntry(f);
            long tl = 0;
            using (var fs = File.OpenRead(item))
            {
                while (fs.Position < fs.Length)
                {
                    int r = await fs.ReadAsync(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, r);
                    SM.speedMonitor.Total = zipStream.Position;
                }
                tl = fs.Length;
            }
            return tl;
        }
    }
}
