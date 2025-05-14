using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CounterNoneUI : UdonSharpBehaviour
{
    public Renderer targetRenderer;
    public Color onColor = Color.green;
    public Color offColor = Color.red;

    private bool isOn = false;

    public override void Interact()
    {
        isOn = !isOn;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (targetRenderer != null)
        {
            targetRenderer.material.color = isOn ? onColor : offColor;
        }
    }
}
