using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class GreekLetter
    {
        public string Name { get; private set; }
        public char Letter { get; private set; }
        public string Abbreviation { get; private set; }
        public string AbbreviationLower { get; private set; }
        public static List<GreekLetter> AllLetters { get; private set; }

        public static GreekLetter Empty = new GreekLetter();

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
            AllLetters = new List<GreekLetter>(24);

            AllLetters.Add(new GreekLetter("Alpha", "Alp", 'α'));
            AllLetters.Add(new GreekLetter("Beta", "Bet", 'β'));
            AllLetters.Add(new GreekLetter("Gamma", "Gam", 'γ'));
            AllLetters.Add(new GreekLetter("Delta", "Del", 'δ'));
            AllLetters.Add(new GreekLetter("Epsilon", "Eps", 'ε'));
            AllLetters.Add(new GreekLetter("Zeta", "Zet", 'ζ'));
            AllLetters.Add(new GreekLetter("Eta", "Eta", 'η'));
            AllLetters.Add(new GreekLetter("Theta", "The", 'θ'));
            AllLetters.Add(new GreekLetter("Iota", "Iot", 'ι'));
            AllLetters.Add(new GreekLetter("Kappa", "Kap", 'κ'));
            AllLetters.Add(new GreekLetter("Lambda", "Lam", 'λ'));
            AllLetters.Add(new GreekLetter("Mu", "Mu", 'μ'));
            AllLetters.Add(new GreekLetter("Nu", "Nu", 'ν'));
            AllLetters.Add(new GreekLetter("Xi", "Xi", 'ξ'));
            AllLetters.Add(new GreekLetter("Omicron", "Omi", 'ο'));
            AllLetters.Add(new GreekLetter("Pi", "Pi", 'π'));
            AllLetters.Add(new GreekLetter("Rho", "Rho", 'ρ'));
            AllLetters.Add(new GreekLetter("Sigma", "Sig", 'σ'));
            AllLetters.Add(new GreekLetter("Tau", "Tau", 'τ'));
            AllLetters.Add(new GreekLetter("Upsilon", "Ups", 'υ'));
            AllLetters.Add(new GreekLetter("Phi", "Phi", 'φ'));
            AllLetters.Add(new GreekLetter("Chi", "Chi", 'χ'));
            AllLetters.Add(new GreekLetter("Psi", "Psi", 'ψ'));
            AllLetters.Add(new GreekLetter("Omega", "Ome", 'ω'));
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