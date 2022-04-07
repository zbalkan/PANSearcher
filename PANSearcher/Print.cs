namespace PANSearcher
{
    public static class Print
    {
        public static PrintMode PrintMode { get; set; } = PrintMode.Output;


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
        }
    }

    public enum PrintMode
    {
        Output,
        Verbose,
        Quiet
    }
}
