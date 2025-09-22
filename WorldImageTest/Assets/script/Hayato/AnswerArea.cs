using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class AnswerArea : UdonSharpBehaviour
{
    [SerializeField] private GameObject answerObj;
    [SerializeField] private TextMeshProUGUI answerText;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == answerObj)
        {
            answerText.text = ">>>success<<<";
        }
        else
        {
            answerText.text = ">>>fail<<<";
        }
    }
}
