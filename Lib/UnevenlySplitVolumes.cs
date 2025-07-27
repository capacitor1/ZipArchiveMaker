using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker
{
    internal class UnevenlySplitVolumes
    {
        long cminsplit = 1024 * 1024 * 1024;
        long cmaxsplit = 5 * 1024L * 1024L * 1024L;
        byte[] buf = new byte[65536];
        public UnevenlySplitVolumes(long min,long max)
        {
            cmaxsplit = max;
            cminsplit = min;
        }
        public async Task Split(string file,string outputbasepath)
        {
            long splitseq = 1;
            FileStream fs = new(file, FileMode.Open, FileAccess.Read);
            //
            while (fs.Position < fs.Length)
            {
                if (fs.Length - fs.Position < cmaxsplit)
                {
                    cminsplit = cmaxsplit = (fs.Length - fs.Position);
                }
                long thissize = NextLong(cminsplit, cmaxsplit);
                long finishsize = fs.Position + thissize;
                FileStream encfs = new(outputbasepath + $".{splitseq.ToString().PadLeft(3, '0')}", FileMode.OpenOrCreate, FileAccess.Write);
                while (fs.Position < finishsize)
                {
                    int c = await fs.ReadAsync(buf);
                    await encfs.WriteAsync(buf, 0, c);
                    SM.speedMonitor.Total = encfs.Position;
                }
                await encfs.DisposeAsync();
                splitseq++;
            }
            await fs.DisposeAsync();
        }
        private static Random R = new Random();
        private static long NextLong(long A, long B)
        {
            long myResult = A;
            //-----
            long Max = B, Min = A;
            if (A > B)
            {
                Max = A;
                Min = B;
            }
            double Key = R.NextDouble();
            myResult = Min + (long)((Max - Min) * Key);
            //-----
            return myResult;
        }
    }
}
