using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CounterContinuousUI : UdonSharpBehaviour
{
    [UdonSynced]
    private bool isOn = false;

    public Renderer targetRenderer;
    public Color onColor = Color.green;
    public Color offColor = Color.red;

    private void Start()
    {
        UpdateColor();
    }

    public override void Interact()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            return;
        }

        isOn = !isOn;
        UpdateColor();
        // ContinuousではRequestSerializationは不要
    }

    public override void OnDeserialization()
    {
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