using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel
{
    internal static class Encapsulation
    {
        public static IEnumerable<Command> Flatten(Command command)
        {
            yield return command;
            while (command is IEncapsulatedCommand encapsulatedCommand)
            {
                command = encapsulatedCommand.Decapsulate();
                yield return command;
            }
        }
    }
}
