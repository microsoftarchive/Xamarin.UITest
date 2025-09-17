using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Utils
{
    internal class ErrorReporting
    {
        private readonly QueryPlatform _platform;

        public ErrorReporting(QueryPlatform platform)
        {
            _platform = platform;
        }


        internal T With<T>(Func<T> func, object[] args = null, [CallerMemberName] string memberName = null)
        {
            UnhandledExceptionWorkaround.HandleUncaughtExceptionsFromOtherThreads();
            T result = default(T);
            try
            {
                result = func();
            }
            catch (Exception e)
            {
                if (memberName != null)
                {
                    string argsString = args == null ? string.Empty : string.Join(", ", args.Select(Format));
                    string message = string.Format("Error while performing {0}({1})", memberName, argsString);
                    Log.Info(message, e);
                    throw new Exception(message, e);
                }
                throw;
            }
            UnhandledExceptionWorkaround.HandleUncaughtExceptionsFromOtherThreads();
            return result;
        }

       internal void With(Action func, object[] args = null, [CallerMemberName] string memberName = null)
       {
           UnhandledExceptionWorkaround.HandleUncaughtExceptionsFromOtherThreads();
           try
           {
               func();
           }
           catch (Exception e)
           {
               if (memberName != null)
               {
                   string argsString = args == null ? string.Empty : string.Join(", ", args.Select(Format));
                   string message = string.Format("Error while performing {0}({1})", memberName, argsString);
                   
                   Log.Info(message, e);
                   throw new Exception(message, e);
               }
               throw;
           }
           UnhandledExceptionWorkaround.HandleUncaughtExceptionsFromOtherThreads();
       }

        string Format(object o)
        {
            try
            {
                if (o == null)
                {
                    return "null";
                }
                if (o is Func<AppQuery, AppQuery>)
                {
                    var func = (Func<AppQuery, AppQuery>)o;
                    return TokenCodePrinter.ToCodeString(func(new AppQuery(_platform)));
                }
                else if (o is ITokenContainer)
                {
                    var tc = (ITokenContainer)o;
                    return TokenCodePrinter.ToCodeString(tc);
                }
                else if (o is Func<AppQuery, ITokenContainer>)
                {
                    var func = (Func<AppQuery, ITokenContainer>)o;
                    return TokenCodePrinter.ToCodeString(func(new AppQuery(_platform)));
                }
                else 
                {
                    var stringify = o.Stringify();
                    return !stringify.IsNullOrWhiteSpace() ? stringify : o.ToString();
                }
            }
            catch 
            {
                return "[unknown]";
            }
        }
    }
}