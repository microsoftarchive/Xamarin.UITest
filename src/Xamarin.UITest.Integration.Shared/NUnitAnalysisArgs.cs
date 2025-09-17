using DocoptNet;

namespace Xamarin.UITest.Integration.Shared
{
    public class NUnitAnalysisArgs
    {
        public const string Usage = @"Usage: my_program <assembly-file> [ --include=<nunit-category> | --exclude=<nunit-category> | --fixture=<fixture> ] ...";
        public NUnitAnalysisArgs(string[] args)
        {
            var options = new Docopt().Apply(Usage, args);

            AssemblyFile = options["<assembly-file>"].Value.ToString();
            IncludedCategories = options["--include"].ToStringArray();
            ExcludedCategories = options["--exclude"].ToStringArray();
            Fixtures = options["--fixture"].ToStringArray();
        }

        public string[] Fixtures { get; private set; }

        public string[] ExcludedCategories { get; private set; }

        public string[] IncludedCategories { get; private set; }
        public string AssemblyFile { get; private set; }
    }
}