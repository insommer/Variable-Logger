using System;
using System.Collections.Generic;
using System.Text;
using DataStructures;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.IO;

namespace Virgil
{
    class ExampleServer : ServerCommunicator
    {

        /// <summary>
        /// To be called to add messages to the server message log. Eventually may support logging to a file
        /// as well as to the screen.
        /// </summary>
        public EventHandler<MessageEvent> messageLog;

        private MainExampleServerlForm exampleServerForm;

        private string basePathFile = "settings.txt";

        public ExampleServerSettings serverSettings;

        public ExampleServer(MainExampleServerlForm form, ExampleServerSettings serverSettings)
        {
            this.exampleServerForm = form;
            this.serverSettings = serverSettings;

            LoadBasePath(basePathFile);
            SetDateFolder();
            this.serverSettings.ServerName = "VariableLogger";
            this.serverSettings.ServerPort = 5679;
        }
        private void SetDateFolder()
        {
            this.serverSettings.LogFileSubfolder = DateTime.Now.ToString("'Data/'yyyy'/'MM'-'yyyy'/'dd' 'MMM' 'yyyy'/''Variable Logs'");
        }
        private void LoadBasePath(string fileName)
        { //get the root folder in which to store the Data folder
            try
            {
                if (!File.Exists(fileName))//Create for next time:
                    using (StreamWriter sw = new StreamWriter(fileName))//Default to My Documents:
                        sw.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
                //Now it must exist, so read it:
                using (StreamReader sr = new StreamReader(fileName))
                {
                    this.serverSettings.LogFileBasePath = sr.ReadLine();
                }
                 
            }
            catch (Exception e) { Console.WriteLine("{0}", e.ToString()); }

        }
        private void SaveBasePath(string fileName)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                    sw.WriteLine(this.serverSettings.LogFileBasePath);
            }
            catch (Exception e) { Console.WriteLine("{0}", e.ToString()); }
        }

        #region Implementation of ServerCommunicator
        public override bool armTasks(UInt32 clockID)
        {
            return true;
        }

        public override BufferGenerationStatus generateBuffers(int listIterationNumber)
        {
            return BufferGenerationStatus.Success;
        }

        public override bool generateTrigger()
        {
            return true;
        }

        public override List<HardwareChannel> getHardwareChannels()
        {
            return new List<HardwareChannel>();
        }

        public override string getServerName()
        {
            return serverSettings.ServerName;
        }

        public override ServerSettingsInterface getServerSettings()
        {
            return this.serverSettings;
        }

        public override void nextRunTimeStamp(DateTime timeStamp)
        {
            messageLog(this, new MessageEvent("Received time stamp."));
        }

        public override bool outputGPIBGroup(GPIBGroup gpibGroup, SettingsData settings)
        {
            return true;
        }

        public override bool outputRS232Group(RS232Group rs232Group, SettingsData settings)
        {
            return true;
        }

        public override bool outputSingleTimestep(SettingsData settings, SingleOutputFrame output)
        {
            return true;
        }

        public override bool ping()
        {
            return true;
        }

        public override bool runSuccess()
        {
            return true;
        }

        public override bool setSequence(SequenceData sequence)
        {
            messageLog(this, new MessageEvent("Received sequence data."));
            List<Variable> vars = sequence.Variables;
            string dateTime = DateTime.Now.ToString("yyyy'_'MM'_'dd'_'HH'_'mm'_'ss");
            SetDateFolder();
            string dir = Path.GetFullPath(Path.Combine(serverSettings.LogFileBasePath, serverSettings.LogFileSubfolder));
            Directory.CreateDirectory(dir);
            SaveBasePath(basePathFile);

            string tentativeLogNameWithDir = Path.Combine(dir, "Variables_" + dateTime);
            string logFileExtension = ".txt";
            string logFullPath = tentativeLogNameWithDir + "_0" + logFileExtension;

            for (int i = 1; i <= 10 & File.Exists(logFullPath); i++)
            {
                logFullPath = tentativeLogNameWithDir + "_" + i.ToString() + logFileExtension;
            }

            using (StreamWriter writer = new StreamWriter(logFullPath))
            {
                writer.WriteLine("[Variables]");
                foreach (Variable var in vars)
                {
                    writer.WriteLine(var.VariableName + " = " + var.VariableValue.ToString());
                }
            }
            return true;
        }

        public override bool setSettings(SettingsData settings)
        {
            messageLog(this, new MessageEvent("Received settings data."));
            return true;
        }

        public override void stop()
        {
            
        }


        #endregion


        #region Methods called by MainVirgilForm

        public void openConnection()
        {
            messageLog(this, new MessageEvent("Attempting to open connection."));
            Thread thread = new Thread(new ThreadStart(startMarshalProc));
            thread.Start();
        }

        #endregion



        #region Thread procedures

        private object marshalLock = new object();
        private TcpChannel tcpChannel;
        private ObjRef objRef;

        /// <summary>
        /// Adapted from corresponding procedure in AtticusServerRuntime
        /// </summary>
        private void startMarshalProc()
        {
            try
            {
                lock (marshalLock)
                {
                    tcpChannel = new TcpChannel(serverSettings.ServerPort);
                    ChannelServices.RegisterChannel(tcpChannel, false);
                    objRef = RemotingServices.Marshal(this, "serverCommunicator");
                }
                messageLog(this, new MessageEvent("Connection suceeded."));
            }
            catch (Exception e)
            {
                messageLog(this, new MessageEvent("Unable to start Marshal due to exception: " + e.Message + e.StackTrace));
                exampleServerForm.reenableConnectButton();
            }
        }



        #endregion
    }
}
