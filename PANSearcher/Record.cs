namespace PANSearcher
{
    public class Record
    {
        public string PossiblePAN { get; set; }

        public CardType CardType { get; set; }

        public string LineText { get; set; }

        public decimal LineNumber { get; set; }

        public override string ToString() => $"{Enum.GetName(CardType)}:{PAN.Format(PossiblePAN, Settings.Instance.PANDisplayMode)} [Line: {LineNumber}]";
    }
}
