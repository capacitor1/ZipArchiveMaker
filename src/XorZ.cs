using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal class XorZ
    {
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level, long splitsize,byte key)
        {
            if (save == string.Empty) return;
            if(splitsize == long.MaxValue)
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
                zipStream = new ZipOutputStream(new XoredStream(new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read), key));
            }
            else
            {
                zipStream = new ZipOutputStream(new XoredStream(new FileStream(save + ".part" + now.ToString("D3"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read), key));
            }
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    if (exists >= splitsize)
                    {
                        zipStream.SetComment($"PartIndex = {now}");
                        await zipStream.DisposeAsync();
                        now++;
                        exists = 0;
                        zipStream = new ZipOutputStream(new FileStream(save + ".part" + now.ToString("D3"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
                    }
                    exists += await FakeSplit1.AddEntry(item, Path.GetFileName(item), zipStream, level);

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
                        }
                        exists += await FakeSplit1.AddEntry(a, epath, zipStream, level);
                        progressBar.Value++;
                    }
                }
            }
            if(splitsize < long.MaxValue) zipStream.SetComment($"PartIndex = {now}");
            await zipStream.DisposeAsync();
        }
    }
}
