namespace Xamarin.UITest.Repl
{
    internal class HistoryEntry
    {
        readonly string _entry;
        readonly bool _success;

        public HistoryEntry(string entry, bool success)
        {
            _entry = entry;
            _success = success;
        }

        public bool Success
        {
            get { return _success; }
        }

        public string Entry
        {
            get { return _entry; }
        }
    }
}