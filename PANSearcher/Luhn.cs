namespace PANSearcher
{
    public static class Luhn
    {
        /// <summary>
        /// Validate number against Luhn Algorithm
        /// </summary>
        /// <param name="cardnumber">Card number</param>
        /// <returns>bool</returns>
        public static bool Validate(string cardnumber)
        {
            if (string.IsNullOrEmpty(cardnumber))
            {
                return false;
            }

            cardnumber = cardnumber.Replace("-", "");

            var sum = 0;
            var alternate = false;
            var nx = cardnumber.ToArray();

            for (var i = cardnumber.Length - 1; i >= 0; i--)
            {
                var n = int.Parse(nx[i].ToString());

                if (alternate)
                {
                    n *= 2;

                    if (n > 9)
                    {
                        n = (n % 10) + 1;
                    }
                }
                sum += n;
                alternate = !alternate;
            }
            return (sum % 10 == 0);
        }
    }
}
