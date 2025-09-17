namespace Xamarin.UITest.Repl
{
	public class UITestExtensionMethod
	{
        readonly string _assemblyFile;
        readonly string _typeNameSpace;
        readonly string _typeName;
        readonly string _methodName;

        public UITestExtensionMethod(string assemblyFile, string typeNameSpace, string typeName, string methodName)
        {
            _methodName = methodName;
            _typeName = typeName;
            _typeNameSpace = typeNameSpace;
            _assemblyFile = assemblyFile;
        }

        public string AssemblyFile
        {
            get { return _assemblyFile; }
        }

        public string TypeNamespace
        {
            get { return _typeNameSpace; }
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        public string MethodName
        {
            get { return _methodName; }
        }
	}
}
