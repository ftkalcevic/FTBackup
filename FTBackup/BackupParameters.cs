using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTBackup
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
    }
}
