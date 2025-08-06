using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal static class ZipInMed
    {
        public static byte[] buffer = new byte[1048576];
        public async static Task Run(string[] items,string save,ProgressBar progressBar,int level,string mediapath)
        {
            if (save == string.Empty) return;
            if (!File.Exists(mediapath)) return;

            save += Path.GetExtension(mediapath);
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
            //copy file
            using (var temp_med = File.OpenRead(mediapath))
            {
                using(var temp_sav = File.OpenWrite(save))
                {
                    while (temp_med.Position < temp_med.Length)
                    {
                        int r = await temp_med.ReadAsync(buffer, 0, buffer.Length);
                        temp_sav.Write(buffer, 0, r);
                        SM.speedMonitor.Total = temp_sav.Position;
                    }
                }
                
            }
            var FS = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            //get zip write offset
            (long,Type) woffset = await GetZipWriteOffset(FS);
            //set offset
            FS.SetLength(woffset.Item1);
            FS.Position = woffset.Item1;
            //get length
            long rawlen = FS.Length;
            ZipOutputStream zipStream = new ZipOutputStream(FS);
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if(File.Exists (item))
                {
                    await CommonZip.AddEntry(item, Path.GetFileName(item),zipStream,level);
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
                        await CommonZip.AddEntry(a,epath,zipStream,level);
                        progressBar.Value++;
                    }
                }
            }
            await zipStream.DisposeAsync();
            FS = new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            //get length
            long nowlen = FS.Length;
            //check ziplen
            long ziplen = nowlen - rawlen;
            //fix format
            FS.Position = FS.Length;
            switch (woffset.Item2)
            {
                case Type.MPEG:
                    FS.Position -= (ziplen + 16);
                    await FS.WriteAsync(new byte[] { 0x00 , 0x00 , 0x00 , 0x01 , 0x66,0x69,0x6c,0x65});
                    byte[] b = BitConverter.GetBytes(ziplen + 16);
                    Array.Reverse(b);
                    await FS.WriteAsync(b);
                    break;
                case Type.PNG:
                    FS.Position -= (ziplen + 8);
                    byte[] uintlen = BitConverter.GetBytes(ziplen > UInt32.MaxValue ? UInt32.MaxValue : (UInt32)ziplen);
                    Array.Reverse(uintlen);
                    await FS.WriteAsync(uintlen);
                    await FS.WriteAsync(new byte[] { 0x46,0x49,0x4C,0x45 });
                    FS.Position = FS.Length;
                    await FS.WriteAsync(new byte[] { 0x4e,0x55,0x4c,0x4c,0x00,0x00,0x00,0x00,0x49,0x45,0x4e,0x44,0xae,0x42,0x60,0x82});
                    break;
                case Type.RIFF:
                    FS.Position -= (ziplen + 8);
                    byte[] uintlen1 = BitConverter.GetBytes(ziplen > UInt32.MaxValue ? UInt32.MaxValue : (UInt32)ziplen);
                    await FS.WriteAsync(new byte[] { 0x66, 0x69, 0x6c, 0x65 });
                    await FS.WriteAsync(uintlen1);
                    FS.Position = 4;
                    byte[] rifflen = BitConverter.GetBytes(FS.Length - 8 > UInt32.MaxValue ? UInt32.MaxValue : (UInt32)(FS.Length - 8 ));
                    await FS.WriteAsync(rifflen);
                    break;
                case Type.Unknown:
                    break;

            }
            await FS.DisposeAsync();
        }
        public static async Task<(long,Type)> GetZipWriteOffset(FileStream fs)
        {
            //init
            fs.Position = 0;
            byte[] buffer;
            //if mpeg:
            fs.Position = 4;
            buffer = new byte[4];
            int w = await fs.ReadAsync(buffer, 0, buffer.Length);
            if (w < 4) return (fs.Length, Type.Unknown);
            if(BitConverter.ToUInt32(buffer) == 0x70797466L)
            {
                fs.Position = 1;
                int b = fs.ReadByte();
                if (b == 0) return (fs.Length + 16,Type.MPEG);
                else return (fs.Length, Type.Unknown);
            }
            //if png:
            fs.Position = 0;
            buffer = new byte[8];
            w = await fs.ReadAsync(buffer, 0, buffer.Length);
            if(w < 8) return (fs.Length, Type.Unknown);
            if (BitConverter.ToUInt64(buffer) == 0x0A1A0A0D474E5089L)
            {
                fs.Position = fs.Length - 12;
                await fs.ReadAsync(buffer, 0, buffer.Length);
                if (BitConverter.ToUInt64(buffer) == 0x444E454900000000L) return (fs.Length - 4,Type.PNG);
                else return (fs.Length, Type.Unknown);
            }
            //if RIFF:
            fs.Position = 0;
            buffer = new byte[4];
            w = await fs.ReadAsync(buffer, 0, buffer.Length);
            if (w < 4) return (fs.Length, Type.Unknown);
            if (BitConverter.ToUInt32(buffer) == 0x46464952L)
            {
                w = await fs.ReadAsync(buffer, 0, buffer.Length);
                if (w < 4) return (fs.Length, Type.Unknown);

                if (BitConverter.ToUInt32(buffer) == fs.Length - 8) return (fs.Length + 8, Type.RIFF);
                else return (fs.Length, Type.Unknown);
            }
            //default
            return (fs.Length, Type.Unknown);
        }
        public enum Type
        {
            Unknown,MPEG,PNG,RIFF
        }
    }
}
