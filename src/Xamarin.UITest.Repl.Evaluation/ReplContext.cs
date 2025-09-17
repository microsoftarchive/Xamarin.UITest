namespace Xamarin.UITest.Repl.Evaluation
{
    public class ReplContext
    {
        public string[] Usings { get; set; }
        public string[] Vars { get; set; }

        public ReplContext(string[] usings, string[] vars)
        {
            Usings = usings;
            Vars = vars;
        }
    }
}