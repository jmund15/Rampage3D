using Godot;
using Godot.Collections;
using System;
using System.IO;

public static partial class NodeExts
{
    #region VECTOR_EXTENSIONS
    public static Vector2 GetVector2(this OrthogDirection orthogDir)
    {
        switch (orthogDir)
        {
            case OrthogDirection.UpLeft:
                return new Vector2(-1f, 0f);
            case OrthogDirection.UpRight:
                return new Vector2(0f, -1f);
            case OrthogDirection.DownLeft:
                return new Vector2(0f, 1f);
            case OrthogDirection.DownRight:
                return new Vector2(1f, 0f);
            default:
                throw new Exception($"ORTHOG DIR: {orthogDir} GAVE NO VECTOR??");
        }
    }
    public static Vector3 GetVector3(this OrthogDirection orthogDir)
    {
        var vec2 = GetVector2(orthogDir);
        return new Vector3(vec2.X, 0f, vec2.Y);
    }
    private static OrthogDirection GetDirectionFromXY(float x, float y)
    {
        if (x < 0 && y >= 0)
        {
            // IMPORTANT: Straight Left = UpLeft
            return OrthogDirection.UpLeft;
        }
        else if (x >= 0 && y > 0)
        {
            // IMPORTANT: Straight Up = UpRight
            return OrthogDirection.UpRight;
        }
        else if (x <= 0 && y < 0)
        {
            // IMPORTANT: Straight Down = DownLeft
            return OrthogDirection.DownLeft;
        }
        else if (x > 0 && y <= 0)
        {
            // IMPORTANT: Straight Right = DownRight
            return OrthogDirection.DownRight;
        }
        else { throw new Exception($"VALUES: ({x}, {y}) GAVE NO ORTHOG DIRECTION??"); }
    }
    public static OrthogDirection GetOrthogDirection(this Vector2 dir)
    {
        return GetDirectionFromXY(dir.X, dir.Y);
    }
    public static OrthogDirection GetOrthogDirection(this Vector3 dir)
    {
        return GetDirectionFromXY(dir.X, dir.Z);
    }


