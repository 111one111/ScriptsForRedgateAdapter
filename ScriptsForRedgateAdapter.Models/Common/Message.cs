using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Models.Common
{
    public class Message
    {
        /// <summary>
        /// Returns true if operation was successful
        /// </summary>
        public bool IsSuccessfull { get; set; }

        /// <summary>
        /// Message to show User.
        /// </summary>
        public string Text { get; set; }
    }
}
