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
            return param;
        }

        static void Main(string[] args)
        {
            BackupParameters param = ProcessCommandLine(args);
            FileList files = Backup.GetFiles(selectionList, filter, param);
            Backup.DoBackup(files, param);
        }
    }
}
