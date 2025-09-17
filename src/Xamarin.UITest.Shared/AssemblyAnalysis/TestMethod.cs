using System;

namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    /// <summary>
    /// Representation of TestMethod which provides access to test method's properties.
    /// </summary>
    public class TestMethod : IEquatable<TestMethod>
    {
        public string FullName { get; }
        public string TypeName { get; }
        public string MethodName { get; }
        public string AssemblyFileName { get; }
        public string ExcludeReason { get; }
        public bool IsExcluded
        {
            get { return !string.IsNullOrWhiteSpace(ExcludeReason); }
        }

        public TestMethod(string typeName, string methodName, string fullName, string assemblyFileName, string excludeReason)
        {
            AssemblyFileName = assemblyFileName;
            MethodName = methodName;
            TypeName = typeName;
            ExcludeReason = excludeReason;
            FullName = fullName;
        }

        public override string ToString()
        {
            var str = string.Empty;

            if (IsExcluded)
            {
                str += string.Format("EXCLUDED ({0}): ", ExcludeReason);
            }

            str += string.Format("{0} ({1})", MethodName, TypeName);

            return str;
        }

        public bool Equals(TestMethod other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FullName, other.FullName) && string.Equals(AssemblyFileName, other.AssemblyFileName);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestMethod) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FullName != null ? FullName.GetHashCode() : 0)*397) ^ (AssemblyFileName != null ? AssemblyFileName.GetHashCode() : 0);
            }
        }
    }
}
