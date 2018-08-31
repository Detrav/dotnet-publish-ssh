using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetPublishSsh
{
    public class FileHash
    {
        public string Hash { get; set; }
        public string Path { get; set; }
        public long Lenght { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
