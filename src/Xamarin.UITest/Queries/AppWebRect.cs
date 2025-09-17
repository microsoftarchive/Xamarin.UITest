namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Representation of a web elements position and size.
    /// </summary>
    public class AppWebRect : IRect
    {
        /// <summary>
        /// Determines whether the specified <see cref="Xamarin.UITest.Queries.AppWebRect"/> is equal to the current 
        /// <see cref="T:Xamarin.UITest.Queries.AppWebRect"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Xamarin.UITest.Queries.AppWebRect"/> to compare with the current 
        /// <see cref="T:Xamarin.UITest.Queries.AppWebRect"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Xamarin.UITest.Queries.AppWebRect"/> is equal to the current
        /// <see cref="T:Xamarin.UITest.Queries.AppWebRect"/>; otherwise, <c>false</c>.
        /// </returns>
        protected bool Equals(AppWebRect other)
        {
            return Width.Equals(other.Width) && Height.Equals(other.Height) && CenterX.Equals(other.CenterX) && CenterY.Equals(other.CenterY) && Top.Equals(other.Top) && Bottom.Equals(other.Bottom) && Left.Equals(other.Left) && Right.Equals(other.Right) && X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current 
        /// <see cref="T:Xamarin.UITest.Queries.AppWebRect"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare with the current <see cref="T:Xamarin.UITest.Queries.AppWebRect"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Xamarin.UITest.Queries.AppWebRect"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AppWebRect) obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Xamarin.UITest.Queries.AppWebRect"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Width.GetHashCode();
                hashCode = (hashCode*397) ^ Height.GetHashCode();
                hashCode = (hashCode*397) ^ CenterX.GetHashCode();
                hashCode = (hashCode*397) ^ CenterY.GetHashCode();
                hashCode = (hashCode*397) ^ Top.GetHashCode();
                hashCode = (hashCode*397) ^ Bottom.GetHashCode();
                hashCode = (hashCode*397) ^ Left.GetHashCode();
                hashCode = (hashCode*397) ^ Right.GetHashCode();
                hashCode = (hashCode*397) ^ X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// The width of the element.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        ///  The height of the element.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        ///  The X coordinate of the center of the element.
        /// </summary>
        public float CenterX { get; set; }

        /// <summary>
        /// The Y coordinate of the center of the element."
        /// </summary>
        public float CenterY { get; set; }

        /// <summary>
        /// The top property of the element.
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// The bottom property of the element.
        /// </summary>
        public float Bottom { get; set; }

        /// <summary>
        /// The left property of the element.
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// The right property of the element.
        /// </summary>
        public float Right { get; set; }

        /// <summary>
        /// The X property of the element, unit is screen coordinates
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The Y property of the element, unit is screen coordinates
        /// </summary>
        public float Y { get; set; }

    }
}