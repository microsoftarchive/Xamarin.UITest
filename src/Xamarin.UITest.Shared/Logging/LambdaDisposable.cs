using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.Logging
{
    public class LambdaDisposable : IDisposable
    {
        readonly Action _disposeAction;
        bool _disposed;

        public LambdaDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _disposeAction();
        }
    }
}