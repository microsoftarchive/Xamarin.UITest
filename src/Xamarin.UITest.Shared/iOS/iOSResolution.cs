namespace Xamarin.UITest.Shared.iOS
{
    public class iOSResolution
	{
        readonly int _height;
        readonly int _width;

        public iOSResolution(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public int Height
        {
            get { return _height; }
        }

        public int Width
        {
            get { return _width; }
        }
	}
}
