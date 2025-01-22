using Godot;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class RoofComponent : Node
{
	[Export]
	private MeshInstance3D _roofMesh;
	//Rel = relative, meaning not global, only accounting for roof mesh
	public Dictionary<OrthogDirection, float> RoofRelHeightMap { get; private set; } 
		= new Dictionary<OrthogDirection, float>();
	public override void _Ready()
	{
        //TODO: ADD EDITOR WARNING
        if (Engine.IsEditorHint() && _roofMesh == null)
        {
            return;
        }
		//TODO: MAKE IT AN EDITOR THING INSTEAD OF RUNNING EACH TIME THE GAME STARTS
		//_roofMesh.Conve
        ArrayMesh arrayMesh = _roofMesh.Mesh as ArrayMesh;
        MeshDataTool mdt = new MeshDataTool();
		List<Vector3> meshVerts = new List<Vector3>();
        for (int i = 0; i < arrayMesh.GetSurfaceCount(); i++) 
		{
            mdt.CreateFromSurface(arrayMesh, 0);
			for (int j = 0; j < mdt.GetVertexCount(); j++)
			{
				meshVerts.Add(mdt.GetVertex(j));
			}
        }

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
        RoofRelHeightMap.Add(OrthogDirection.DownLeft, maxZ);
        RoofRelHeightMap.Add(OrthogDirection.DownRight, maxX);
        RoofRelHeightMap.Add(OrthogDirection.UpLeft, minX);
        RoofRelHeightMap.Add(OrthogDirection.UpRight, minZ);

        GD.Print("Calculated roof height map: ", RoofRelHeightMap);
    }
	public override void _Process(double delta)
	{
	}
}
