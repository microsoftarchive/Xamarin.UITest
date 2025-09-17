namespace Xamarin.UITest.Queries
{
    /// <summary>
    /// Representation of a view elements position and size.
    /// </summary>
    public class AppRect : IRect
    {
        /// <summary>
        /// The width of the element.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The height of the element.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// The X coordinate of the top left corner of the element.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The Y coordinate of the top left corner of the element.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// The X coordinate of the center of the element.
        /// </summary>
        public float CenterX { get; set; }

        /// <summary>
        /// The Y coordinate of the center of the element.
        /// </summary>
        /// <value>The center y.</value>
        public float CenterY { get; set; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="AppRect"/>.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Width: {0}, Height: {1}, X: {2}, Y: {3}, CenterX: {4}, CenterY: {5}", Width, Height, X, Y, CenterX, CenterY);
        }
    }
}