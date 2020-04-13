using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Enums;

namespace ScriptsForRedgateAdapter.DAL
{
    public class FileAccess<T> : IFileAccess<T>
    {
        readonly AppConfig _settings;
        public FileAccess() { }
        public FileAccess(IOptions<AppConfig> settings)
        {
            _settings = settings.Value;
        }

        /// <summary>
        /// Gets a list of SQL files for processing.
        /// </summary>
        /// <returns></returns>
        public List<string> GetFileNames(string directoryDetails, FileExtensions fileExtension)
        {
            string[] fileList = Directory.GetFiles(directoryDetails, $"*.{fileExtension.ToString()}");
            return fileList.ToList();
        }

        /// <summary>
        /// returns file contents as string.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFileContents(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return string.Empty;
            }

            string fileContents = "";
            using (StreamReader reader = new StreamReader(fileName))
            {
                fileContents = reader.ReadToEnd();
            }
            return fileContents;
        }

        /// <summary>
        /// returns file contents and puts them into the generically set object.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T LoadJsonFile(string fileName)
        {
            string contents = GetFileContents(fileName);
            if (contents.Length == 0)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(contents);
        }

        /// <summary>
        /// Writes File to Disk
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public void WriteToFile(string fileName, string content)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(content);
            }
        }

        /// <summary>
        /// Adds new line to existing file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="content"></param>
        public void AddLineToFile(string filename, string content)
        {
            File.AppendAllText(filename, content + Environment.NewLine);
        }
    }
}