    public static Vector3 ClampIsometric(this Vector3 direction)
    {
        //var rotDir = (new Vector2(direction.X, direction.Z).Rotated(Mathf.Pi / 2));
        if (direction.X < 0 && direction.Z <= 0)
        {
            // IMPORTANT: Straight Left = UpLeft
            return new Vector3(-1f, direction.Y, 0f);
        }
        else if (direction.X >= 0 && direction.Z < 0)
        {
            // IMPORTANT: Straight Up = UpRight
            return new Vector3(0f, direction.Y, -1f);
        }
        else if (direction.X <= 0 && direction.Z > 0)
        {
            // IMPORTANT: Straight Down = DownLeft
            return new Vector3(0f, direction.Y, 1f);
        }
        else if (direction.X > 0 && direction.Z >= 0)
        {
            // IMPORTANT: Straight Right = DownRight
            return new Vector3(1f, direction.Y, 0f);
        }
        //Should be impossible
        else { throw new Exception("VECTOR GAVE NO ORTHOG DIRECTION??"); }
    }
    public static Vector2 ClampIsometric(this Vector2 direction)
    {
        var rotDir = direction.Rotated(-Mathf.Pi / 2);
        if (rotDir.X < 0 && rotDir.Y <= 0)
        {
            // IMPORTANT: Straight Left = UpLeft
            return new Vector2(-1f, 0f);
        }
        else if (rotDir.X >= 0 && rotDir.Y < 0)
        {
            // IMPORTANT: Straight Up = UpRight
            return new Vector2(0f, -1f);
        }
        else if (rotDir.X <= 0 && rotDir.Y > 0)
        {
            // IMPORTANT: Straight Down = DownLeft
            return new Vector2(0f, 1f);
        }
        else if (rotDir.X > 0 && rotDir.Y >= 0)
        {
            // IMPORTANT: Straight Right = DownRight
            return new Vector2(1f, 0f);
        }
        //Should be impossible
        else { throw new Exception("VECTOR GAVE NO ORTHOG DIRECTION??"); }


        //if (direction.X < 0)
        //{
        //    if (direction.X / (direction.Y + float.Epsilon) >= 2)
        //    {
        //        return new Vector2(-1f, 0f);
        //        return OrthogDirection.UpLeft;
        //    }
        //    else 
        //}
        
        //if (direction.X < 0)
        //{
        //    if ()
        //    {
        //        case float n when (Math.Abs(n) >= 2):
                    
        //        case float n when (Math.Abs(n) > 0.5):
        //            if (direction.Y >= 0)
        //            {
        //                return EightDirection.DOWNLEFT;
        //            }
        //            else
        //            {
        //                return EightDirection.UPLEFT;
        //            }
        //        default:
        //            if (direction.Y >= 0)
        //            {
        //                return EightDirection.DOWN;
        //            }
        //            else
        //            {
        //                return EightDirection.UP;
        //            }
        //    }
        //}
        //else
        //{
        //    switch (direction.X / direction.Y + float.Epsilon)
        //    {
        //        case float n when (Math.Abs(n) > 2):
        //            return EightDirection.RIGHT;
        //        case float n when (Math.Abs(n) > 0.5):
        //            if (direction.Y >= 0)
        //            {
        //                return EightDirection.DOWNRIGHT;
        //            }
        //            else
        //            {
        //                return EightDirection.UPRIGHT;
        //            }
        //        default:
        //            if (direction.Y >= 0)
        //            {
        //                return EightDirection.DOWN;
        //            }
        //            else
        //            {
        //                return EightDirection.UP;
        //            }
        //    }
        //}
    }
    public static Vector3 ClampInputToIsometric(this Vector2 inputDir)
    {
        // INPUT DIRECTION MEANS UP Y IS NEGATIVE
        if (inputDir.X < 0 && inputDir.Y <= 0)
        {
            // IMPORTANT: Straight Left = UpLeft
            return new Vector3(-1f, 0f, 0f);
        }
        else if (inputDir.X >= 0 && inputDir.Y < 0)
        {
            // IMPORTANT: Straight Up = UpRight
            return new Vector3(0f, 0f, -1f);
        }
        else if (inputDir.X <= 0 && inputDir.Y > 0)
        {
            // IMPORTANT: Straight Down = DownLeft
            return new Vector3(0f, 0f, 1f);
        }
        else if (inputDir.X > 0 && inputDir.Y >= 0)
        {
            // IMPORTANT: Straight Right = DownRight
            return new Vector3(1f, 0f, 0f);
        }
        //Should be impossible
        else { throw new Exception("VECTOR GAVE NO ORTHOG DIRECTION??"); }
    }
    public static Vector3 GetWeightedGravity(this CharacterBody3D body)
    {
        const float WEIGHT_PERCENTAGE = 0.06f;
        Vector3 weightedGrav;
        if (body.Velocity.Y < 0)
        {
            weightedGrav = body.GetGravity() - (body.Velocity * body.GetGravity() * WEIGHT_PERCENTAGE);
        }
        else
        {
            weightedGrav = body.GetGravity();
        }
        //GD.Print("weighted grav: ", weightedGrav);
        return weightedGrav;
    }
    #endregion
    #region VALIDATION_EXTENSIONS

