using Godot;
using Godot.Collections;
using System;
using System.IO;

public static partial class NodeExts
{
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
