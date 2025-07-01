using UnityEngine;

public class SurvivorController : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private SurvivorType survivorType;

}

public enum SurvivorType {

    Medic, Engineer, Guard, Cook, Scavenger

}
