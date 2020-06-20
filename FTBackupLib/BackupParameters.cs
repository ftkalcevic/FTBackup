using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTBackupLib
{
    public enum EBackupType
    {
        Incremental,
        Full
    };

    public class BackupParameters
    {
        public EBackupType type;
        public DateTime lastBackup;
        public String outputDirectory;
        public String backupName;
        public DateTime backupTime;

        public BackupParameters()
        {
            backupTime = DateTime.Now;
        }
    }
}
