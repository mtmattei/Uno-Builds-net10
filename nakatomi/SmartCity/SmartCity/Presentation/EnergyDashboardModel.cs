namespace SmartCity.Presentation;

/// <summary>
/// MVUX surface for the Predictive AI → Building dashboard. Slice 1 holds the scale selector
/// only; the energy feeds/states/commands (hourly, rollup, suggestion, savings, floors) land in
/// slice 2 once the static layout is verified. Data comes from <see cref="IEnergyService"/>.
/// </summary>
public partial record EnergyDashboardModel(IEnergyService Energy)
{
    // Building is the only wired scale in this build; the selector is present but inert otherwise.
    public IState<BuildingScale> Scale => State<BuildingScale>.Value(this, () => BuildingScale.Building);
}
