using ICSharpCode.SharpZipLib.Zip;

namespace ZipArchiveMaker.src
{
    internal class FakeSplitAndImage
    {
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level, long splitsize, string imgpath, byte[] insert)
        {
            if (save == string.Empty) return;
            if (!File.Exists(imgpath)) return;

            string ext = Path.GetExtension(imgpath);
            if (File.Exists(save + ".part001" + ext))
            {
                DialogResult dr = MessageBox.Show("目标文件已经存在，是否覆盖？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes)
                {
                    int i = 1;
                    while (File.Exists(save + ".part" + i.ToString("D3") + ext))
                    {
                        File.Delete(save + ".part" + i.ToString("D3") + ext);
                        i++;
                    }
                }
                else
                {
                    return;
                }
            }
            long exists = 0;
            int now = 1;
            ZipOutputStream zipStream = await Renew(save + ".part" + now.ToString("D3") + ext,imgpath,insert,now);
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    if (exists >= splitsize)
                    {
                        await zipStream.DisposeAsync();
                        now++;
                        exists = 0;
                        zipStream = await Renew(save + ".part" + now.ToString("D3") + ext, imgpath, insert, now);
                        zipStream.SetLevel(level);
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
                            await zipStream.DisposeAsync();
                            now++;
                            exists = 0;
                            zipStream = await Renew(save + ".part" + now.ToString("D3") + ext, imgpath, insert, now);
                            zipStream.SetLevel(level);
                        }
                        exists += await FakeSplit1.AddEntry(a, epath, zipStream, level);
                        progressBar.Value++;
                    }
                }
            }
            await zipStream.DisposeAsync();
        }
        private static async Task<ZipOutputStream> Renew(string path,string imgpath, byte[] insert,int partindex)
        {
            var zips = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using (var img = File.OpenRead(imgpath))
            {
                await img.CopyToAsync(zips);
                SM.speedMonitor.Total = zips.Position;
            }
            string cmt = string.Empty;
            await zips.FlushAsync();
            long imglen = zips.Position - 1;
            cmt += $"DataLength = {imglen}\r\n";

            if (insert.Length > 0)
            {
                await zips.WriteAsync(insert);
                await zips.FlushAsync();
                cmt += $"Insert = 0x{insert[0]:X2}\r\nInsertLength = {zips.Position - imglen - 1}\r\n";
            }
            cmt += $"ZipOffset = {zips.Position}\r\nPartIndex = {partindex}";
            SM.speedMonitor.Total = zips.Position;
            var zipStream = new ZipOutputStream(zips);
            zipStream.UseZip64 = UseZip64.On;
            zipStream.SetComment(cmt);
            SM.speedMonitor.Total = zipStream.Position;
            return zipStream;
        }
    }
}
