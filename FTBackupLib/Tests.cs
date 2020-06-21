using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;

namespace FTBackupLib
{
    [TestFixture]
    public class Tests
    {
        struct FileData
        {
            public String filename;
            public DateTime lastChange;
        };

        static readonly String tempPath = Environment.GetEnvironmentVariable("TEMP");//@"c:\temp\ftbackup_tests";
        static readonly String testPath = tempPath + @"\ftbackup_tests";
        static readonly String outputPath = tempPath + @"\ftbackup_tests_output";
        static readonly FileData[] testFiles = new FileData[] {
            new FileData { filename=testPath+@"\file1.a", lastChange = new DateTime(2020, 1, 1, 12, 0, 0 ) },
            new FileData { filename=testPath+@"\file2.b", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=testPath+@"\file3.c", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=testPath+@"\dir1\file4.a", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=testPath+@"\dir1\file5.b", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=testPath+@"\dir2\file6.a", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=testPath+@"\dir2\file7.b", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            new FileData { filename=testPath+@"\dir1\dir3\dir4\file8.b", lastChange = new DateTime(2020, 1, 1, 12, 10, 0 ) },
            new FileData { filename=testPath+@"\dir1\dir3\dir4\file9.c", lastChange = new DateTime(2020, 1, 1, 12, 20, 0 ) },
            };

        private void CompareFileLists(FileList files, List<string> expected)
        {
            Assert.AreEqual(expected.Count, files.Count, "Number of files found does not match");

            HashSet<string> fileSet = new HashSet<string>();
            foreach (var f in files)
                fileSet.Add(f);

            foreach (var file in expected)
            {
                Assert.IsTrue(fileSet.Contains(file), $"file '{file}' is not in the file list");
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
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
        }

        [Test]
        public void GetAllFiles()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath);
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            FileList files = new Backup().GetFiles(list, filter, param);

            List<String> expected = new List<string>();
            foreach (var f in testFiles)
                expected.Add(f.filename);

            CompareFileLists(files, expected);
        }

        [Test]
        public void GetAllFiles2()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath+@"\dir2");
            list.Add(testPath+ @"\dir1\dir3");
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            FileList files = new Backup().GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                            testPath+@"\dir2\file6.a",
                            testPath+@"\dir2\file7.b",
                            testPath+@"\dir1\dir3\dir4\file8.b",
                            testPath+@"\dir1\dir3\dir4\file9.c",
            };

            CompareFileLists(files, expected);
        }


        [Test]
        public void WildcardFilter()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath);
            FileFilter filter = new FileFilter();
            filter.Add(@".*\.a");
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            FileList files = new Backup().GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                            //tempPath+@"\file1.a",
                            testPath+@"\file2.b",
                            testPath+@"\file3.c",
                            //tempPath+@"\dir1\file4.a",
                            testPath+@"\dir1\file5.b",
                            //tempPath+@"\dir2\file6.a",
                            testPath+@"\dir2\file7.b",
                            testPath+@"\dir1\dir3\dir4\file8.b",
                            testPath+@"\dir1\dir3\dir4\file9.c",
            };

            CompareFileLists(files, expected);
        }

        [Test]
        public void Wildcard2Filter()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath);
            FileFilter filter = new FileFilter();
            filter.Add(@".*\.b");
            filter.Add(@".*\.c");
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            FileList files = new Backup().GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                    testPath+@"\file1.a",
                    testPath+@"\dir1\file4.a",
                    testPath+@"\dir2\file6.a",
            };

            CompareFileLists(files, expected);
        }

        [Test]
        public void Incremental()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath);
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Incremental, lastBackup = new DateTime(2020, 1, 1, 12, 15, 0) };

            FileList files = new Backup().GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                    testPath + @"\file3.c",
                    testPath + @"\dir1\file4.a",
                    testPath + @"\dir2\file7.b",
                    testPath + @"\dir1\dir3\dir4\file9.c",
            };

            CompareFileLists(files, expected);
        }

        [Test]
        public void IncrementalFilter()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath);
            FileFilter filter = new FileFilter();
            filter.Add(@".*\.b");
            filter.Add(@".*\.c");
            BackupParameters param = new BackupParameters() { type = EBackupType.Incremental, lastBackup = new DateTime(2020, 1, 1, 12, 15, 0) };

            FileList files = new Backup().GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                    testPath+@"\dir1\file4.a",
            };

            CompareFileLists(files, expected);
        }

        [Test]
        public void TestMakeFilename()
        {
            BackupParameters param = new BackupParameters() { type = EBackupType.Full, outputDirectory = "d:\\outdir", backupName = "TestMakeFilename", backupTime = new DateTime(2020, 1, 1, 13, 0, 0) };
            string path = new Backup().MakeBackupFilename(param);
            Assert.AreEqual("d:\\outdir\\TestMakeFilename.Full.2020-01-01T130000.zip", path);

            param = new BackupParameters() { type = EBackupType.Full, outputDirectory = "d:\\outdir", backupName = "TestMakeFilename", backupTime = new DateTime(2020, 1, 1, 1, 2, 3) };
            path = new Backup().MakeBackupFilename(param);
            Assert.AreEqual("d:\\outdir\\TestMakeFilename.Full.2020-01-01T010203.zip", path);

            param = new BackupParameters() { type = EBackupType.Incremental, outputDirectory = "d:\\outdir", backupName = "TestMakeFilename", backupTime = new DateTime(2020, 1, 1, 1, 2, 3) };
            path = new Backup().MakeBackupFilename(param);
            Assert.AreEqual("d:\\outdir\\TestMakeFilename.Inc.2020-01-01T010203.zip", path);
        }

        [Test]
        public void GetAllFilesZip()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath);
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Full, outputDirectory = outputPath, backupName = "GetAllFilesZip", backupTime = new DateTime(2020,1,1,13,0,0) };

            FileList files = new Backup().GetFiles(list, filter, param);
            new Backup().DoBackup(files, param);
        }

        [Test]
        public void DuplicateFiles()
        {
            FileSelectionList list = new FileSelectionList();
            list.Add(testPath+@"\file3.c");
            list.Add(testPath + @"\file3.c");
            FileFilter filter = new FileFilter();
            BackupParameters param = new BackupParameters() { type = EBackupType.Full };

            FileList files = new Backup().GetFiles(list, filter, param);

            List<String> expected = new List<string>() {
                    testPath+@"\file3.c",
            };

            CompareFileLists(files, expected);
        }
    }
}
