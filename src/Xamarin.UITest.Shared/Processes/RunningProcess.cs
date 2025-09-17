using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.UITest.Shared.Extensions;
using System;

namespace Xamarin.UITest.Shared.Processes
{
    internal class RunningProcess
    {
        internal class MaxCapacityQueue<T> : IEnumerable<T>
        {
            readonly int _capacity;
            readonly Queue<T> _queue;

            public MaxCapacityQueue(int capacity) 
            {
                _capacity = capacity;
                _queue = new Queue<T>();
            }

            public void Enqueue(T item) 
            {
                _queue.Enqueue(item);
                if (_capacity != -1 && _queue.Count > _capacity)
                {
                    _queue.Dequeue();
                }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return _queue.GetEnumerator();
            }

            public IEnumerator GetEnumerator()
            {
                return _queue.GetEnumerator();
            }

            public T[] ToArray() 
            {
                return _queue.ToArray();
            }

            public void Clear()
            {
                _queue.Clear();
            }
        }

        readonly Process _process;
        readonly MaxCapacityQueue<ProcessOutput> _processOutput;
        readonly object _processsOutputLock = new object();
        readonly Stopwatch _stopwatch;

        /// <summary>
        /// Warning: it's possible that not all output will be captured from the process.  If you can wait for the
        /// process to complete then use <c>ProcessRunner.Run()</c> instead of <c>RunningProcess</c>.
        /// 
        /// See http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/ for more
        /// info, although we have experienced data loss even though we're not using process.WaitForExit(timeout).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="arguments"></param>
        /// <param name="dropFilter"></param>
        /// <param name="maxNumberOfLines"></param>
        public RunningProcess (string path, string arguments, Predicate<string> dropFilter = null, int maxNumberOfLines = -1)
        {
            _stopwatch = Stopwatch.StartNew ();

            _processOutput = new MaxCapacityQueue<ProcessOutput>(maxNumberOfLines);

            var psi = new ProcessStartInfo();

            psi.FileName = path;
            psi.Arguments = arguments;

            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            _process = new Process {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data == null || (dropFilter != null && dropFilter(args.Data)))
                    {
                        return;
                    }

                    lock (_processsOutputLock)
                    {
                        _processOutput.Enqueue(new ProcessOutput(args.Data));
                    }
                };

            _process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data == null)
                    {
                        return;
                    }

                    lock (_processsOutputLock)
                    {
                        _processOutput.Enqueue(new ProcessOutput(args.Data));
                    }
                };

            _process.Start();

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            _process.Exited += (sender, e) => 
                {
                    _stopwatch.Stop();
                };
        }

        public ProcessResult WaitForExit()
        {
            _process.WaitForExit ();
            return GetOutput ();
        }

        public void Kill()
        {
            _process.Kill();
        }

        /// <summary>
        /// Warning: it's possible that not all output will be captured from the process.  If you can wait for the
        /// process to complete then use <c>ProcessRunner.Run()</c> instead of <c>RunningProcess</c>.
        /// 
        /// See http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/ for more
        /// info, although we have experienced data loss even though we're not using process.WaitForExit(timeout).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="arguments"></param>
        /// <param name="dropFilter"></param>
        /// <param name="maxNumberOfLines"></param>
        public ProcessResult GetOutput(bool removeEmptyLines = false)
        {
            var exitCode = _process.HasExited ? _process.ExitCode : 0;

            lock (_processsOutputLock)
            {
                if (removeEmptyLines)
                {
                    return new ProcessResult(_processOutput.Where(x => !x.Data.IsNullOrWhiteSpace()).ToArray(), 
                        exitCode, _stopwatch.ElapsedMilliseconds, _process.HasExited);
                }

                return new ProcessResult(_processOutput.ToArray(), exitCode, _stopwatch.ElapsedMilliseconds,
                    _process.HasExited);
            }
        }

        /// <summary>
        /// Warning: it's possible that not all output will be captured from the process.  If you can wait for the
        /// process to complete then use <c>ProcessRunner.Run()</c> instead of <c>RunningProcess</c>.
        /// 
        /// See http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/ for more
        /// info, although we have experienced data loss even though we're not using process.WaitForExit(timeout).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="arguments"></param>
        /// <param name="dropFilter"></param>
        /// <param name="maxNumberOfLines"></param>
        public ProcessResult GetOutputAndFlush(bool removeEmptyLines = false)
        {
            var exitCode = _process.HasExited ? _process.ExitCode : 0;

            lock (_processsOutputLock)
            {
                var processOutputArray = removeEmptyLines
                    ? _processOutput.Where(x => !x.Data.IsNullOrWhiteSpace()).ToArray()
                    : _processOutput.ToArray();

                _processOutput.Clear();
                
                return new ProcessResult(
                    processOutputArray, exitCode, _stopwatch.ElapsedMilliseconds, _process.HasExited);
            }
        }
    }
}
