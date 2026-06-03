namespace SmartCity.Services;

/// <summary>
/// Builds the tower as N stacked extruded boxes (one per floor), centred on the origin and stacked
/// along +Y. A slight upward taper gives the slab tower a touch of the reference silhouette.
/// </summary>
public class BoxStackGeometryProvider : IBuildingGeometryProvider
{
    // Model-space footprint + per-floor height. Units are arbitrary; the view scales to fit.
    private const float BaseHalfWidth = 1.0f;
    private const float BaseHalfDepth = 0.78f;
    private const float FloorHeight = 0.30f;
    private const float TaperPerFloor = 0.006f; // shrink footprint slightly toward the top

    public IReadOnlyList<FloorBox> BuildTower(int floorCount, int flaggedFloor)
    {
        var floors = new List<FloorBox>(floorCount);
        var totalHeight = floorCount * FloorHeight;
        var yBottomStart = -totalHeight / 2f;

        for (var i = 0; i < floorCount; i++)
        {
            var y0 = yBottomStart + i * FloorHeight;
            var y1 = y0 + FloorHeight * 0.92f; // small gap between slabs reads as floor lines

            var hwLow = BaseHalfWidth - TaperPerFloor * i;
            var hdLow = BaseHalfDepth - TaperPerFloor * i;
            var hwHigh = BaseHalfWidth - TaperPerFloor * (i + 1);
            var hdHigh = BaseHalfDepth - TaperPerFloor * (i + 1);

            var corners = new[]
            {
                new Vec3(-hwLow,  y0, -hdLow),  // 0
                new Vec3( hwLow,  y0, -hdLow),  // 1
                new Vec3( hwLow,  y0,  hdLow),  // 2
                new Vec3(-hwLow,  y0,  hdLow),  // 3
                new Vec3(-hwHigh, y1, -hdHigh), // 4
                new Vec3( hwHigh, y1, -hdHigh), // 5
                new Vec3( hwHigh, y1,  hdHigh), // 6
                new Vec3(-hwHigh, y1,  hdHigh), // 7
            };

            var floorNumber = i + 1;
            floors.Add(new FloorBox(floorNumber, $"{floorNumber}F", corners, floorNumber == flaggedFloor));
        }

        return floors;
    }
}
