using System;
using UnityEngine;

[Serializable]
public class PopupPoolConfigurator {

    [SerializeField] private Popup popup;
    [SerializeField] private int numToSpawn;

    public int NumToSpawn {

        get { return numToSpawn; }

    }

    public Popup Popup {

        get { return popup; }

    }

}
