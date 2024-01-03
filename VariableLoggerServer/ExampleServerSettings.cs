using System;
using System.Collections.Generic;
using System.Text;
using DataStructures;

namespace Virgil
{
    public class ExampleServerSettings : ServerSettingsInterface
    {
        private string serverName;

        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }

        private int serverPort;
        public int ServerPort
        {
            get { return serverPort; }
            set { serverPort = value; }
        }

        private string logFileBasePath;
        public string LogFileBasePath
        {
            get { return logFileBasePath; }
            set { logFileBasePath = value; }
        }

        private string logFileSubfolder;
        public string LogFileSubfolder
        {
            get { return logFileSubfolder; }
            set { logFileSubfolder = value; }
        }


    }
}
