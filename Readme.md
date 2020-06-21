# FTBackup
A quick and dirty NTBackup replacement.

I used NTBackup up until Dec 2019, when the latest Windows 10 updates made it completely stop working.
I tried some commercial backup programs, but they either run continuously in the background or have other limitations.
I just wanted an NTBackup replacement - a scheduled backup job that would do either a full backup or incremental backup.

This project takes a list of directories, and a list of exclusions and zips them up into a zip archive.  
The exlusions are in .Net Regex format.

Volume Shadow Copy hasn't been implemented, so "in-use" files aren't backed up.

I'm using this for a 3-times-a-day incremental backup.  I still use the commerical package for a weekly full backup because it uses VSS.
