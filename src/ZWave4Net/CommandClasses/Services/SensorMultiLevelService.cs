using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave.Channel;

namespace ZWave.CommandClasses.Services
{
    internal class SensorMultiLevelService : CommandClassService, ISensorMultiLevel
    {
        enum SensorMultiLevelCommand
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Get = 0x04,
            Report = 0x05
        }

        public SensorMultiLevelService(byte nodeID, byte endpointID, ZWaveController controller) 
            : base(nodeID, endpointID, controller, CommandClass.SensorMultiLevel)
        {
        }

        public Task<SensorMultiLevelReport> Get(CancellationToken cancellationToken = default)
        {
            var command = new Command(CommandClass, SensorMultiLevelCommand.Get);
            return Send<SensorMultiLevelReport>(command, SensorMultiLevelCommand.Report, cancellationToken);
        }

        public Task<SensorMultiLevelReport> Get(SensorType sensor, byte scale = 0, CancellationToken cancellationToken = default)
        {
            var command = new Command(CommandClass, SensorMultiLevelCommand.Get, (byte)sensor, (byte)(scale & 0x18));
            return Send<SensorMultiLevelReport>(command, SensorMultiLevelCommand.Report, cancellationToken);
        }

        public IObservable<SensorMultiLevelReport> Reports
        {
            get { return Reports<SensorMultiLevelReport>(SensorMultiLevelCommand.Report); }
        }
    }
}
