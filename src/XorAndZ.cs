using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal static class XorAndZ
    {
        public static byte[] buffer = new byte[1048576];
        public async static Task Run(string[] items,string save,ProgressBar progressBar,int level,byte key)
        {
            if (save == string.Empty) return;
            if (File.Exists(save))
            {
                DialogResult dr = MessageBox.Show("目标文件已经存在，是否覆盖？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if(dr == DialogResult.Yes)
                {
                    File.Delete(save);
                }
                else
                {
                    return;
                }
            }
            ZipOutputStream zipStream = new ZipOutputStream(new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if(File.Exists (item))
                {
                    await AddEntry(item, Path.GetFileName(item),zipStream,level,key);
                    progressBar.Value++;
                }
                else if (Directory.Exists(item))
                {
                    string[] thisadd = Directory.GetFiles(item,"*.*",SearchOption.AllDirectories);
                    progressBar.Maximum += thisadd.Length;
                    foreach(string a in  thisadd)
                    {
                        var p = Directory.GetParent(item);
                        string epath = p == null ? a : Path.GetRelativePath(p.FullName, a) ;
                        await AddEntry(a,epath,zipStream,level,key);
                        progressBar.Value++;
                    }
                }
            }
            await zipStream.DisposeAsync();
        }
        public static async Task AddEntry(string item, string entrypath,ZipOutputStream zipStream,int level,byte key)
        {
            var f = new ZipEntry(entrypath);
            if (level == 0) f.CompressionMethod = CompressionMethod.Stored;
            zipStream.PutNextEntry(f);
            using (var fs = new XoredStream(File.OpenRead(item),key))
            {
                while (fs.Position < fs.Length)
                {
                    int r = await fs.ReadAsync(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, r);
                    SM.speedMonitor.Total = zipStream.Position;
                }
            }
        }
    }
}
