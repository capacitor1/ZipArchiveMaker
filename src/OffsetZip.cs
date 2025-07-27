using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal class OffsetZip
    {
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level, byte[] insert,int perzip)
        {
            if (save == string.Empty) return;
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
            FileStream zips = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            await zips.WriteAsync(new byte[] {0x50,0x4b,0x03,0x04 ,0x00,0x00,0x00,0x00 });
            if (insert.Length > 0)
            {
                await zips.WriteAsync(insert);
                await zips.FlushAsync();
            }
            List<long> offsets = [];
            int added = 0;
            SM.speedMonitor.Total = zips.Position;
            offsets.Add(zips.Position);
            ZipOutputStream zipStream = new ZipOutputStream(zips);
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    if(added >= perzip)
                    {
                        await zipStream.DisposeAsync();
                        (long, ZipOutputStream) a2 = Renew(save,level);
                        zipStream = a2.Item2;
                        offsets.Add(a2.Item1);
                        added = 0;
                    }
                    await CommonZip.AddEntry(item, Path.GetFileName(item), zipStream, level);
                    added++;
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
                        if (added >= perzip)
                        {
                            await zipStream.DisposeAsync();
                            (long, ZipOutputStream) a2 = Renew(save, level);
                            zipStream = a2.Item2;
                            offsets.Add(a2.Item1);
                            added = 0;
                        }
                        await CommonZip.AddEntry(a, epath, zipStream, level);
                        added++;
                        progressBar.Value++;
                    }
                }
            }
            await zipStream.DisposeAsync();
            zips = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            zips.Position = zips.Length - 1;
            foreach (long o in offsets)
            {
                await zips.WriteAsync(BitConverter.GetBytes(o));
            }
            zips.Position = 4;
            await zips.WriteAsync(BitConverter.GetBytes(offsets.Count));
            await zips.DisposeAsync();
        }

        private static (long, ZipOutputStream) Renew(string save,int l)
        {
            var zips = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            zips.Position = zips.Length - 1;
            long pos = zips.Position;
            ZipOutputStream z = new ZipOutputStream(zips);
            z.SetLevel(l);
            z.UseZip64 = UseZip64.On;
            return (pos ,z );
        }
    }
}
