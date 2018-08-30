using System;
using Renci.SshNet;

namespace DotnetPublishSsh
{
    internal class Runner
    {
        public char DirectorySeparator { get; set; } = '/';

        private readonly ConnectionInfo connectionInfo;
        private readonly string scriptFile;
        private readonly string directory;

        public Runner(PublishSshOptions publishSshOptions)
        {
            connectionInfo = Uploader.CreateConnectionInfo(publishSshOptions);
            scriptFile = publishSshOptions.ScriptFile;
            directory = publishSshOptions.Path;
        }

        internal void Run()
        {
            using (Renci.SshNet.SshClient client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.RunCommand("cd \"" + directory.Replace("\"", "\\\"") + "\"");
                client.RunCommand(scriptFile);
            }
        }
    }
}