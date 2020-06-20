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

        static readonly String tempPath = @"c:\temp\ftbackup_tests";
        static readonly FileData[] testFiles = new FileData[] {
            new FileData { filename=tempPath+@"\file1.a", lastChange = new DateTime(2020, 1, 1, 12, 0, 0 ) },
            new FileData { filename=tempPath+@"\file2.b", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=tempPath+@"\file3.c", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"\dir1\file4.a", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"\dir1\file5.b", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=tempPath+@"\dir2\file6.a", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=tempPath+@"\dir2\file7.b", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=tempPath+@"\dir1\dir3\dir4\file8.b", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=tempPath+@"\dir1\dir3\dir4\file9.c", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
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
            foreach (var f in testFiles)
            {
                String path = f.filename;
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
            list.Add(tempPath);
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            List<String> files = Backup.GetFiles(list, filter, param);

            List<String> expected = new List<string>();
            foreach (var f in testFiles)
                expected.Add(f.filename);

            CompareFileLists(files, expected);
        }

        [Test]
        public void GetAllFiles2()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(tempPath+@"\dir2");
            list.Add(tempPath+ @"\dir1\dir3");
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            List<String> files = Backup.GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                            tempPath+@"\dir2\file6.a",
                            tempPath+@"\dir2\file7.b",
                            tempPath+@"\dir1\dir3\dir4\file8.b",
                            tempPath+@"\dir1\dir3\dir4\file9.c",
            };

            CompareFileLists(files, expected);
        }


        [Test]
        public void WildcardFilter()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(tempPath);
            FileFilter filter = new FileFilter();
            filter.Add(@".*\.a");
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            List<String> files = Backup.GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                            //tempPath+@"\file1.a",
                            tempPath+@"\file2.b",
                            tempPath+@"\file3.c",
                            //tempPath+@"\dir1\file4.a",
                            tempPath+@"\dir1\file5.b",
                            //tempPath+@"\dir2\file6.a",
                            tempPath+@"\dir2\file7.b",
                            tempPath+@"\dir1\dir3\dir4\file8.b",
                            tempPath+@"\dir1\dir3\dir4\file9.c",
            };

            CompareFileLists(files, expected);
        }

        [Test]
        public void Wildcard2Filter()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(tempPath);
            FileFilter filter = new FileFilter();
            filter.Add(@".*\.b");
            filter.Add(@".*\.c");
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            List<String> files = Backup.GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                    tempPath+@"\file1.a",
                    tempPath+@"\dir1\file4.a",
                    tempPath+@"\dir2\file6.a",
            };

            CompareFileLists(files, expected);
        }

        [Test]
        public void Incremental()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(tempPath);
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Incremental, lastBackup = new DateTime(2020, 1, 1, 12, 15, 0) };

            List<String> files = Backup.GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                    tempPath + @"\file3.c",
                    tempPath + @"\dir1\file4.a",
                    tempPath + @"\dir2\file7.b",
                    tempPath + @"\dir1\dir3\dir4\file9.c",
            };

            CompareFileLists(files, expected);
        }

        [Test]
        public void IncrementalFilter()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(tempPath);
            FileFilter filter = new FileFilter();
            filter.Add(@".*\.b");
            filter.Add(@".*\.c");
            BackupParameters param = new BackupParameters() { type = EBackupType.Incremental, lastBackup = new DateTime(2020, 1, 1, 12, 15, 0) };

            List<String> files = Backup.GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                    tempPath+@"\dir1\file4.a",
            };

            CompareFileLists(files, expected);
        }
    }
}
