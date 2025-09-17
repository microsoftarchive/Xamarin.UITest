using System;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.iOS
{
    internal class EnsureVersion
    {
        readonly VersionNumber _currentVersion;
        readonly string _name;

        public EnsureVersion(VersionNumber currentVersion, string name)
        {
            _currentVersion = currentVersion;
            _name = name;
        }

        public void AtLeast(VersionNumber atLeastVersion, Action action, string optionalErrorMessage = null)
        {
            if (_currentVersion < atLeastVersion)
            {
                var message = optionalErrorMessage ?? "A newer version of {0} is needed. Has version {1} and needs at least version {2}";
                throw new Exception(string.Format(message, _name, _currentVersion, atLeastVersion));
            }
            action();
        }

        public void AtLeast(string atLeastVersion, Action action, string optionalErrorMessage = null)
        {
            AtLeast(new VersionNumber(atLeastVersion), action, optionalErrorMessage);
        }


        public void LessThan(VersionNumber maxVersion, Action action, string optionalErrorMessage = null)
        {
            if (_currentVersion >= maxVersion)
            {
                var message = optionalErrorMessage ?? "A older version of {0} is needed. Has version {1} and must be less that version {2}";
                throw new Exception(string.Format(message, _name, _currentVersion, maxVersion));
            }
            action();
        }

        public void LessThan(string maxVersion, Action action, string optionalErrorMessage = null)
        {
            LessThan(new VersionNumber(maxVersion), action, optionalErrorMessage);
        }


        public void IfAtLeast(string atLeastVersion, Action trueAction, Action falseAction)
        {
            if (_currentVersion < new VersionNumber(atLeastVersion))
            {
                falseAction();
            }
            else
            {
                trueAction();
            }
        }

        public T AtLeastWithFallback<T>(string atLeastVersion, Func<T> func, Func<T> fallbackFunc)
        {
            return _currentVersion < new VersionNumber(atLeastVersion) ? fallbackFunc() : func();
        }
    }
}
