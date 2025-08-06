using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal class HuhZip
    {
        public static byte[] buffer = new byte[1048576];
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level, int mode, int listmode)
        {
            /* listmode
0 写入ZipComment
1 添加到每个文件Comment
2 单独写入TXT文件
3 Base64编码后写入ZipComment
4 Base64编码后单独写入TXT文件
             */
            if (save == string.Empty) return;
            if (mode < 0 || listmode < 0) return;
            SIInt = 0;
            List = "ZipEntryName,RawName\r\n";
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
            ZipOutputStream zipStream = new ZipOutputStream(new FileStream(save, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
            zipStream.SetLevel(level);
            zipStream.UseZip64 = UseZip64.On;
            progressBar.Maximum = items.Length;
            foreach (string item in items)
            {
                if (File.Exists(item))
                {
                    string proc = await GetProcessedName(mode, Path.GetFileName(item), item);
                    if(listmode == 1)
                    {
                        await AddEntry(item, proc, zipStream, level, Path.GetFileName(item));
                    }
                    else
                    {
                        await AddEntry(item, proc, zipStream, level, "");
                        List += proc + "," + Path.GetFileName(item) + "\r\n";
                    }
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
                        string proc = await GetProcessedNameFromPath(mode, epath, a);
                        if (listmode == 1)
                        {
                            await AddEntry(a,proc, zipStream, level,Path.GetFileName(a));
                        }
                        else
                        {
                            await AddEntry(a, proc, zipStream, level, "");
                            List += proc + "," + epath + "\r\n";
                        }
                        progressBar.Value++;
                    }
                }
            }
            if (listmode != 1)
            {
                switch (listmode)
                {
                    case 0:
                        zipStream.SetComment(List);
                        break;
                    case 2:
                        await File.WriteAllTextAsync(save + ".list.txt",List);
                        break;
                    case 3:
                        zipStream.SetComment(Convert.ToBase64String(Encoding.UTF8.GetBytes(List)));
                        break;
                    case 4:
                        await File.WriteAllTextAsync(save + ".list.txt", Convert.ToBase64String(Encoding.UTF8.GetBytes(List)));
                        break;
                    default:
                        break;
                }
            }
            SM.speedMonitor.Total = zipStream.Position;
            await zipStream.DisposeAsync();
        }
        public static string List = "ZipEntryName,RawName\r\n";
        public static async Task AddEntry(string item, string entrypath, ZipOutputStream zipStream, int level,string data)
        {
            var f = new ZipEntry(entrypath);
            if (level == 0) f.CompressionMethod = CompressionMethod.Stored;
            if (data != string.Empty) f.Comment = data;
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
        }
        public static string GetRandomChar(int n = 10)  
        {
            StringBuilder tmp = new StringBuilder();
            Random rand = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ+-=[]<>.{}&^%$#@!)(0123456789abcdefghijklmnopqrstuvwxyz";
            if (characters.Length < 1)
            {
                return "";
            }
            for (int i = 0; i < n; i++)
            {
                tmp.Append(characters[rand.Next(0, characters.Length)].ToString());
            }
            return tmp.ToString();
        }
        
        static async Task<string> GetSHA1(string s)
        {
            try
            {
                FileStream file = new FileStream(s, FileMode.Open);
                SHA1 sha1 = SHA1.Create();
                byte[] retval = await sha1.ComputeHashAsync(file);
                file.Close();

                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < retval.Length; i++)
                {
                    sc.Append(retval[i].ToString("x2"));
                }
                return sc.ToString();
            }
            catch
            {
                return "Null";
            }
        }
        static async Task<string> GetSHA1OfString(string s)
        {
            try
            {
                MemoryStream file = new MemoryStream();
                await file.WriteAsync(Encoding.UTF8.GetBytes(s));
                await file.FlushAsync();
                file.Position = 0;
                SHA1 sha1 = SHA1.Create();
                byte[] retval = await sha1.ComputeHashAsync(file);
                file.Close();

                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < retval.Length; i++)
                {
                    sc.Append(retval[i].ToString("x2"));
                }
                return sc.ToString();
            }
            catch
            {
                return "Null";
            }
        }
        static async Task<string> GetMD5(string s)
        {
            try
            {
                FileStream file = new FileStream(s, FileMode.Open);
                MD5 md5 = MD5.Create();
                byte[] retval = await md5.ComputeHashAsync(file);
                file.Close();

                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < retval.Length; i++)
                {
                    sc.Append(retval[i].ToString("x2"));
                }
                return sc.ToString();
            }
            catch
            {
                return "Null";
            }
        }
        static async Task<string> GetMD5OfString(string s)
        {
            try
            {
                MemoryStream file = new MemoryStream();
                await file.WriteAsync(Encoding.UTF8.GetBytes(s));
                await file.FlushAsync();
                file.Position = 0;
                MD5 md5 = MD5.Create();
                byte[] retval = await md5.ComputeHashAsync(file);
                file.Close();

                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < retval.Length; i++)
                {
                    sc.Append(retval[i].ToString("x2"));
                }
                return sc.ToString();
            }
            catch
            {
                return "Null";
            }
        }
        static int SIInt = 0;
        private static async Task<string> GetProcessedNameFromPath(int mode, string rawpath, string fpath)
        {
            string pathnew = string.Empty;
            var s = rawpath.Split('\\');
            int splits = s.Length;
            for (int i = 0; i < splits - 1; i++)
            {
                pathnew += await GetProcessedName(mode, s[i],fpath,false) + "\\";
            }
            pathnew += await GetProcessedName(mode, s.Last(),fpath);
            return pathnew;

        }
        private static async Task<string> GetProcessedName(int mode,string rawpath,string filepath,bool ispath = true)
        {
            /* mode
    0 标准UUID
    1 半UUID
    2 随机字符串
    3 Base64（路径安全）
    4 文件SHA1
    5 文件MD5
    6 自增序号
    7 自增序号（8字符HEX）
    8 转义URL
            */
            switch (mode)
            {
                case 0:
                    return Guid.NewGuid().ToString();
                case 1:
                    return Guid.NewGuid().ToString("N")[..16];
                case 2:
                    return GetRandomChar();
                case 3:
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawpath)).Replace("/","_");
                case 4:
                    return ispath ? await GetSHA1(filepath) : await GetSHA1OfString(rawpath);
                case 5:
                    return ispath ? await GetMD5(filepath) : await GetMD5OfString(rawpath);
                case 6:
                    string si = SIInt.ToString();
                    SIInt++;
                    return si;
                case 7:
                    string si1 = SIInt.ToString("X8");
                    SIInt++;
                    return si1;
                case 8:
                    return System.Net.WebUtility.UrlEncode(rawpath);
                default:
                    return "";

            }
        }
    }
}
