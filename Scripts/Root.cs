using Godot;
using System;
using System.Collections.Generic;

public partial class Root : Node3D
{

    public static void GetRecursive<T>(Node parent, List<T> outNodes) where T : class
    {
        if (parent is T) {
            outNodes.Add(parent as T);
        }
        foreach (var child in parent.GetChildren()) {
            GetRecursive<T>(child, outNodes);
        }

    }

    public static T FindNodeRecusive<T>(Node parent) where T : class
    {
        if (parent is T) {
            return parent as T;
        }
        foreach (var child in parent.GetChildren()) {
            T node = FindNodeRecusive<T>(child);
            if (node != null) {
                return node;
            }
        }
        return null;
    }

    public static float Timef()
    {
        return Time.GetTicksMsec() / 1000.0f;
    }

    public override void _Ready()
	{
	
	}

	public override void _Process(double delta)
	{

	}
}
