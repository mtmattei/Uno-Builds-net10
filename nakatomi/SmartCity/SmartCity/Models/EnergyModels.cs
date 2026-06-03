namespace SmartCity.Models;

/// <summary>Scale selector levels. Only <see cref="Building"/> is wired in this build.</summary>
public enum BuildingScale
{
    City,
    District,
    Street,
    Building,
}

/// <summary>One floor of the building twin. Floor 14 is the flagged optimization target.</summary>
public record FloorReading(int Floor, string Label, double CurrentKwh, double OptimizedKwh, bool IsFlagged);

/// <summary>A single bar in the energy-consumption chart (current vs after-optimization).</summary>
public record HourlyConsumption(string TimeLabel, double CurrentKwh, double OptimizedKwh);

/// <summary>Current vs optimized consumption for one rollup window. Reduction is computed, never stored (Decision 4).</summary>
public record Metric(double Current, double Optimized)
{
    public double ReductionPct => Current <= 0 ? 0 : (Current - Optimized) / Current;
}

/// <summary>Daily / Weekly / Monthly / Yearly KPI roll-ups.</summary>
public record ConsumptionRollup(Metric Daily, Metric Weekly, Metric Monthly, Metric Yearly);

/// <summary>The blue AI suggestion card content.</summary>
public record AiSuggestion(string Title, string Body, double ReductionPct, int TargetFloor);

/// <summary>One point on the monthly-savings trend line.</summary>
public record TrendPoint(string DateLabel, double Value);

/// <summary>Monthly savings roll-up: cost, energy, CO₂ avoided, plus the trend series.</summary>
public record SavingsSummary(double CostSaved, double EnergySaved, double Co2Avoided, TrendPoint[] Trend);
