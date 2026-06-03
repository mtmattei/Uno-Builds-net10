namespace SmartCity.Models;

/// <summary>A point in model space (right-handed: +Y up, tower stacked along Y).</summary>
public readonly record struct Vec3(float X, float Y, float Z);

/// <summary>
/// One floor slab as an extruded box: 8 corners, ordered bottom face 0-3 then top face 4-7
/// (each face CCW from -X/-Z). <see cref="Edges"/> indexes pairs into <see cref="Corners"/>.
/// </summary>
public record FloorBox(int Floor, string Label, Vec3[] Corners, bool IsFlagged)
{
    /// <summary>The 12 box edges as corner-index pairs (bottom ring, top ring, verticals).</summary>
    public static readonly (int A, int B)[] Edges =
    {
        (0,1),(1,2),(2,3),(3,0),   // bottom
        (4,5),(5,6),(6,7),(7,4),   // top
        (0,4),(1,5),(2,6),(3,7),   // verticals
    };
}
