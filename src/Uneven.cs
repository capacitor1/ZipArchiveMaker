using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker.src
{
    internal class Uneven
    {
        public async static Task Run(string[] items, string save, ProgressBar progressBar, int level,bool aszip,long min ,long max)
        {
            UnevenlySplitVolumes unevenlySplitVolumes = new(min, max);
            if (aszip)
            {
                progressBar.Maximum = 2;
                progressBar.Value = 1;
                foreach (string item in items)
                {
                    if(File.Exists(Path.Combine(save, Path.GetFileName(item) + ".001")))
                    {
                        DialogResult dr = MessageBox.Show("目标文件已经存在，是否覆盖？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dr == DialogResult.Yes)
                        {
                            int i = 1;
                            while (File.Exists(Path.Combine(save, Path.GetFileName(item) + "." + i.ToString("D3")))) 
                            {
                                File.Delete(Path.Combine(save, Path.GetFileName(item) + "." + i.ToString("D3")));
                                i++;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (File.Exists(item))
                    {
                        await unevenlySplitVolumes.Split(item, Path.Combine(save, Path.GetFileName(item)));
                        //progressBar.Value++;
                    }
                }
            }
            else
            {
                if (File.Exists(Path.Combine(save + ".001")))
                {
                    DialogResult dr = MessageBox.Show("目标文件已经存在，是否覆盖？", "Tips", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dr == DialogResult.Yes)
                    {
                        int i = 1;
                        while (File.Exists(save + "." + i.ToString("D3")))
                        {
                            File.Delete(save + "." + i.ToString("D3"));
                            i++;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                await CommonZip.Run(items, save, progressBar, level);
                progressBar.Maximum = 2;
                progressBar.Value = 1;
                await unevenlySplitVolumes.Split(save, save);
                File.Delete(save);
            }
        }
    }
}
