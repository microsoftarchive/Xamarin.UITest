namespace Xamarin.UITest.Android
{
    /// <summary>
    /// Specifies the location of the drop in a drag and drop action.
    /// </summary>
    public enum DropLocation
	{
        /// <summary>
        /// Drop on top of the element matched by the query.
        /// </summary>
        OnTop,

        /// <summary>
        /// Drop above the element matched by the query.
        /// </summary>
        Above,

        /// <summary>
        /// Drop below the element matched by the query.
        /// </summary>
        Below,

        /// <summary>
        /// Drop to the left of the element matched by the query.
        /// </summary>
        Left,

        /// <summary>
        /// Drop to the right of the element matched by the query.
        /// </summary>
        Right
	}
}
