namespace Xamarin.UITest.iOS.ApplicationSigning.Entities
{
    internal class CodesignIdentity
    {
        public readonly string Name;
        public readonly string SHASum;
        public bool IsDeveloperIdentity => Name.Contains(value: "iPhone Developer") || Name.Contains("Apple Development");
        public CodesignIdentity(string name, string shaSum)
        {
            Name = name;
            SHASum = shaSum;
        }
    }
}

