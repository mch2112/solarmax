namespace SolarMax;

internal sealed class StarDesignation
{
    public GreekLetter GreekLetter;
    public string BayerDesignator { get; private set; }
    public string BayerDesignatorOriginal { get; private set; }
    public string ConstellationAbbreviation { get; private set; }
    public string ConstellationGenitive { get; private set; }
    public int BayerIndex { get; private set; }
    public int FlamsteedNumber { get; private set; }
    public string SortKey { get; private set; }
    public bool IsEmpty { get; private set; }
    public bool HasBayerOrFlamsteedDesignation { get; private set; }
    public string PrimaryDesignation { get; private set; }
    public string SecondaryDesignation { get; private set; }
    public string ShortDesignation { get; private set; }

    public StarDesignation(string ConstellationAbbreviation, string BayerDesignator, int BayerIndex, int FlamsteedNumber) : this()
    {
        if (Constellation.GenitiveNames.TryGetValue(ConstellationAbbreviation, out var gn))
        {
            this.ConstellationGenitive = gn;
            this.ConstellationAbbreviation = ConstellationAbbreviation;

            this.GreekLetter = GreekLetter.FromAbbreviation(BayerDesignator);
            this.BayerDesignatorOriginal = BayerDesignator;
            this.BayerDesignator = this.GreekLetter == GreekLetter.Empty ? BayerDesignator : this.GreekLetter.ToString();
            this.BayerIndex = BayerIndex;
            this.FlamsteedNumber = FlamsteedNumber;

            this.IsEmpty = false;

            if (!string.IsNullOrWhiteSpace(BayerDesignator))
            {
                if (this.BayerIndex > 0)
                {
                    this.PrimaryDesignation = $"{this.BayerDesignator}{this.BayerIndex.ToSuperscript()} {this.ConstellationGenitive}";
                    this.ShortDesignation = $"{this.BayerDesignator}{this.BayerIndex.ToSuperscript()}{this.ConstellationAbbreviation}";
                }
                else
                {
                    this.PrimaryDesignation = $"{this.BayerDesignator} {this.ConstellationGenitive}";
                    this.ShortDesignation = $"{this.BayerDesignator}{this.ConstellationAbbreviation}";
                }
                if (this.FlamsteedNumber > 0)
                {
                    this.SecondaryDesignation = $"{this.FlamsteedNumber} {this.ConstellationGenitive}";
                }
                this.HasBayerOrFlamsteedDesignation = true;
            }
            else if (FlamsteedNumber > 0)
            {
                this.PrimaryDesignation = $"{this.FlamsteedNumber} {this.ConstellationGenitive}";
                this.ShortDesignation = $"{this.FlamsteedNumber}{this.ConstellationAbbreviation}";
                this.SecondaryDesignation = string.Empty;
                this.HasBayerOrFlamsteedDesignation = true;
            }
            else
            {
                this.PrimaryDesignation = ConstellationGenitive;
                this.ShortDesignation = ConstellationAbbreviation;
                this.SecondaryDesignation = string.Empty;
            }
        }
        if (this.GreekLetter != null && this.GreekLetter.Letter != '\0')
            this.SortKey = ConstellationAbbreviation + "AA" + this.GreekLetter.ToString() + FlamsteedNumber.ToString("0000");
        else if (this.BayerDesignator.Length > 0 && this.BayerDesignator[0] >= 'a')
            this.SortKey = ConstellationAbbreviation + "AB" + this.BayerDesignator + FlamsteedNumber.ToString("0000");
        else if (this.BayerDesignator.Length > 0)
            this.SortKey = ConstellationAbbreviation + "AC" + this.BayerDesignator + FlamsteedNumber.ToString("0000");
        else if (FlamsteedNumber > 0)
            this.SortKey = ConstellationAbbreviation + "B" + FlamsteedNumber.ToString("0000");
        else
            this.SortKey = ConstellationAbbreviation + "C";

    }
    public StarDesignation()
    {
        this.BayerDesignator = this.ConstellationAbbreviation = string.Empty;
        this.BayerIndex = 0;
        this.FlamsteedNumber = 0;
        this.PrimaryDesignation = string.Empty;
        this.SecondaryDesignation = string.Empty;
        this.IsEmpty = true;
        this.HasBayerOrFlamsteedDesignation = false;
        this.SortKey = "Z";
    }
}
