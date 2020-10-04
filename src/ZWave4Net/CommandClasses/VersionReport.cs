using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    public class VersionReport : Report
    {
        public LibraryType LibraryType { get; private set; }
        public string ProtocolVersion { get; private set; }
        public string ApplicationVersion { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            var libraryType = reader.ReadByte();
            LibraryType = Enum.IsDefined(typeof(LibraryType), libraryType) ? (LibraryType)libraryType : LibraryType.NotApplicable;

            ProtocolVersion = reader.ReadByte().ToString("d") + "." + reader.ReadByte().ToString("d2");
            ApplicationVersion = reader.ReadByte().ToString("d") + "." + reader.ReadByte().ToString("d2");
        }

        public override string ToString()
        {
            return $"Library: {LibraryType}, Protocol: {ProtocolVersion}, Application: {ApplicationVersion}";
        }
    }
}
