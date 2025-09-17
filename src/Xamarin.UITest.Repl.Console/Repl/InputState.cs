namespace Xamarin.UITest.Repl.Repl
{
    /// <summary>
    /// Class for storing current console input state.
    /// </summary>
    public class InputState
    {
        /// <summary>
        /// <see cref="string"/> with currently entered code in console.
        /// </summary>
        public string code { private set; get; }

        /// <summary>
        /// <see cref="int"/> representing current index in <see cref="code"/> string.
        /// </summary>
        public int index { private set; get; }

        /// <summary>
        /// Initializes new <see cref="InputState"/> instance.
        /// </summary>
        /// <param name="code">Code string.</param>
        /// <param name="index">Index.</param>
        public InputState(string code, int index)
        {
            this.code = code;
            this.index = index;
        }
    }
}