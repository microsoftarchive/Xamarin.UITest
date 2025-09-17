namespace Xamarin.UITest.Repl.Console.Dummy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Console.Title = "Xamarin.UITest REPL (dummy)";
            System.Console.Clear();
            System.Console.WriteLine("This is not the real repl - this is just a placeholder.");
            System.Console.ReadLine();
        }
    }
}
