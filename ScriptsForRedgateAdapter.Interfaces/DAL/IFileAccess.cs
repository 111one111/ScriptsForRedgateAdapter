using ScriptsForRedgateAdapter.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Interfaces.DAL
{
    public interface IFileAccess<T>
    {
        /// <summary>
        /// Gets a list of files for processing.
        /// </summary>
        /// <returns></returns>
        List<string> GetFileNames(string directoryDetails, FileExtensions fileExtension);

        /// <summary>
        /// returns file contents and puts them into the generically set object.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        T LoadJsonFile(string fileName);

        /// <summary>
        /// returns file contents as string.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string GetFileContents(string fileName);

        /// <summary>
        /// Writes File to Disk
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        void WriteToFile(string fileName, string content);

        /// <summary>
        /// Adds new line to existing file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="content"></param>
        void AddLineToFile(string filename, string content);
    }
}
