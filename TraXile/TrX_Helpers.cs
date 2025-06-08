using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TraXile
{
    public static class TrX_Helpers
    {
        /// <summary>
        /// Convert a string, making the first letter uppercase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CapitalFirstLetter(string input)
        {
            input = input.ToLower();
            return string.Format("{0}{1}", input[0].ToString().ToUpper(), input.Substring(1, input.Length - 1));
        }

        public static IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        /// <summary>
        /// Resize the last column of a ListView to fit the remaining space.
        /// </summary>
        /// <param name="listView"></param>
        public static void AutoResizeLastColumn(ListView listView)
        {
            if (listView.View != View.Details || listView.Columns.Count == 0)
                return;

            int totalWidth = listView.ClientSize.Width;
            int otherColumnsWidth = 0;

            // Sum widths of all but the last column
            for (int i = 0; i < listView.Columns.Count - 1; i++)
            {
                otherColumnsWidth += listView.Columns[i].Width;
            }

            int newWidth = totalWidth - otherColumnsWidth - 2; // Adjust for borders/scrollbars
            if (newWidth > 0)
            {
                listView.Columns[listView.Columns.Count - 1].Width = newWidth;
            }
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        /// <summary>
        /// Get line count from Client.txt. Used for progress calculation
        /// </summary>
        /// <returns></returns>
        public static int GetLogFileLineCount(string path)
        {
            int iCount = 0;
            FileStream fs1 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            TextReader reader1 = new StreamReader(fs1);
            while ((reader1.ReadLine()) != null)
            {
                iCount++;
            }
            reader1.Close();
            return iCount;
        }


        /// <summary>
        /// Get the last line of the given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetLastLineOfFile(string path)
        {
            return File.ReadLines(path).Last();
        }

        public static string GetRegistryValue(string key, string value, string defaultVal = "")
        {
            return (string)Registry.GetValue(key, value, defaultVal);
        }
    }
}
