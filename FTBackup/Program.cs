using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTBackupLib;

namespace FTBackup
{
    class Program
    {
        static FileFilter filter = new FileFilter();
        static FileSelectionList selectionList = new FileSelectionList();
        static bool error=true;

        static string helpText = @"Usage:
FTBackup --type [Full|Incremental] --fileList file --excludeList file --lastBackup datetime --outputPath directory --backupName filenamePrefix";

        static BackupParameters ProcessCommandLine(string[] args)
        {
            BackupParameters param = new BackupParameters();

            int index = 0;
            while (index < args.Length)
                {
                string arg = args[index];
                string nextArg = index + 1 < args.Length ? args[index + 1] : "";
                switch (arg)
                {
                    case "--type":
                        if (nextArg == "Full")
                            param.type = EBackupType.Full;
                        else if (nextArg == "Incremental")
                            param.type = EBackupType.Incremental;
                        else
                            throw new ApplicationException($"Unknown backup type '{nextArg}'");
                        index++;
                        break;
                    case "--fileList":
                        if (!File.Exists(nextArg))
                            throw new ApplicationException($"Can't find include file '{nextArg}'");
                        else
                        {
                            string[] files = File.ReadAllLines(nextArg);
                            foreach (var f in files)
                                selectionList.Add(f);
                        }
                        index++;
                        break;
                    case "--excludeList":
                        if (!File.Exists(nextArg))
                            throw new ApplicationException($"Can't find exclusion file '{nextArg}'");
                        else
                        {
                            string[] files = File.ReadAllLines(nextArg);
                            foreach (var f in files)
                                filter.Add(f);
                        }
                        index++;
                        break;
                    case "--lastBackup":
                        {
                            DateTime dt;
                            if (DateTime.TryParse(nextArg, out dt))
                            {
                                param.lastBackup = dt;
                            }
                            else
                            {
                                throw new ApplicationException($"Can't understand --lastBackup datetime '{nextArg}'");
                            }
                        }
                        index++;
                        break;
                    case "--outputPath":
                        param.outputDirectory = nextArg;
                        index++;
                        break;
                    case "--backupName":
                        param.backupName = nextArg;
                        index++;
                        break;
                    case "--help":
                        System.Console.WriteLine(helpText);
                        System.Environment.Exit(0);
                        break;
                }
                index++;
            }
            System.Console.WriteLine("Backup Starting...");
            System.Console.WriteLine($"Backup Name      {param.backupName}");
            System.Console.WriteLine($"Backup Time      {param.backupTime}");
            System.Console.WriteLine($"Last Backup Time {param.lastBackup}");
            System.Console.WriteLine($"Output Directory {param.outputDirectory}");
            System.Console.WriteLine($"Backup Type      {param.type}");

            return param;
        }

        static void BackupUpdateHandler(int done, int steps)
        {
            if (error)
            {
                System.Console.WriteLine("|----:----|----:----|----:----|----:----|----:----|");
                for ( int i = 0; i < done; i++ )
                    System.Console.Write(".");
            }
            else
            {
                System.Console.Write(".");
            }
            error = false;
        }

        static void BackupErrorMessageHandler(string message)
        {
            System.Console.WriteLine(message);
            error = true;
        }

        static void Main(string[] args)
        {
            System.Console.WriteLine(System.Environment.CommandLine);
            BackupParameters param = ProcessCommandLine(args);
            Backup b = new Backup();
            b.BackupErrorMessage += new Backup.BackupErrorMessageDelegate(BackupErrorMessageHandler);
            b.BackupUpdate += new Backup.BackupUpdateDelegate(BackupUpdateHandler);

            FileList files = b.GetFiles(selectionList, filter, param);
            System.Console.WriteLine($"Backing Up {files.Count:n0} files {files.totalBytes:n0} bytes"); 
            b.DoBackup(files, param);
        }
    }
}

/*

Todo...
- Volume Shadow Copy support 
- Better, faster exclusion list support
    - .gitignore like.
 */