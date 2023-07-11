#region Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: SCLA 1.0
//   - GPL V3: everybody else
//
// SCLA license terms accompanied with this source code.
// See SCLA 1.0: https://technosoftware.com/license/Source_Code_License_Agreement.pdf
//
// GNU General Public License as published by the Free Software Foundation;
// version 3 of the License are accompanied with this source code.
// See https://technosoftware.com/license/GPLv3License.txt
//
// This source code is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.IO;
// ReSharper disable UnusedMember.Global

#endregion

namespace Technosoftware.DaAeHdaClient.Utilities
{
    /// <summary>
    /// Defines various static utility functions.
    /// </summary>
    public static class Utils
    {
        #region Trace Support
#if DEBUG
        private static int traceOutput_ = (int)TraceOutput.DebugAndFile;
        private static int traceMasks_ = TraceMasks.All;
#else
        private static int traceOutput_ = (int)TraceOutput.FileOnly;
        private static int traceMasks_ = (int)TraceMasks.None;
#endif

        private static string traceFileName_;
        private static long baseLineTicks_ = DateTime.UtcNow.Ticks;
        private static object traceFileLock_ = new object();

        /// <summary>
        /// The possible trace output mechanisms.
        /// </summary>
        public enum TraceOutput
        {
            /// <summary>
            /// No tracing
            /// </summary>
            Off = 0,

            /// <summary>
            /// Only write to file (if specified). Default for Release mode.
            /// </summary>
            FileOnly = 1,

            /// <summary>
            /// Write to debug trace listeners and a file (if specified). Default for Debug mode.
            /// </summary>
            DebugAndFile = 2,

            /// <summary>
            /// Write to trace listeners and a file (if specified).
            /// </summary>
            StdOutAndFile = 3
        }

        /// <summary>
        /// The masks used to filter trace messages.
        /// </summary>
        public static class TraceMasks
        {
            /// <summary>
            /// Do not output any messages.
            /// </summary>
            public const int None = 0x0;

            /// <summary>
            /// Output error messages.
            /// </summary>
            public const int Error = 0x1;

            /// <summary>
            /// Output informational messages.
            /// </summary>
            public const int Information = 0x2;

            /// <summary>
            /// Output stack traces.
            /// </summary>
            public const int StackTrace = 0x4;

            /// <summary>
            /// Output basic messages for service calls.
            /// </summary>
            public const int Service = 0x8;

            /// <summary>
            /// Output detailed messages for service calls.
            /// </summary>
            public const int ServiceDetail = 0x10;

            /// <summary>
            /// Output basic messages for each operation.
            /// </summary>
            public const int Operation = 0x20;

            /// <summary>
            /// Output detailed messages for each operation.
            /// </summary>
            public const int OperationDetail = 0x40;

            /// <summary>
            /// Output messages related to application initialization or shutdown
            /// </summary>
            public const int StartStop = 0x80;

            /// <summary>
            /// Output messages related to a call to an external system.
            /// </summary>
            public const int ExternalSystem = 0x100;

            /// <summary>
            /// Output messages related to security
            /// </summary>
            public const int Security = 0x200;

            /// <summary>
            /// Output all messages.
            /// </summary>
            public const int All = 0x7FFFFFFF;
        }

        /// <summary>
        /// Sets the output for tracing (thead safe).
        /// </summary>
        public static void SetTraceOutput(TraceOutput output)
        {
            lock (traceFileLock_)
            {
                traceOutput_ = (int)output;
            }
        }

        /// <summary>
        /// Gets the current trace mask settings.
        /// </summary>
        public static int TraceMask => traceMasks_;

        /// <summary>
        /// Sets the mask for tracing (thead safe).
        /// </summary>
        public static void SetTraceMask(int masks)
        {
            traceMasks_ = masks;
        }

        /// <summary>
        /// Returns Tracing class instance for event attaching.
        /// </summary>
        public static Tracing Tracing => Tracing.Instance;

