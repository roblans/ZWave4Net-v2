using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave.Channel;

namespace ZWave.CommandClasses.Services
{
    internal class WakeUpService : CommandClassService, IWakeUp
    {
        enum WakeUpCommand
        {
            IntervalSet = 0x04,
            IntervalGet = 0x05,
            IntervalReport = 0x06,
            Notification = 0x07,
            NoMoreInformation = 0x08
        }

        public WakeUpService(byte nodeID, byte endpointID, ZWaveController controller) : 
            base(nodeID, endpointID, controller, CommandClass.WakeUp)
        {
        }

        public Task<WakeUpIntervalReport> GetInterval(CancellationToken cancellationToken = default)
        {
            var command = new Command(CommandClass, WakeUpCommand.IntervalGet);
            return Send<WakeUpIntervalReport>(command, WakeUpCommand.IntervalReport, cancellationToken);
        }

        public Task SetInterval(TimeSpan interval, byte targetNodeID, CancellationToken cancellationToken = default)
        {
            using (var writer = new PayloadWriter())
            {
                writer.WriteInt24((int)interval.TotalSeconds);
                writer.WriteByte(targetNodeID);

                var command = new Command(CommandClass, WakeUpCommand.IntervalSet, writer.ToPayload());
                return Send(command, cancellationToken);
            }
        }

        public Task NoMoreInformation(CancellationToken cancellationToken)
        {
            var command = new Command(CommandClass, WakeUpCommand.NoMoreInformation);
            return Send(command, cancellationToken);
        }

        public IObservable<WakeUpNotificationReport> Reports
        {
            get { return Reports<WakeUpNotificationReport>(WakeUpCommand.Notification); }
        }
    }
}
