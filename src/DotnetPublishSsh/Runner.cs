using System;
using System.Threading.Tasks;
using Renci.SshNet;

namespace DotnetPublishSsh
{
    internal class Runner
    {
        public char DirectorySeparator { get; set; } = '/';

        private readonly ConnectionInfo connectionInfo;
        private readonly string cmdBefore;
        private readonly string cmdAfter;

        public Runner(PublishSshOptions publishSshOptions)
        {
            connectionInfo = Uploader.CreateConnectionInfo(publishSshOptions);
            cmdBefore = publishSshOptions.CmdBefore;
            cmdAfter = publishSshOptions.CmdAfter;
        }

        internal void RunBefore()
        {
            Console.WriteLine("Try to call before command: " + cmdBefore);
            using (Renci.SshNet.SshClient client = new SshClient(connectionInfo))
            {
                client.Connect();
                SshCommand cmd = client.RunCommand(cmdBefore);
                Console.WriteLine(cmd.Result);
            }
        }

        internal void RunAfter()
        {
            Console.WriteLine("Try to call after command: " + cmdAfter);
            using (Renci.SshNet.SshClient client = new SshClient(connectionInfo))
            {
                client.Connect();
                SshCommand cmd = client.RunCommand(cmdAfter);
                Console.WriteLine(cmd.Result);
            }
        }
    }
}