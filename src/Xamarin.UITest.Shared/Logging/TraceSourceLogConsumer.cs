using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.UITest.Shared.Logging
{
    public class TraceSourceLogConsumer : ILogConsumer
    {
        /// <summary>
        /// The key into <see cref="AppDomain.GetData"/> to retrieve the <see cref="TraceSource"/> 
        /// to configure the tracing behavior of this log consumer.
        /// </summary>
        /// <remarks>
        /// External libraries (or IDEds) can configure the trace source used by UITest by replacing 
        /// or configuring the trace source in the AppDomain as follows:
        /// <code>
        /// // Configure existing
        /// var traceSource = (TraceSource)AppDomain.CurrentDomain.GetData(TraceSourceLogConsumer.AppDomainTraceSourceKey);
        /// traceSource.Listeners.Add(myListener);
        /// 
        /// // Replace with new source. This must be done before the TraceSourceLogConsumer static constructor runs.
        /// AppDomain.CurrentDomain.SetData(TraceSourceLogConsumer.AppDomainTraceSourceKey, mySource);
        /// traceSource.Listeners.Add(myListener);
        /// </code>
        /// </remarks>
        public const string AppDomainTraceSourceKey = "System.Diagnostics.TraceSource::Xamarin.UITest";

        public static TraceSource TraceSource
        {
            get
            {
                // Grabbing and placing the trace source in the app domain allows anyone in the 
                // same process to grab the trace source and modify its listeners at runtime. 
                // This is needed since .NET does not provide any other way of accessing 
                // a trace source to modify its configuration at runtime, and form within VS 
                // we can't either register a listener via the process .config since that would 
                // be devenv.exe.config, which is in Program Files and can't be modified without 
                // elevation (and would be highly suspicious and non-modifyable once VS is 
                // running. So this is the lesser evil, I guess :(.
                var traceSource = AppDomain.CurrentDomain.GetData(AppDomainTraceSourceKey) as TraceSource;
                
                // If the AppDomain happened to contain an object that isn't a TraceSource, we just overwrite it.
                if (traceSource == null)
                {
                    var sourceName = AppDomainTraceSourceKey.Substring(32);
                    traceSource = new TraceSource(sourceName, SourceLevels.Information);
                    
                    TraceSource = traceSource;
                }

                return traceSource;
            }
            set
            {
                AppDomain.CurrentDomain.SetData(AppDomainTraceSourceKey, value);
            }
        }
       
        public void Consume(LogEntry logEntry)
        {
            switch (logEntry.LogLevel)
            {
                case LogLevel.Debug:
                    TraceSource.TraceEvent(TraceEventType.Verbose, 0, logEntry.Message);
                    break;
                case LogLevel.Info:
                    TraceSource.TraceInformation(logEntry.Message);
                    break;
                default:
                    break;
            }
        }
    }
}
