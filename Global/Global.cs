using Godot;
using System;
using System.Diagnostics;

public partial class Global : Node
{
    public static readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());
    public const float MovementTransitionBufferTime = 0.1f;

    public override void _Ready()
	{
	}
	public override void _Process(double delta)
	{
	}
    public static float GetRndInRange(float min, float max)
    {
        var normF = Rnd.NextSingle();
        return (normF * (max - min) + min);
    }
    public static OrthogDirection GetRndEightDirection()
    {
        var rndEight = Rnd.Next(0, 4);
        return (OrthogDirection)rndEight;
    }
    public static void LogError(string message)
    {
        var lastFrame = new StackFrame(1);
        string className = lastFrame.GetMethod().DeclaringType.Name;
        string methodName = lastFrame.GetMethod().Name;

        GD.PrintErr($"{className}.{methodName} ERROR in || {message}");
    }
}
