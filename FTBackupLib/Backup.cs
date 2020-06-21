using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTBackupLib
{
    public class Backup
    {
        public delegate void BackupUpdateDelegate(int done, int steps);
        public event BackupUpdateDelegate BackupUpdate;
        public delegate void BackupErrorMessageDelegate(string message);
        public event BackupErrorMessageDelegate BackupErrorMessage;

        private void AppendDirectory(ref FileList list, FileFilter filter, BackupParameters param, DirectoryInfo di)
        {
            try
            {
                FileInfo[] files = di.GetFiles();
                foreach (var f in files)
                {
                    AppendFile(ref list, filter, param, f);
                }
                DirectoryInfo[] dirs = di.GetDirectories();
                foreach (var d in dirs)
                {
                    AppendDirectory(ref list, filter, param, d);
                }
            }
            catch (Exception ex)
            {
                BackupErrorMessage?.Invoke($"Failed to backup directory '{di.FullName}' - {ex.Message}");
            }
        }

        private void AppendFile(ref FileList list, FileFilter filter, BackupParameters param, FileInfo f)
        {
            if ( ( param.type == EBackupType.Incremental && f.LastWriteTime > param.lastBackup ) || param.type == EBackupType.Full )
                if (!filter.Match(f.FullName))
                {
                    list.Add(f);
                }
        }


        public FileList GetFiles(FileSelectionList list, FileFilter filter, BackupParameters param)
        {
            FileList files = new FileList();

            foreach (var path in list)
            {
                FileInfo f = new FileInfo(path);
                if (f.Exists)
                {
                    AppendFile(ref files, filter, param, f);
                }
                else
                {
                    DirectoryInfo d = new DirectoryInfo(path);
                    if (d.Exists)
                    {
                        AppendDirectory(ref files, filter, param, d);
                    }
                }
            }
            return files;
        }

        public string MakeBackupFilename(BackupParameters param)
        {
            string filename = param.backupName + (param.type == EBackupType.Full ? ".Full" : ".Inc") + param.backupTime.ToString(".yyyy-MM-ddTHHmmss") + ".zip";
            return Path.Combine(param.outputDirectory, filename);
        }

        public void DoBackup(FileList files, BackupParameters param)
        {
            string path = MakeBackupFilename(param);

            //System.Console.WriteLine("|----:----|----:----|----:----|----:----|----:----|");
            //System.Console.WriteLine("01234567890123456789012345678901234567890123456789");

            int steps = 50;
            UInt64 count = 0;
            int lastTick = -1;
            using (FileStream zipStream = new FileStream(path, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {

                    foreach (var file in files)
                    {
                        int tick = (int)(count / (files.totalBytes / (ulong)steps));
                        if (tick != lastTick)
                        {
                            BackupUpdate?.Invoke(tick,steps);
                            //System.Console.Write(".");
                            lastTick = tick;
                        }
                        try
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            count += (UInt64)fileInfo.Length;
                            archive.CreateEntryFromFile(file, file);
                        }
                        catch (Exception ex)
                        {
                            BackupErrorMessage?.Invoke($"Failed to backup file '{file}' - {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}


/*
- UI to...
    - select folders, files, etc
    - select filters
    - backup
    - restore
- Inputs
    - output location
    - backup type - inc/full
    - last backup timestamp
    - input files/paths
    - input file filter


 */