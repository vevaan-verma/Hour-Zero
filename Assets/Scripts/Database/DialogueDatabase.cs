using System;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private DialogueSequence[] dialogueSequences;

    public DialogueSequence GetRandomDialogueSequence() => dialogueSequences[UnityEngine.Random.Range(0, dialogueSequences.Length)];

}

[Serializable]
public class DialogueSequence {

    [Header("Data")]
    [SerializeField] private string[] dialogueLines;

    public string[] GetDialogueLines() => dialogueLines;

}
