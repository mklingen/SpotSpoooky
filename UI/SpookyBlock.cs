using Godot;
using System;

public partial class SpookyBlock : TextureRect
{

	[Export]
	private float fillAmountTarget;

	private ShaderMaterial shaderMaterial;

    private float currentFillAmount;

    private bool isActive = false;

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public bool IsNearTarget()
    {
        return Mathf.Abs(fillAmountTarget - currentFillAmount) < 1e-3;
    }

    public float GetTargetFillAmount()
    {
        return fillAmountTarget;
    }

    public float GetCurrentDrawnAmount()
    {
        return currentFillAmount;
    }

    public void SetColor(Color baseColor)
    {
        shaderMaterial.SetShaderParameter("baseColor", baseColor);
    }

	public override void _Ready()
	{
		base._Ready();
		shaderMaterial = (Material as ShaderMaterial).Duplicate(true) as ShaderMaterial;
		Material = shaderMaterial;
        setFillAmountInternal(0);
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (isActive) {
            setFillAmountInternal(Mathf.Lerp(currentFillAmount, fillAmountTarget, (float)delta * 10.0f));
        }
    }

    private void setFillAmountInternal(float amount)
	{
        bool isFilled = amount > 1e-3;
        bool isCompletelyFilled = amount > 0.999f;
        if (isFilled) {
            if (isCompletelyFilled) {
                shaderMaterial.SetShaderParameter("endFadeStart", 1.0f);
                shaderMaterial.SetShaderParameter("endFadeAlpha", 0.0f);
                shaderMaterial.SetShaderParameter("filledAmount", 1.0f);
            }
            else {
                shaderMaterial.SetShaderParameter("endFadeAlpha", 1.0f);
                shaderMaterial.SetShaderParameter("endFadeStart", 0.8f);
                shaderMaterial.SetShaderParameter("endFadeFinal", 0.9f);
                shaderMaterial.SetShaderParameter("filledAmount", amount);
            }
        }
        else {
            shaderMaterial.SetShaderParameter("filledAmount", 0.0f);
        }
        currentFillAmount = amount;
    }

	public void SetFillAmount(float amount)
	{
        fillAmountTarget = amount;
	}

	
}
