namespace ChefsTest6.Models;

public class NutritionData
{
    public double Protein { get; set; }
    public double ProteinBase { get; set; }
    public double Carbs { get; set; }
    public double CarbsBase { get; set; }
    public double Fat { get; set; }
    public double FatBase { get; set; }

    public double ProteinRatio => ProteinBase <= 0 ? 0 : Math.Min(1, Protein / ProteinBase);
    public double CarbsRatio => CarbsBase <= 0 ? 0 : Math.Min(1, Carbs / CarbsBase);
    public double FatRatio => FatBase <= 0 ? 0 : Math.Min(1, Fat / FatBase);

    public string ProteinLabel => $"{Protein:0} / {ProteinBase:0}g";
    public string CarbsLabel => $"{Carbs:0} / {CarbsBase:0}g";
    public string FatLabel => $"{Fat:0} / {FatBase:0}g";
}
