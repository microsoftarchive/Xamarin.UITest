using System;
using System.Text.RegularExpressions;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Utils
{
    /// <summary>
    /// An object representing a version with the format <![CDATA[ <major>.<minor>.<build>.<revision>-<label> ]]>.
    /// </summary>
    public class VersionNumber : IVersionNumber, IComparable<VersionNumber>, IEquatable<VersionNumber>
    {
        readonly string _label;
        readonly int _major;
        readonly int _minor;
        readonly int _buildOrPatch;
        readonly int _revision;
        readonly string _rawVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Xamarin.UITest.Utils.VersionNumber"/> class.
        /// </summary>
        /// <param name="versionStr">A version string used to set the version's value.</param>
        public VersionNumber(string versionStr)
        {
            var match = Regex.Match(versionStr, @"(?<major>\d+)(\.(?<minor>\d+))?(\.(?<build>\d+))?(\.(?<revision>\d+))?([\.\-](?<label>.+))?");

            _major = TryParseIntOr(match.Groups["major"].Value, 0);
            _minor = TryParseIntOr(match.Groups["minor"].Value, 0);
            _buildOrPatch = TryParseIntOr(match.Groups["build"].Value, 0);
            _revision = TryParseIntOr(match.Groups["revision"].Value, 0);
            _label = match.Groups["label"].Value;

            _rawVersion = versionStr;
        }

        /// <summary>
        /// The version's label component
        /// </summary>
        /// <value>The label.</value>
        public string Label
        {
            get { return _label; }
        }

        /// <summary>
        /// The version string used to initialize this <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <value>The raw version.</value>
        public string RawVersion
        {
            get { return _rawVersion; }
        }

        /// <summary>
        /// The version's major number
        /// </summary>
        public int Major
        {
            get { return _major; }
        }

        /// <summary>
        /// The version's minor number
        /// </summary>
        public int Minor
        {
            get { return _minor; }
        }

        /// <summary>
        /// The version's build number
        /// </summary>
        /// <value>The build or patch.</value>
        public int BuildOrPatch
        {
            get { return _buildOrPatch; }
        }

        /// <summary>
        /// The version's revision number
        /// </summary>
        /// <value>The revision.</value>
        public int Revision
        {
            get { return _revision; }
        }

        /// <summary>
        /// Compares two <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>s.
        /// </summary>
        /// <param name="v1">A <see cref="T:Xamarin.UITest.Utils.VersionNumber"/> to compare</param>
        /// <param name="v2">A <see cref="T:Xamarin.UITest.Utils.VersionNumber"/> to compare</param>
        public static int CompareTo(VersionNumber v1, VersionNumber v2)
        {
            if (ReferenceEquals(v1, null) && ReferenceEquals(v2, null))
            {
                return 0;
            }

            if (ReferenceEquals(v1, null))
            {
                return -1;
            }

            if (ReferenceEquals(v2, null))
            {
                return 1;
            }

            if (v1.Major - v2.Major != 0)
            {
                return v1.Major - v2.Major;
            }

            if (v1.Minor - v2.Minor != 0)
            {
                return v1.Minor - v2.Minor;
            }

            if (v1.BuildOrPatch - v2.BuildOrPatch != 0)
            {
                return v1.BuildOrPatch - v2.BuildOrPatch;
            }

            if (v1.Revision - v2.Revision != 0)
            {
                return v1.Revision - v2.Revision;
            }

            if (v1.Label.IsNullOrWhiteSpace() && v2.Label.IsNullOrWhiteSpace())
            {
                return 0;
            }

            if (!v1.Label.IsNullOrWhiteSpace() && !v2.Label.IsNullOrWhiteSpace())
            {
                return string.Compare(v1.Label, v2.Label, StringComparison.InvariantCultureIgnoreCase);
            }

            if (v1.Label.IsNullOrWhiteSpace())
            {
                return 1;
            }

            if (v2.Label.IsNullOrWhiteSpace())
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current 
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current 
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </returns>
        public override string ToString()
        {
            return RawVersion;
        }

        /// <summary>
        /// Compares the current <see cref="T:Xamarin.UITest.Utils.VersionNumber"/> with the provided
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="v1">
        /// The <see cref="T:Xamarin.UITest.Utils.VersionNumber"/> to compare this 
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/> with.
        /// </param>
        public int CompareTo(VersionNumber v1) { return CompareTo(this, v1); }

        /// <summary>
        /// Determines whether one specified <see cref="Xamarin.UITest.Utils.VersionNumber"/> is lower than another
        /// specfied <see cref="Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="x">The first <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <param name="y">The second <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <returns><c>true</c> if <c>x</c> is lower than <c>y</c>; otherwise, <c>false</c>.</returns>
        public static bool operator <(VersionNumber x, VersionNumber y) { return CompareTo(x, y) < 0; }

        /// <summary>
        /// Determines whether one specified <see cref="Xamarin.UITest.Utils.VersionNumber"/> is greater than another
        /// specfied <see cref="Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="x">The first <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <param name="y">The second <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <returns><c>true</c> if <c>x</c> is greater than <c>y</c>; otherwise, <c>false</c>.</returns>
        public static bool operator >(VersionNumber x, VersionNumber y) { return CompareTo(x, y) > 0; }

        /// <summary>
        /// Determines whether one specified <see cref="Xamarin.UITest.Utils.VersionNumber"/> is lower than or equal to
        /// another specfied <see cref="Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="x">The first <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <param name="y">The second <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <returns><c>true</c> if <c>x</c> is lower than or equal to <c>y</c>; otherwise, <c>false</c>.</returns>
        public static bool operator <=(VersionNumber x, VersionNumber y) { return CompareTo(x, y) <= 0; }

        /// <summary>
        /// Determines whether one specified <see cref="Xamarin.UITest.Utils.VersionNumber"/> is greater than or equal
        /// to another specfied <see cref="Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="x">The first <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <param name="y">The second <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <returns><c>true</c> if <c>x</c> is greater than or equal to <c>y</c>; otherwise, <c>false</c>.</returns>
        public static bool operator >=(VersionNumber x, VersionNumber y) { return CompareTo(x, y) >= 0; }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Xamarin.UITest.Utils.VersionNumber"/> is equal to
        /// another specified <see cref="Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="x">The first <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <param name="y">The second <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <returns><c>true</c> if <c>x</c> and <c>y</c> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(VersionNumber x, VersionNumber y) { return CompareTo(x, y) == 0; }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Xamarin.UITest.Utils.VersionNumber"/> is not equal to
        /// another specified <see cref="Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="x">The first <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <param name="y">The second <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare.</param>
        /// <returns><c>true</c> if <c>x</c> and <c>y</c> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(VersionNumber x, VersionNumber y) { return CompareTo(x, y) != 0; }

        /// <summary>
        /// Determines whether the specified <see cref="Xamarin.UITest.Utils.VersionNumber"/> is equal to the current
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="x">
        /// The <see cref="Xamarin.UITest.Utils.VersionNumber"/> to compare with the current 
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Xamarin.UITest.Utils.VersionNumber"/> is equal to the current
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(VersionNumber x)
        {
            if (ReferenceEquals(null, x)) return false;
            if (ReferenceEquals(this, x)) return true;
            return string.Equals(_label, x._label) && _major == x._major && _minor == x._minor && _buildOrPatch == x._buildOrPatch && _revision == x._revision;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current 
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare with the current <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Xamarin.UITest.Utils.VersionNumber"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VersionNumber) obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Xamarin.UITest.Utils.VersionNumber"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_label != null ? _label.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ _major;
                hashCode = (hashCode*397) ^ _minor;
                hashCode = (hashCode*397) ^ _buildOrPatch;
                hashCode = (hashCode*397) ^ _revision;
                return hashCode;
            }
        }

        int TryParseIntOr(string value, int fallback)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return fallback;
            }

            int intValue;

            if (int.TryParse(value, out intValue))
            {
                return intValue;
            }

            return fallback;
        }
    }
}