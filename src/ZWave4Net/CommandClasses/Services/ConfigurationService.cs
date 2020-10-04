using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave.Channel;

namespace ZWave.CommandClasses.Services
{
    internal class ConfigurationService : CommandClassService, IConfiguration
    {
        enum ConfigurationCommand : byte
        {
            Set = 0x04,
            Get = 0x05,
            Report = 0x06
        }

        public ConfigurationService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, controller, CommandClass.Configuration)
        {
        }

        public Task<ConfigurationReport> Get(byte parameter, CancellationToken cancellationToken = default)
        {
            var command = new Command(CommandClass, ConfigurationCommand.Get, parameter);
            return Send<ConfigurationReport>(command, ConfigurationCommand.Report, cancellationToken);
        }

        public Task Set(byte parameter, object value, byte size, CancellationToken cancellationToken = default)
        {
            if (size < 1 || size > 4)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be between 1 and 4");

            using (var writer = new PayloadWriter())
            {
                writer.WriteByte(parameter);
                writer.WriteByte(size);
                switch(size)
                {
                    case 1:
                        writer.WriteByte(Convert.ToByte(value));
                        break;
                    case 2:
                        writer.WriteInt16(Convert.ToInt16(value));
                        break;
                    case 3:
                        writer.WriteInt24(Convert.ToInt16(value));
                        break;
                    case 4:
                        writer.WriteInt32(Convert.ToInt32(value));
                        break;
                }
                var command = new Command(CommandClass, ConfigurationCommand.Set, writer.ToPayload());
                return Send(command, cancellationToken);
            }
        }
    }
}
