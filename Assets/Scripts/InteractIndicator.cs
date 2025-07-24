using TMPro;
using UnityEngine;

public class InteractIndicator : MonoBehaviour {

    [SerializeField] private TextMeshPro actionTextTmp;

    public void SetText(string text) => actionTextTmp.text = text;

}
