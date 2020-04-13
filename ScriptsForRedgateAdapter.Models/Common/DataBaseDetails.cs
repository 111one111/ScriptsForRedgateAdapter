using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Models.Common
{
    public class DataBaseDetails
    {
        /// <summary>
        /// The Name of the Database
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Corresponding folder of the Database
        /// </summary>
        public string FolderLocation { get; set; }
    }
}
