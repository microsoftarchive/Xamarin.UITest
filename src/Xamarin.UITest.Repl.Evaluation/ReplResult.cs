namespace Xamarin.UITest.Repl.Evaluation
{
    public class ReplResult
    {
        private readonly bool _hasValue;
        private readonly object _value;
        private readonly string _error;

        public ReplResult(bool hasValue, object value, string error)
        {
            _hasValue = hasValue;
            _value = value;
            _error = error;
        }

        public bool HasValue
        {
            get { return _hasValue; }
        }

        public object Value
        {
            get { return _value; }
        }

        public bool HasError
        {
            get { return !string.IsNullOrEmpty(_error); }
        }

        public string Error
        {
            get { return _error; }
        }
    }
}