    public static bool IsValid<T>(this T node) where T : GodotObject
    {
        return (node is not null)
            && GodotObject.IsInstanceValid(node)
            && !node.IsQueuedForDeletion();
    }
    /// <summary>
    /// Extension that checks if the object is valid to use. See the "IsValid" extension for more information.
    /// </summary>  
    public static T IfValid<T>(this T control) where T : GodotObject
           => control.IsValid() ? control : null;
    public static void SafeQueueFree(this Node node)
    {
        if (node.IsValid()) { node.QueueFree(); }
        else { GD.PrintErr("Couldn't Safely Queue Free node: ", node.Name, ", node is not valid"); }
    }
    #endregion
    #region SEARCH_EXTENSIONS
    public static T GetFirstNodeOfTypeInScene<T>(bool includeSubChildren = true) where T : Node
    {
        return (Engine.GetMainLoop() as SceneTree)?.CurrentScene.GetFirstChildOfType<T>(includeSubChildren);
    }
    public static T GetFirstChildOfType<T>(this Node root, bool includeSubChildren = true) where T : Node
    {
        if (!includeSubChildren)
        {
            Array<Node> children = root.GetChildren();
            foreach (var node in children)
            {
                if (node is T castedNode)
                    return castedNode;
            }
        }
        else
        {
            Array<Node> nodesToParse = root.GetChildren();
            while (nodesToParse.Count > 0)
            {
                var cursor = nodesToParse[0];
                nodesToParse.Remove(cursor);
                if (cursor is T castedNode)
                    return castedNode;
                nodesToParse.AddRange(cursor.GetChildren());
            }
        }

        return null;
    }
    public static Array<T> GetChildrenOfType<[MustBeVariant] T>(this Node root, bool includeSubChildren = true) where T : Node
    {
        Array<T> childArray = new Array<T>();
        if (!includeSubChildren)
        {
            foreach (var node in root.GetChildren())
            {
                if (node is T castNode)
                    childArray.Add(castNode);
            }
        }
        else
        {
            Array<Node> nodesToParse = root.GetChildren();
            while (nodesToParse.Count > 0)
            {
                var cursor = nodesToParse[0];
                nodesToParse.Remove(cursor);
                if (cursor is T castedNode)
                {
                    childArray.Add(castedNode);
                }
                nodesToParse.AddRange(cursor.GetChildren());
            }
        }
        return childArray;
    }

    // SAME AS "GetChildrenOfType" once we added the "includeSubChildren" param, so it's redundant
    //public static Array<T> GetAllNodesOfType<[MustBeVariant] T>(this Node root) where T : Node
    //{
    //    Array<Node> nodesToParse = root.GetChildren();
    //    Array<T> castedNodes = new Array<T>();
    //    while (nodesToParse.Count > 0)
    //    {
    //        var cursor = nodesToParse[0];
    //        nodesToParse.Remove(cursor);
    //        if (cursor is T castedNode)
    //            castedNodes.Add(castedNode);
    //        nodesToParse.AddRange(cursor.GetChildren());
    //    }

    //    return castedNodes;
    //}
    public static Array<Node> GetAllChildrenNodesInGroup(this Node root, StringName groupName, bool includeSubChildren = true)
    {
        Array<Node> children = root.GetChildren();
        Array<Node> groupChildren = new Array<Node>();
        
        if (!includeSubChildren)
        {
            foreach (var child in children)
            {
                if (child.IsInGroup(groupName))
                {
                    groupChildren.Add(child);
                }
            }
        }
        else
        {
            Array<Node> nodesToParse = root.GetChildren();
            while (nodesToParse.Count > 0)
            {
                var cursor = nodesToParse[0];
                nodesToParse.Remove(cursor);
                if (cursor.IsInGroup(groupName)) 
                {
                    groupChildren.Add(cursor);
                }
                nodesToParse.AddRange(cursor.GetChildren());
            }
        }
        return groupChildren;
    }
    public static Array<T> GetAllNodesOfTypeInScene<[MustBeVariant] T>(bool includeSubChildren = true) where T : Node
    {
        Node currentScene = (Engine.GetMainLoop() as SceneTree)?.CurrentScene;
        return currentScene == null ? null :
            currentScene.GetChildrenOfType<T>(includeSubChildren);
            //includeSubChildren ? currentScene.GetAllNodesOfType<T>() : currentScene.GetChildrenOfType<T>();
    }
    #endregion
}
