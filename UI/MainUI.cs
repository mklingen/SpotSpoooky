using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MainUI : Control, Root.IScoreChangedHandler, Root.IAlertHandler, Waldo.ITurnTimeChangedHandler
{
	private Control spookyContainer;

	[Export]
	private PackedScene alertPrefab;

    [Export]
    private PackedScene blockPrefab;

    private List<SpookyBlock> blocks = new List<SpookyBlock>();

	int currentNumSpooks = -1;
	int currentMaxSpooks = -1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        spookyContainer = FindChild("SpookyBar") as Control;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SetElements(List<SpookyBlock> elements, Control parent, int numElements, PackedScene prefab)
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
			elements.Add(node as SpookyBlock);
		}
    }

	public void OnScoreChanged(int numSpooks, int maxSpooks)
	{
		SetElements(blocks, spookyContainer, maxSpooks, blockPrefab);
		for (int k = 0; k < numSpooks; k++) {
			blocks[k].SetFillAmount(1);
		}
		for (int j = numSpooks; j < maxSpooks; j++) {
			blocks[j].SetFillAmount(0);
		}
		currentNumSpooks = numSpooks;
		currentMaxSpooks = maxSpooks;
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

	// Slowly fill up the next spooky block.
	public void OnTurnTimeChanged(float normalizedTime)
	{
		int targetSpook = currentNumSpooks;
		if (targetSpook < currentMaxSpooks) {
			blocks[targetSpook].SetFillAmount(normalizedTime);
		}
	}
}
