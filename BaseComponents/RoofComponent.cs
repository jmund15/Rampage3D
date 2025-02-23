using Godot;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class RoofComponent : Node
{
    [Export] // TODO: IMPLEMENT
    private bool _setTopFloorAsRoof = false;
    [Export]
    public MeshInstance3D RoofMesh { get; private set; }
    private StandardMaterial3D _roofMat;
    private CompressedTexture2D _texture;
    public CompressedTexture2D Texture
    {
        get => _texture;
        set
        {
            if (value == _texture || value == null || _roofMat == null) { return; }
            _texture = value;
            SetTexture();
        }
    }
    private CompressedTexture2D _normalMap;
    public CompressedTexture2D NormalMap
    {
        get => _normalMap;
        set
        {
            if (value == _normalMap || _roofMat == null) { return; }
            if (value == null)
            {
                DisableNormalMap();
                return;
            }
            _normalMap = value;
            SetNormalMap();
        }
    }
    //Rel = relative to max height of building
    public Dictionary<Dir4, float> RoofRelHeightMap { get; private set; } 
		= new Dictionary<Dir4, float>();
    public float MaxRoofHeight { get; private set; } = float.MinValue;
	public override void _Ready()
	{
        //TODO: ADD EDITOR WARNING
        if (Engine.IsEditorHint() && RoofMesh == null)
        {
            return;
        }
		//TODO: MAKE IT AN EDITOR THING INSTEAD OF RUNNING EACH TIME THE GAME STARTS
		//_roofMesh.Conve
        if (RoofMesh == null)
        {
            GD.PrintErr($"ROOF ERROR || Building '{GetOwner().Name}' has no roof mesh!");
            return;
        }
        RoofMesh.MaterialOverride.ResourceLocalToScene = true;
        _roofMat = RoofMesh.MaterialOverride as StandardMaterial3D;
        _roofMat.ResourceLocalToScene = true;
        ArrayMesh arrayMesh = RoofMesh.Mesh as ArrayMesh;
        MeshDataTool mdt = new MeshDataTool();
		List<Vector3> meshVerts = new List<Vector3>();
        for (int i = 0; i < arrayMesh.GetSurfaceCount(); i++) 
		{
            mdt.CreateFromSurface(arrayMesh, 0);
			for (int j = 0; j < mdt.GetVertexCount(); j++)
			{
                //GD.Print("meshVert: ", mdt.GetVertex(j));
				meshVerts.Add(mdt.GetVertex(j));
			}
        }
        
        MaxRoofHeight = float.MinValue;

        // Initialize placeholders for extreme points and their max heights
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        float maxHeightMinX = float.MinValue;
        float maxHeightMaxX = float.MinValue;
        float maxHeightMinZ = float.MinValue;
        float maxHeightMaxZ = float.MinValue;

        // Iterate through the vertices to find extreme points and their max heights
        foreach (var vertex in meshVerts)
        {
            float x = vertex.X, y = vertex.Y, z = vertex.Z;

            if (y > MaxRoofHeight)
            {
                MaxRoofHeight = y;
            }

            // Check min x
            if (x < minX)
            {
                minX = x;
                maxHeightMinX = y;
            }
            else if (x == minX)
            {
                maxHeightMinX = Math.Max(maxHeightMinX, y);
            }

            // Check max x
            if (x > maxX)
            {
                maxX = x;
                maxHeightMaxX = y;
            }
            else if (x == maxX)
            {
                maxHeightMaxX = Math.Max(maxHeightMaxX, y);
            }

            // Check min z
            if (z < minZ)
            {
                minZ = z;
                maxHeightMinZ = y;
            }
            else if (z == minZ)
            {
                maxHeightMinZ = Math.Max(maxHeightMinZ, y);
            }

            // Check max z
            if (z > maxZ)
            {
                maxZ = z;
                maxHeightMaxZ = y;
            }
            else if (z == maxZ)
            {
                maxHeightMaxZ = Math.Max(maxHeightMaxZ, y);
            }
        }
        RoofRelHeightMap.Add(Dir4.Down, maxHeightMaxZ - MaxRoofHeight);
        RoofRelHeightMap.Add(Dir4.Right, maxHeightMaxX - MaxRoofHeight);
        RoofRelHeightMap.Add(Dir4.Left, maxHeightMinX - MaxRoofHeight);
        RoofRelHeightMap.Add(Dir4.Up, maxHeightMinZ - MaxRoofHeight);

        //GD.Print($"Calculated roof height map. " +
        //    $"\nUL: {RoofRelHeightMap[Dir4.UpLeft]}" +
        //    $"\nUR: {RoofRelHeightMap[Dir4.UpRight]}" +
        //    $"\nDL: {RoofRelHeightMap[Dir4.DownLeft]}" +
        //    $"\nDR: {RoofRelHeightMap[Dir4.DownRight]}");
    }
	public override void _Process(double delta)
	{
	}
    private void SetTexture()
    {
        _roofMat.ResourceLocalToScene = true;

        _roofMat.AlbedoTexture = Texture;
        _roofMat.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;

        _roofMat.AlbedoTexture.ResourceLocalToScene = true;
    }
    private void SetNormalMap()
    {
        _roofMat.ResourceLocalToScene = true;

        _roofMat.NormalEnabled = true;
        _roofMat.NormalTexture = NormalMap;

        _roofMat.NormalTexture.ResourceLocalToScene = true;
    }
    private void DisableNormalMap()
    {
        //MaterialOverride.ResourceLocalToScene = true;
        _roofMat.NormalEnabled = false;
    }
}
