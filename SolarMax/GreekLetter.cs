using System.Collections.Generic;
using System.Linq;

namespace SolarMax
{
    internal class GreekLetter
    {
        public string Name { get; private set; }
        public char Letter { get; private set; }
        public string Abbreviation { get; private set; }
        public string AbbreviationLower { get; private set; }
        public static List<GreekLetter> AllLetters { get; private set; }

        public static GreekLetter Empty = new();

        private GreekLetter()
        {
            this.Name = this.Abbreviation = string.Empty;
            this.Letter = '\0';
        }
        public GreekLetter(string Name, string Abbreviation, char Letter)
        {
            this.Name = Name;
            this.Abbreviation = Abbreviation;
            this.AbbreviationLower = Abbreviation.ToLower();
            this.Letter = Letter;
        }
        public static GreekLetter FromAbbreviation(string Abbreviation)
        {
            Abbreviation = Abbreviation.ToLower();

            if (Abbreviation == "alf")
                Abbreviation = "alp";
            else if (Abbreviation == "ksi")
                Abbreviation = "xi";

            return AllLetters.FirstOrDefault(l => l.AbbreviationLower == Abbreviation) ?? Empty;
        }
        static GreekLetter()
        {
            AllLetters =
            [
                new GreekLetter("Alpha", "Alp", 'α'),
                new GreekLetter("Beta", "Bet", 'β'),
                new GreekLetter("Gamma", "Gam", 'γ'),
                new GreekLetter("Delta", "Del", 'δ'),
                new GreekLetter("Epsilon", "Eps", 'ε'),
                new GreekLetter("Zeta", "Zet", 'ζ'),
                new GreekLetter("Eta", "Eta", 'η'),
                new GreekLetter("Theta", "The", 'θ'),
                new GreekLetter("Iota", "Iot", 'ι'),
                new GreekLetter("Kappa", "Kap", 'κ'),
                new GreekLetter("Lambda", "Lam", 'λ'),
                new GreekLetter("Mu", "Mu", 'μ'),
                new GreekLetter("Nu", "Nu", 'ν'),
                new GreekLetter("Xi", "Xi", 'ξ'),
                new GreekLetter("Omicron", "Omi", 'ο'),
                new GreekLetter("Pi", "Pi", 'π'),
                new GreekLetter("Rho", "Rho", 'ρ'),
                new GreekLetter("Sigma", "Sig", 'σ'),
                new GreekLetter("Tau", "Tau", 'τ'),
                new GreekLetter("Upsilon", "Ups", 'υ'),
                new GreekLetter("Phi", "Phi", 'φ'),
                new GreekLetter("Chi", "Chi", 'χ'),
                new GreekLetter("Psi", "Psi", 'ψ'),
                new GreekLetter("Omega", "Ome", 'ω'),
            ];
        }
        public override string ToString()
        {
            if (this.Letter != '\0')
                return this.Letter.ToString();
            else
                return string.Empty;
        }
    }
}