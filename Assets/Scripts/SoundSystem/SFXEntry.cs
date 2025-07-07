using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SFXEntry {

    [SerializeField] private SFXLib.Sounds key;
    [SerializeField] private List<PopupSound> sounds;


    public SFXEntry(SFXLib.Sounds key) {
        this.key = key;
        sounds = new List<PopupSound>();
    }

    #region Acessors

    public SFXLib.Sounds Key {

        get { return key; }

    }

    public List<PopupSound> Sounds {

        get { return sounds; }

    }

    public PopupSound GetSound() => sounds[UnityEngine.Random.Range(0, sounds.Count - 1)];



    #endregion

}
