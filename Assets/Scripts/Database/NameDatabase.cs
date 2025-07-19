using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NameDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private NameData[] nameData;

    public string GetRandomName(Gender gender) {

        NameData[] filteredNames = nameData.Where(n => n.GetGender() == gender).ToArray();
        return filteredNames[UnityEngine.Random.Range(0, filteredNames.Length)].GetName();

    }
}

[Serializable]
public class NameData {

    [Header("Data")]
    [SerializeField] private string name;
    [SerializeField] private Gender gender;

    public string GetName() => name;

    public Gender GetGender() => gender;

}

public enum Gender {

    Male, Female

}
