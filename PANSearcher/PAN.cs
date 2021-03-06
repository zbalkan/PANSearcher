using System.Text.RegularExpressions;

namespace PANSearcher
{
    public static class PAN
    {
        private static readonly Regex mastercard = new(@"(?:\D|^)(5[1-5][0-9]{2}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);
        private static readonly Regex visa = new(@"(?:\D|^)(4[0-9]{3}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);
        private static readonly Regex amex = new(@"(?:\D|^)((?:34|37)[0-9]{2}(?:\ |\-|)[0-9]{6}(?:\ |\-|)[0-9]{5})(?:\D|$)", RegexOptions.Compiled);

        public static bool Validate(string cardNumber, out CardType cardType)
        {
            cardType = GetCardType(cardNumber);
            return cardType != CardType.Invalid && Luhn.Validate(cardNumber);
        }

        private static CardType GetCardType(string cardNumber)
        {
            if (mastercard.IsMatch(cardNumber))
            {
                return CardType.Mastercard;
            }

            if (visa.IsMatch(cardNumber))
            {
                return CardType.Visa;
            }

            if (amex.IsMatch(cardNumber))
            {
                return CardType.Amex;
            }
            return CardType.Invalid;
        }

        public static IReadOnlyList<string> ParseLine(string line)
        {
            var list = new List<string>();
            var matches1 = mastercard.Matches(line);
            var matches2 = visa.Matches(line);
            var matches3 = amex.Matches(line);

            list.AddRange(matches1.Select(item => GetNumbers(item.Value)).ToList());
            list.AddRange(matches2.Select(item => GetNumbers(item.Value)).ToList());
            list.AddRange(matches3.Select(item => GetNumbers(item.Value)).ToList());

            return list.AsReadOnly();
        }

        public static string Format(string PANNumber, PANDisplayMode displayMode) => displayMode switch
        {
            PANDisplayMode.Unmasked => PANNumber,
            PANDisplayMode.Truncated => Truncate(PANNumber),
            _ => Mask(PANNumber),
        };

        /// <summary>
        ///     By default all PANs are masked
        /// </summary>
        /// <para>
        ///     PCI-DSS v3.2 - Req.3.3: Mask PAN when displayed (the first six and last four digits 
        ///     are the maximum number of digits to be displayed), such that only personnel with a legitimate
        ///     business need can see more than the first six/last four digits of the PAN.
        /// </para>
        /// <see href="https://www.pcisecuritystandards.org/"/>
        /// <param name="cardNumber">PAN</param>
        /// <returns></returns>
        private static string Mask(string cardNumber)
        {
            var first = cardNumber.Substring(0, 6);
            var middle = cardNumber.Substring(6, cardNumber.Length - 10);
            var last = cardNumber.Substring(cardNumber.Length - 5, 4);

            var maskedArray = new char[middle.Length];

            for (var i = 0; i < middle.Length; i++)
            {
                if (char.IsDigit(middle[i]))
                {
                    maskedArray[i] = '*';
                }
                else
                {
                    maskedArray[i] = middle[i];
                }
            }

            return string.Concat(first, new string(maskedArray), last);
        }

        private static string Truncate(string cardNumber)
        {
            var first = cardNumber.Substring(0, 6);
            var last = cardNumber.Substring(cardNumber.Length - 5, 4);

            return string.Concat(first, last);
        }

        private static string GetNumbers(string input) => new(input.Where(c => char.IsDigit(c)).ToArray());
    }
}
