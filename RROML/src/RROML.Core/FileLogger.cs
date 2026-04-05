using System;
using System.IO;
using System.Text;
using RROML.Abstractions;

namespace RROML.Core
{
    public sealed class FileLogger : IModLogger
    {
        private readonly object _gate;
        private readonly string _logPath;

        public FileLogger(string logPath)
        {
            _gate = new object();
            _logPath = logPath;
        }

        public void Info(string message)
        {
            Write("INFO", message, null);
        }

        public void Warn(string message)
        {
            Write("WARN", message, null);
        }

        public void Error(string message)
        {
            Write("ERROR", message, null);
        }

        public void Error(string message, Exception exception)
        {
            Write("ERROR", message, exception);
        }

        private void Write(string level, string message, Exception exception)
        {
            var builder = new StringBuilder();
            builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            builder.Append(" [");
            builder.Append(level);
            builder.Append("] ");
            builder.Append(message);

            if (exception != null)
            {
                builder.AppendLine();
                builder.Append(exception);
            }

            lock (_gate)
            {
                File.AppendAllText(_logPath, builder.ToString() + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