        /// <summary>
        /// Writes a trace statement.
        /// </summary>
        private static void TraceWriteLine(string message, params object[] args)
        {
            // null strings not supported.
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            // format the message if format arguments provided.
            var output = message;

            if (args != null && args.Length > 0)
            {
                try
                {
                    output = string.Format(CultureInfo.InvariantCulture, message, args);
                }
                catch (Exception)
                {
                    output = message;
                }
            }

            // write to the log file.
            lock (traceFileLock_)
            {
                // write to debug trace listeners.
                if (traceOutput_ == (int)TraceOutput.DebugAndFile)
                {
                    Debug.WriteLine(output);
                }

                // write to trace listeners.
                if (traceOutput_ == (int)TraceOutput.StdOutAndFile)
                {
                    System.Diagnostics.Trace.WriteLine(output);
                }

                var traceFileName = traceFileName_;

                if (traceOutput_ != (int)TraceOutput.Off && !string.IsNullOrEmpty(traceFileName))
                {
                    try
                    {
                        var file = new FileInfo(traceFileName);

                        // limit the file size. hard coded for now - fix later.
                        var truncated = false;

                        if (file.Exists && file.Length > 10000000)
                        {
                            file.Delete();
                            truncated = true;
                        }

                        using (var writer = new StreamWriter(File.Open(traceFileName, FileMode.Append)))
                        {
                            if (truncated)
                            {
                                writer.WriteLine("WARNING - LOG FILE TRUNCATED.");
                            }

                            writer.WriteLine(output);
                            writer.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(@"Could not write to trace file. Error={0} FilePath={1}", e.Message, traceFileName);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the path to the log file to use for tracing.
        /// </summary>
        public static void SetTraceLog(string filePath, bool deleteExisting)
        {
            // turn tracing on.
            lock (traceFileLock_)
            {
                // check if tracing is being turned off.
                if (string.IsNullOrEmpty(filePath))
                {
                    traceFileName_ = null;
                    return;
                }

                traceFileName_ = GetAbsoluteFilePath(filePath, true, false, true);

                if (traceOutput_ == (int)TraceOutput.Off)
                {
                    traceOutput_ = (int)TraceOutput.FileOnly;
                }

                try
                {
                    var file = new FileInfo(traceFileName_);

                    if (deleteExisting && file.Exists)
                    {
                        file.Delete();
                    }

                    // write initial log message.
                    TraceWriteLine(
                        "\r\nPID:{2} {1} Logging started at {0} {1}",
                        DateTime.Now,
                        new string('*', 25),
                        Process.GetCurrentProcess().Id);
                }
                catch (Exception e)
                {
                    TraceWriteLine(e.Message, null);
                }
            }
        }

        /// <summary>
        /// Writes an informational message to the trace log.
        /// </summary>
        public static void Trace(string format, params object[] args)
        {
            Trace(TraceMasks.Information, format, false, args);
        }

        /// <summary>
        /// Writes an exception/error message to the trace log.
        /// </summary>
        public static void Trace(Exception e, string format, params object[] args)
        {
            Trace(e, format, false, args);
        }

        /// <summary>
        /// Writes a general message to the trace log.
        /// </summary>
        public static void Trace(int traceMask, string format, params object[] args)
        {
            Trace(traceMask, format, false, args);
        }

        /// <summary>
        /// Writes an exception/error message to the trace log.
        /// </summary>
        internal static void Trace(Exception e, string format, bool handled, params object[] args)
        {
            var message = new StringBuilder();

            // format message.            
            if (args != null && args.Length > 0)
            {
                try
                {
                    message.AppendFormat(CultureInfo.InvariantCulture, format, args);
                }
                catch (Exception)
                {
                    message.Append(format);
                }
            }
            else
            {
                message.Append(format);
            }

            // append exception information.
            if (e != null)
            {
                if (e is OpcResultException sre)
                {
                    message.AppendFormat(CultureInfo.InvariantCulture, " {0} '{1}'", sre.Result.Code, sre.Message);
                }
                else
                {
                    message.AppendFormat(CultureInfo.InvariantCulture, " {0} '{1}'", e.GetType().Name, e.Message);
                }

                // append stack trace.
                if ((traceMasks_ & TraceMasks.StackTrace) != 0)
                {
                    message.AppendFormat(CultureInfo.InvariantCulture, "\r\n\r\n{0}\r\n", new string('=', 40));
                    message.Append(e.StackTrace);
                    message.AppendFormat(CultureInfo.InvariantCulture, "\r\n{0}\r\n", new string('=', 40));
                }
            }

            // trace message.
            Trace(TraceMasks.Error, message.ToString(), handled, null);
        }

        /// <summary>
        /// Writes the message to the trace log.
        /// </summary>
        private static void Trace(int traceMask, string format, bool handled, params object[] args)
        {
            if (!handled)
            {
                Tracing.Instance.RaiseTraceEvent(new TraceEventArgs(traceMask, format, string.Empty, null, args));
            }

            // do nothing if mask not enabled.
            if ((traceMasks_ & traceMask) == 0)
            {
                return;
            }

            var message = new StringBuilder();

            // append process and timestamp.
            message.AppendFormat("{0} - ", Process.GetCurrentProcess().Id);
            message.AppendFormat("{0:d} {0:HH:mm:ss.fff} ", HiResClock.UtcNow.ToLocalTime());

            // format message.
            if (args != null && args.Length > 0)
            {
                try
                {
                    message.AppendFormat(CultureInfo.InvariantCulture, format, args);
                }
                catch (Exception)
                {
                    message.Append(format);
                }
            }
            else
            {
                message.Append(format);
            }

            TraceWriteLine(message.ToString(), null);
        }

        #endregion

        #region File Access

        /// <summary>
        /// Replaces a prefix enclosed in '%' with a special folder or environment variable path (e.g. %ProgramFiles%\MyCompany).
        /// </summary>
        public static string ReplaceSpecialFolderNames(string input)
        {
            // nothing to do for nulls.
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            // check for absolute path.
            if (input.Length > 1 && ((input[0] == '\\' && input[1] == '\\') || input[1] == ':'))
            {
                return input;
            }

            // check for special folder prefix.
            if (input[0] != '%')
            {
                return input;
            }

            // extract special folder name.
            string folder;
            string path;

            var index = input.IndexOf('%', 1);

            if (index == -1)
            {
                folder = input.Substring(1);
                path = string.Empty;
            }
            else
            {
                folder = input.Substring(1, index - 1);
                path = input.Substring(index + 1);
            }

            var buffer = new StringBuilder();

            // check for special folder.
            try
            {
                var specialFolder = (Environment.SpecialFolder)Enum.Parse(
                    typeof(Environment.SpecialFolder),
                    folder,
                    true);

                buffer.Append(Environment.GetFolderPath(specialFolder));
            }

            // check for generic environment variable.
            catch (Exception)
            {
                var value = Environment.GetEnvironmentVariable(folder);

                if (value != null)
                {
                    buffer.Append(value);
                }
            }

            // construct new path.
            buffer.Append(path);
            return buffer.ToString();
        }

        /// <summary>
        /// Checks if the file path is a relative path and returns an absolute path relative to the EXE location.
        /// </summary>
        public static string GetAbsoluteFilePath(string filePath)
        {
            return GetAbsoluteFilePath(filePath, false, true, false);
        }

        /// <summary>
        /// Checks if the file path is a relative path and returns an absolute path relative to the EXE location.
        /// </summary>
        public static string GetAbsoluteFilePath(string filePath, bool checkCurrentDirectory, bool throwOnError, bool createAlways)
        {
            filePath = ReplaceSpecialFolderNames(filePath);

            if (!string.IsNullOrEmpty(filePath))
            {
                var file = new FileInfo(filePath);

                // check for absolute path.
                var isAbsolute = filePath.StartsWith("\\\\", StringComparison.Ordinal) || filePath.IndexOf(':') == 1;

                if (isAbsolute)
                {
                    if (file.Exists)
                    {
                        return filePath;
                    }

                    if (createAlways)
                    {
                        return CreateFile(file, filePath, throwOnError);
                    }
                }

                if (!isAbsolute)
                {
                    // look current directory.
                    if (checkCurrentDirectory)
                    {
                        if (!file.Exists)
                        {
                            file = new FileInfo(Format("{0}\\{1}", Environment.CurrentDirectory, filePath));
                        }

                        if (file.Exists)
                        {
                            return file.FullName;
                        }

                        if (createAlways)
                        {
                            return CreateFile(file, filePath, throwOnError);
                        }
                    }

                    // look executable directory.
                    if (!file.Exists)
                    {
                        var executablePath = Environment.GetCommandLineArgs()[0];
                        var executable = new FileInfo(executablePath);

                        if (executable.Exists)
                        {
                            file = new FileInfo(Format("{0}\\{1}", executable.DirectoryName, filePath));
                        }

                        if (file.Exists)
                        {
                            return file.FullName;
                        }

                        if (createAlways)
                        {
                            return CreateFile(file, filePath, throwOnError);
                        }
                    }
                }
            }

            // file does not exist.
            if (throwOnError)
            {
                throw new OpcResultException(new OpcResult(OpcResult.CONNECT_E_NOCONNECTION.Code, OpcResult.FuncCallType.SysFuncCall, null),
                    $"File does not exist: {filePath}\r\nCurrent directory is: {Environment.CurrentDirectory}");
            }

            return null;
        }

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        private static string CreateFile(FileInfo file, string filePath, bool throwOnError)
        {
            try
            {
                // create the directory as required.
                if (file.Directory != null && !file.Directory.Exists)
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }

                // open and close the file.
                using (file.Open(FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    return filePath;
                }
            }
            catch (Exception)
            {
                if (throwOnError)
                {
                    throw;
                }

                return filePath;
            }
        }

        /// <summary>
        /// Formats a message using the invariant locale.
        /// </summary>
        public static string Format(string text, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, text, args);
        }
        #endregion
    }

    #region Tracing Class
    /// <summary>
    /// Used as underlying tracing object for event processing.
    /// </summary>
    public class Tracing
    {
        #region Private Members
        private static object syncRoot_ = new object();
        private static Tracing instance_;
        #endregion Private Members

        #region Singleton Instance
        /// <summary>
        /// Private constructor.
        /// </summary>
        private Tracing()
        { }

        /// <summary>
        /// Internal Singleton Instance getter.
        /// </summary>
        internal static Tracing Instance
        {
            get
            {
                if (instance_ == null)
                {
                    lock (syncRoot_)
                    {
                        if (instance_ == null)
                        {
                            instance_ = new Tracing();
                        }
                    }
                }
                return instance_;
            }
        }
        #endregion Singleton Instance

        #region Public Events
        /// <summary>
        /// Occurs when a trace call is made.
        /// </summary>
        public event EventHandler<TraceEventArgs> TraceEventHandler;
        #endregion Public Events

        internal void RaiseTraceEvent(TraceEventArgs eventArgs)
        {
            if (TraceEventHandler != null)
            {
                try
                {
                    TraceEventHandler(this, eventArgs);
                }
                catch (Exception ex)
                {
                    Utils.Trace(ex, "Exception invoking Trace Event Handler", true, null);
                }
            }
        }
    }
    #endregion Tracing Class

    #region TraceEventArgs Class
    /// <summary>
    /// The event arguments provided when a trace event is raised.
    /// </summary>
    public class TraceEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the TraceEventArgs class.
        /// </summary>
        /// <param name="traceMask">The trace mask.</param>
        /// <param name="format">The format.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="args">The arguments.</param>
        internal TraceEventArgs(int traceMask, string format, string message, Exception exception, object[] args)
        {
            TraceMask = traceMask;
            Format = format;
            Message = message;
            Exception = exception;
            Arguments = args;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets the trace mask.
        /// </summary>
        public int TraceMask { get; }

        /// <summary>
        /// Gets the format.
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; }
        #endregion Public Properties
    }
    #endregion TraceEventArgs Class
}
