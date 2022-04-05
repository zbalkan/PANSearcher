using Utility.CommandLine;

namespace PANSearcher
{
    public static class Program
    {
        /// <summary>
        ///     Gets or sets the list of Card numbers.
        /// </summary>
        [Argument('c', "CardNumbers", "Type in cardnumbers delimited by comma.")]
        private static string[]? CardNumbers { get; set; }

        /// <summary>
        ///     Gets or sets the FilePath.
        /// </summary>
        [Argument('f', "FilePath", "Path to the file to be checked against PAN numbers.")]
        private static string? FilePath { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('h', "Help", "Displays help text and exits.")]
        private static bool ShowHelpText { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('u', "Unmask", "Displays PAN numbers unmasked. Ignored when used with 'u' flag. (Default: false)")]
        private static bool Unmask { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('t', "Truncate", "Displays PAN numbers truncated.")]
        private static bool Truncate { get; set; }

        // TODO: Run like PANHunt.
        public static void Main(string[] args)
        {
            // enable ctrl+c
            Console.CancelKeyPress += (o, e) => Environment.Exit(1);

            Arguments.Populate();

            if (ShowHelpText)
            {
                ShowHelp();
                return;
            }

            if (!string.IsNullOrEmpty(FilePath) && CardNumbers != null && CardNumbers.Length > 0)
            {
                Console.WriteLine("You cannot provide both file path and card numbers at the same time.");
                Environment.Exit(1);
            }

            // Interactive session
            if (string.IsNullOrEmpty(FilePath) && (CardNumbers == null || CardNumbers.Length == 0))
            {
                while (true)
                {
                    Console.Write("Enter Card Number or type Q or q to exit : ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine("Invalid input.");
                        continue;
                    }
                    if (input.ToUpper().Equals("Q", StringComparison.Ordinal))
                    {
                        break;
                    }
                    var cardType = PAN.GetCardType(input);
                    if (cardType != CardType.Invalid && Luhn.Validate(input))
                    {
                        Console.WriteLine($"Valid {Enum.GetName(typeof(CardType), cardType)} Card.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid Card");
                    }

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    var lines = File.ReadLines(FilePath);
                    foreach (var line in lines)
                    {
                        var found = PAN.ParseLine(line);
                        if (found.Count == 0)
                        {
                            continue;
                        }

                        foreach (var item in found)
                        {
                            var cardType = PAN.GetCardType(item);
                            if (cardType == CardType.Invalid)
                            {
                                continue;
                            }

                            if (cardType != CardType.Invalid && Luhn.Validate(item))
                            {
                                string pan;
                                if (Truncate) { pan = PAN.Truncate(item); }
                                else if(Unmask) { pan = item; }
                                else { pan = PAN.Mask(item); }
                                Console.WriteLine($"FOUND PAN: {pan} - {Enum.GetName(typeof(CardType), cardType)} (Path: {FilePath})");
                            }
                            else
                            {
                                // Console.WriteLine("Invalid Card");
                            }
                        }
                    }
                }
                else
                {
                    if (CardNumbers != null) // I know that it is not null but I needed to silence the warning
                    {
                        foreach (var item in CardNumbers)
                        {
                            var cardType = PAN.GetCardType(item);
                            if (cardType == CardType.Invalid)
                            {
                                continue;
                            }

                            if (cardType != CardType.Invalid && Luhn.Validate(item))
                            {
                                Console.WriteLine($"{item} - Valid {Enum.GetName(typeof(CardType), cardType)} Card.");
                            }
                            else
                            {
                                Console.WriteLine($"{item} - Invalid Card");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Returns a "pretty" string representation of the provided Type; specifically, corrects the naming of generic Types
        ///     and appends the type parameters for the type to the name as it appears in the code editor.
        /// </summary>
        /// <param name="type">The type for which the colloquial name should be created.</param>
        /// <returns>A "pretty" string representation of the provided Type.</returns>
        public static string ToColloquialString(this Type type) => (!type.IsGenericType ? type.Name : type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(a => a.ToColloquialString())) + ">");


        /// <summary>
        ///     Show help for arguments.
        /// </summary>
        private static void ShowHelp()
        {
            var helpAttributes = Arguments.GetArgumentInfo(typeof(Program));

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var maxLen = helpAttributes.Select(a => a.Property.PropertyType.ToColloquialString()).OrderByDescending(s => s.Length).FirstOrDefault().Length;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            Console.WriteLine($"Short\tLong\t\t{"Type".PadRight(maxLen)}\tFunction");
            Console.WriteLine($"-----\t----\t\t{"----".PadRight(maxLen)}\t--------");

            foreach (var item in helpAttributes)
            {
                var result = item.ShortName + "\t" + item.LongName + "\t" + item.Property.PropertyType.ToColloquialString().PadRight(maxLen) + "\t" + item.HelpText;
                Console.WriteLine(result);
            }
        }
    }
}