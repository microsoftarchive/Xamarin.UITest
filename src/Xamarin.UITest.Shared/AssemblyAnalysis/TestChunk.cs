using System;
using System.Text;

namespace Xamarin.UITest.Shared.AssemblyAnalysis
{
    /// <summary>
    /// Representation of test chunk.
    /// </summary>
    public class TestChunk : IEquatable<TestChunk>
    {
        /// <summary>
        /// Assembly file's name.
        /// </summary>
        public string AssemblyFileName { private set; get; }

        /// <summary>
        /// Namespace of test target. Actually it's types inside <see cref="AssemblyFileName"/> file.
        /// </summary>
        public string TestTarget { private set; get; }

        /// <summary>
        /// Reason why <see cref="TestTarget"/> is excluded.
        /// </summary>
        public string ExcludeReason { private set; get; }

        /// <summary>
        /// Initializes <see cref="TestChunk"/> instance.
        /// </summary>
        /// <param name="assemblyFileName">Name of assembly's file.</param>
        /// <param name="testTarget">Name of test target.</param>
        /// <param name="excludeReason">Reason why test target is excluded.</param>
        public TestChunk(string assemblyFileName, string testTarget, string excludeReason)
        {
            AssemblyFileName = assemblyFileName;
            TestTarget = testTarget;
            ExcludeReason = excludeReason;
        }

        /// <summary>
        /// Indicates if current <see cref="TestChunk"/> is excluded.
        /// </summary>
        public bool IsExcluded()
        {
            return !string.IsNullOrWhiteSpace(ExcludeReason);
        }

        /// <returns><see cref="string"/> representation of <see cref="TestChunk"/> instance.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (IsExcluded())
            {
                stringBuilder.AppendFormat("EXCLUDED ({0}): ", ExcludeReason);
            }
            stringBuilder.AppendFormat("{0} ({1})", TestTarget, AssemblyFileName);
            return stringBuilder.ToString();
        }

        #region Equality members

        public bool Equals(TestChunk other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(AssemblyFileName, other.AssemblyFileName) && string.Equals(TestTarget, other.TestTarget) && string.Equals(ExcludeReason, other.ExcludeReason);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestChunk) obj);
        }

        public override int GetHashCode()
        {
#if NET6_0_OR_GREATER
            return HashCode.Combine(AssemblyFileName, TestTarget, ExcludeReason);
#else
            unchecked
            {
                var hashCode = (AssemblyFileName != null ? AssemblyFileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TestTarget != null ? TestTarget.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExcludeReason != null ? ExcludeReason.GetHashCode() : 0);
                return hashCode;
            }
#endif
        }

        public static bool operator ==(TestChunk left, TestChunk right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TestChunk left, TestChunk right)
        {
            return !Equals(left, right);
        }

#endregion
    }
}