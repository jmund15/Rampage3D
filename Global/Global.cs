﻿using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Global : Node
{
    public const string BUILDING_GROUP_NAME = "Building";
    public const string VEHICLE_GROUP_NAME = "Vehicle";
    public const string OCCUPIABLE_VEHICLE_GROUP_NAME = "OccupiableVehicle";
    public static City CurrentCity { get; private set; }
    public static readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());
    public const float MovementTransitionBufferTime = 0.1f;
    public const float MINIMUM_PIXEL_SIZE = 0.01f;
    public const float CHANGE_DIR_VEL_REQ = 0.01f;
    public const string NAV_OBSTACLE_GROUP_NAME = "FullNav";
    public override void _Ready()
	{
        SetCurrentCity();
    }
	public override void _Process(double delta)
	{
	}
    private void SetCurrentCity()
    {
        CurrentCity = GetTree().CurrentScene as City;
        GD.Print($"Set current city to {CurrentCity.Name}!");
    }
    public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
    }
    public static float GetRndInRange(float min, float max)
    {
        var normF = Rnd.NextSingle();
        return (normF * (max - min) + min);
    }
    public static Dir4 GetRndEightDirection()
    {
        var rndEight = Rnd.Next(0, 4);
        return (Dir4)rndEight;
    }
    public static Vector2 GetRndVector2()
    {
        var x = GetRndInRange(-1.0f, 1.0f);
        var y = GetRndInRange(-1.0f, 1.0f);
        return (new Vector2(x, y)).Normalized();
    }
    public static Vector3 GetRndVector3()
    {
        var x = GetRndInRange(-1.0f, 1.0f);
        var y = GetRndInRange(-1.0f, 1.0f);
        var z = GetRndInRange(-1.0f, 1.0f);
        return (new Vector3(x, y, z)).Normalized();
    }
    public static Vector3 GetRndVector3PosY()
    {
        var x = GetRndInRange(-1.0f, 1.0f);
        var y = GetRndInRange(0f, 1.0f);
        var z = GetRndInRange(-1.0f, 1.0f);
        return (new Vector3(x, y, z)).Normalized();
    }
    public static Vector3 GetRndVector3ZeroY()
    {
        var rnd2 = GetRndVector2();
        return (new Vector3(rnd2.X, 0f, rnd2.Y)).Normalized();
    }
    public static Vector3 QuadraticBezier3D(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 q0 = p0.Lerp(p1, t);
        Vector3 q1 = p1.Lerp(p2, t);
        Vector3 r = q0.Lerp(q1, t);
        return r;
    }
    public static IEnumerable<T> GetEnumValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
    public static void LogError(string message)
    {
        var lastFrame = new StackFrame(1);
        string className = lastFrame.GetMethod().DeclaringType.Name;
        string methodName = lastFrame.GetMethod().Name;

        GD.PrintErr($"{className}.{methodName} ERROR in || {message}");
    }
}
