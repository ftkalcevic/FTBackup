using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;

namespace FTBackup
{
    [TestFixture]
    public class Tests
    {
        struct FileData
        {
            public String filename;
            public DateTime lastChange;
        };

        static readonly String tempPath = @"d:\temp\ftbackup_tests\";
        static readonly FileData[] files = new FileData[] {
            new FileData { filename=tempPath+@"file1", lastChange = new DateTime(2020, 1, 1, 12, 0, 0 ) },
            new FileData { filename=tempPath+@"file2", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=tempPath+@"file3", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"dir1\file4", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"dir1\file5", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"dir2\file6", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"dir2\file7", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"dir1\dir3\dir4\file8", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"dir1\dir3\dir4\file9", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            };

        private void CompareFileLists(List<string> files, List<string> expected)
        {
            Assert.AreEqual(expected.Count, files.Count, "Number of files found does not match");

            HashSet<string> fileSet = new HashSet<string>(files);

            foreach (var file in expected)
            {
                Assert.IsTrue(fileSet.Contains(file), $"file '{file}' is no in the file list");
                fileSet.Remove(file);
            }
        }

        [SetUp]
        public void Setup()
        {
            // Create files and set last update dates.
            foreach (var f in files)
            {
                String path = Path.Combine(tempPath, f.filename);
                FileInfo fi = new FileInfo(path);
                if (!fi.Exists)
                {
                    if ( !Directory.Exists(fi.Directory.FullName) )
                        Directory.CreateDirectory(fi.Directory.FullName);

                    using (StreamWriter writer = File.CreateText(path))
                    {
                        writer.WriteLine("This is file " + path);
                        writer.Close();
                    }
                }
                fi.LastWriteTime = f.lastChange;
            }
        }

        [Test]
        public void GetAllFiles()
        {
            FileSelectionList list = new FileSelectionList();
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            List<String> files = Backup.GetFiles(list, filter, param);

            List<String> expected = new List<string>() {tempPath+@"file1",
                                                        tempPath+@"file2",
                                                        tempPath+@"file3",
                                                        tempPath+@"dir1\file4",
                                                        tempPath+@"dir1\file5",
                                                        tempPath+@"dir2\file6",
                                                        tempPath+@"dir2\file7",
                                                        tempPath+@"dir1\dir3\dir4\file8",
                                                        tempPath+@"dir1\dir3\dir4\file9" };

            CompareFileLists(files, expected);
        }
    }
}
