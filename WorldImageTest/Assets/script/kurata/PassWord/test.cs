
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class test : UdonSharpBehaviour
{
    void Start()
    {
        
    }

    private void Update()
    {
        this.transform.position = new Vector3(0, Mathf.Sin(Time.time) * 2, 0);
    }
}
