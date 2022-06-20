using System.CommandLine;
using System.CommandLine.IO;
using Xunit.Abstractions;

namespace AnnoDesigner.Tests
{
    internal class MockedConsole : IConsole
    {
        public MockedConsole(ITestOutputHelper output)
        {
            Out = new TestOutputStreamWriter(output);
            Error = new TestOutputStreamWriter(output);
        }

        /// <inheritdoc />
        public IStandardStreamWriter Error { get; protected set; }

        /// <inheritdoc />
        public IStandardStreamWriter Out { get; protected set; }

        /// <inheritdoc />
        public bool IsOutputRedirected { get; protected set; }

        /// <inheritdoc />
        public bool IsErrorRedirected { get; protected set; }

        /// <inheritdoc />
        public bool IsInputRedirected { get; protected set; }

        private class TestOutputStreamWriter : IStandardStreamWriter
        {
            private readonly ITestOutputHelper _output;

            public TestOutputStreamWriter(ITestOutputHelper output)
            {
                _output = output;
            }

            public void Write(string value)
            {
                _output.WriteLine(value);
            }
        }
    }
}
