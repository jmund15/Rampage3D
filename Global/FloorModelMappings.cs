using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum FloorType
{
    Base,
    Middle,
    Roof
}

public enum FloorOverlay
{
    Snowy
}

public enum FloorAesthetic
{
    Rural,
    Urban,
    Army
}

public enum FloorMaterial
{
    Wood,
    Brick,
    SheetMetal,
}
public record FloorIdentifier(FloorAesthetic Aesthetic, FloorType Type, FloorOverlay Overlay);
public static class FloorModelMappings
{
    public static Dictionary<FloorMaterial, string> FloorMaterialAnimationMap { get; private set; } = new Dictionary<FloorMaterial, string>()
    {
        { FloorMaterial.Wood, "wood" },
        { FloorMaterial.Brick, "brick" },
        { FloorMaterial.SheetMetal, "sheetmetal" },
    };
}

