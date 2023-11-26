using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

public partial class MainUI : Control, Root.IScoreChangedHandler, Root.IAlertHandler, Waldo.ITurnTimeChangedHandler, NPCManager.IOnLoadedHandler
{
	private Control spookyContainer;

	private Node loadingScreen;

	[Export]
	private PackedScene alertPrefab;

    [Export]
    private PackedScene blockPrefab;

    private List<SpookyBlock> blocks = new List<SpookyBlock>();

	[Export]
	private Color normalColor;

	[Export]
	private Color alertColor;

	int currentNumSpooks = -1;
	int currentMaxSpooks = -1;

	[Export]
	float alertWhenTimeGreaterThan = 0.6f;

	private Root root;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		root = Root.Get(GetTree());
		targetColor = normalColor;
		prevColor = normalColor;
        spookyContainer = FindChild("SpookyBar") as Control;
		loadingScreen = FindChild("LoadingScreen");
		if (loadingScreen != null && root.IsTutorial) {
			OnLoaded();
		}
		var gameStats = GameStats.Get(this);
		if (gameStats != null) {
			Label text = loadingScreen.FindChild("Label") as Label;
			if (text != null) {
				text.Text = String.Format(text.Text, gameStats.currentLevel);
			}
		}
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
		Flash();
		SetElements(blocks, spookyContainer, maxSpooks, blockPrefab);
		for (int k = 0; k < numSpooks; k++) {
			blocks[k].SetFillAmount(1);
		}
		for (int j = numSpooks; j < maxSpooks; j++) {
			if (j > 0) {
				blocks[j].SetFillAmount(0);
			}
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
	private Color targetColor;
	private Color prevColor;
	private bool wasAlerting = false;
	private float lastFlashTime = 0.0f;
	private bool isFlashing = false;

	[Export]
	private Color flashColor;
	[Export]
	private float flashFor = 0.1f;
	[Export]
	private Curve flashCurve;
	public void Flash()
	{
        isFlashing = true;
        lastFlashTime = Root.Timef();
    }

	// Slowly fill up the next spooky block.
	public void OnTurnTimeChanged(float normalizedTime)
	{
		int targetSpook = currentNumSpooks;
		if (targetSpook < currentMaxSpooks && targetSpook >= 0) {
			blocks[targetSpook].SetFillAmount(normalizedTime);
		}
		bool shouldAlert = normalizedTime > alertWhenTimeGreaterThan;
        targetColor = shouldAlert ? alertColor : normalColor;
		if (wasAlerting != shouldAlert) {
			isFlashing = true;
			lastFlashTime = Root.Timef();
		}
		wasAlerting = shouldAlert;
		if (isFlashing) {
			float alpha = (Root.Timef() - lastFlashTime) / flashFor;
			if (alpha > 1.0f) {
				isFlashing = false;
			} else {
				targetColor = targetColor.Lerp(flashColor, flashCurve.Sample(alpha));
			}
		}
		prevColor = prevColor.Lerp(targetColor, 0.1f);
        foreach (SpookyBlock block in blocks) {
			block.SetColor(isFlashing ? targetColor : prevColor);
		}

		// Update the blocks so that they fill in a sensible order.
		float totalCurrentFill = 0;
		float totalDesiredFill = 0;
		for (int k = 0; k < blocks.Count; k++) {
			totalCurrentFill += blocks[k].GetCurrentDrawnAmount();
			totalDesiredFill += blocks[k].GetTargetFillAmount();
		}
		// If we haven't filled the correct number of blocks yet, then set the first block that doesn't have the
		// correct amount to active. All others to inactive.
		if (totalCurrentFill < totalDesiredFill) {
			bool foundBlock = false;
			foreach (var block in blocks) {
				if (!block.IsNearTarget() && !foundBlock) {
					block.SetActive(true);
					foundBlock = true;
				} else {
					block.SetActive(false);
				}
			}
        } else {
			bool foundBlock = false;
			// Otherwise, set the first block from the end active.
			for (int k = blocks.Count - 1; k >= 0; k--) {
                if (!blocks[k].IsNearTarget() && !foundBlock) {
					blocks[k].SetActive(true);
					foundBlock = true;
                } else {
					blocks[k].SetActive(false);
				}
            }
        }
	}

	public void OnLoaded()
	{
		if (loadingScreen != null && loadingScreen.NativeInstance.ToInt64() > 0x0) {
			loadingScreen.QueueFree();
		}
	}

}
