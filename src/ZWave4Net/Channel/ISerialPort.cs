using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZWave.Channel
{
    public interface ISerialPort : IDuplexStream
    {
        bool IsOpen { get; }
        Task Open();
        Task Close();
    }
}
