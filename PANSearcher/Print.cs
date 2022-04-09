using System.Diagnostics;

namespace PANSearcher
{
    public static class Print
    {
        private static PrintMode PrintMode => Settings.Instance.PrintMode;


        public static void Output(string message)
        {
            if (PrintMode == PrintMode.Output || PrintMode == PrintMode.Verbose)
            {
                Console.WriteLine(message);
            }
        }

        public static void Verbose(string message)
        {
            if (PrintMode == PrintMode.Verbose)
            {
                Console.WriteLine(message);
            }
            else
            {
                Debug.WriteLine(message);
            }
        }
    }
}
