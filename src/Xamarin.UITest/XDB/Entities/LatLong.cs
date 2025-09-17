namespace Xamarin.UITest.XDB.Entities
{
    struct LatLong
    {
        public readonly double Latitude;
        public readonly double Longitude;

        public LatLong(double lattitude, double longitude)
        {
            Latitude = lattitude;
            Longitude = longitude;
        }
    }
}