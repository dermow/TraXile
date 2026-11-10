using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace TraXile.Next.Services
{
    /// <summary>
    /// Downloads the TraXile metadata (leagues) like the classic Main.DownloadMetaData:
    /// <c>{METADATA_BASE_URL}/{metadata_meta_branch}/metadata.xml</c> into <c>%APPDATA%\TraXile\metadata\metadata.xml</c>.
    /// The file is only replaced by a download that parses and contains leagues.
    /// </summary>
    public static class MetadataDownloader
    {
        public const string BranchKey = "metadata_meta_branch";

        public static string Url(string branch) =>
            $"{TrX_Static.METADATA_BASE_URL}/{(string.IsNullOrWhiteSpace(branch) ? TrX_Static.METADATA_DEFAULT_BRANCH : branch)}/{TrX_Static.METADATA_XML_FILE}";

        /// <summary>Downloads and stores the metadata; false (and the old file kept) on any error.</summary>
        public static async Task<bool> UpdateAsync(string url, string targetPath, Func<string, Task<string>> download = null)
        {
            try
            {
                download = download ?? Http.GetStringAsync;
                string data = await download(url).ConfigureAwait(false);
                return Store(data, targetPath);
            }
            catch (Exception)
            {
                return false; // offline etc.: keep the existing file
            }
        }

        /// <summary>Validates the downloaded XML and writes it (atomically) to <paramref name="targetPath"/>.</summary>
        public static bool Store(string data, string targetPath)
        {
            var xml = new XmlDocument();
            xml.LoadXml(data);
            if (LeagueCatalog.Parse(xml).Count == 0) return false;

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            string tmp = targetPath + ".download";
            xml.Save(tmp);
            if (File.Exists(targetPath)) File.Delete(targetPath);
            File.Move(tmp, targetPath);
            return true;
        }
    }
}
