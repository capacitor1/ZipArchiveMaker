using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal class ChangeHeader
    {
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level, byte[] changed)
        {
            if (save == string.Empty) return;
            if (File.Exists(save) || File.Exists(save + ".lst"))
            {
                DialogResult dr = MessageBox.Show("目标文件已经存在，是否覆盖？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes)
                {
                    if(File.Exists(save)) File.Delete(save);
                    if(File.Exists(save + ".lst")) File.Delete(save + ".lst");
                }
                else
                {
                    return;
                }
            }
            FileStream zips = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            List<long> offsets = [];
            //offsets.Add(zips.Position);
            ZipOutputStream zipStream = new ZipOutputStream(zips);
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    offsets.Add(zips.Position);
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
                        offsets.Add(zips.Position);
                        await CommonZip.AddEntry(a, epath, zipStream, level);
                        progressBar.Value++;
                    }
                }
            }
            await zipStream.DisposeAsync();
            await zips.DisposeAsync();

            FileStream after = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            FileStream os = new FileStream(save + ".lst", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            foreach (long o in offsets)
            {
                after.Position = o;
                await after.WriteAsync(changed);
                await os.WriteAsync(BitConverter.GetBytes(o));
            }
            await after.DisposeAsync();
            await os.DisposeAsync();
        }
    }
}
