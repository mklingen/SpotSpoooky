using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MainUI : Control, Root.IScoreChangedHandler, Root.IAlertHandler
{
	private Control strikeContainer;
	private Control heartContainer;

	[Export]
	private PackedScene strikePrefab;

	[Export]
	private PackedScene heartPrefab;

	[Export]
	private PackedScene alertPrefab;

	private List<Node> strikes = new List<Node>();
	private List<Node> hearts = new List<Node>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		strikeContainer = FindChild("Strikes") as Control;
		heartContainer = FindChild("SpookyHearts") as Control;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SetElements(List<Node> elements, Control parent, int numElements, PackedScene prefab)
	{
        int numDeletes = elements.Count - numElements;
        for (int k = 0; k < numDeletes; k++) {
			Root.Kill(elements.ElementAt(0));
            elements.RemoveAt(0);
        }
		int numAdds = numElements - elements.Count;
		for (int k = 0; k < numAdds; k++) {
			var node = prefab.Instantiate();
			parent.AddChild(node);
			elements.Add(node);
		}
    }

	public void SetStrikes(int numStrikes)
	{
		SetElements(strikes, strikeContainer, numStrikes, strikePrefab);
	}

	public void SetHearts(int numHearts)
	{
		SetElements(hearts, heartContainer, numHearts, heartPrefab);
	}

	public void OnScoreChanged(int numStrikes, int numHearts, bool animate)
	{
		SetStrikes(numStrikes);
		SetHearts(numHearts);
	}

	public void MakeAlert(string alert)
	{
		var obj = alertPrefab.Instantiate();
		AddChild(obj);
		var txt = Root.FindNodeRecusive<Label>(obj);
		if (txt != null) {
			txt.Text = alert;
		}
	}

	public void OnAlert(string alert)
	{
		MakeAlert(alert);
	}
}
