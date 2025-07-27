using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SFXEntry {

    [SerializeField, Tooltip("Categorization label for this SFXEntry used to keep the library organized")] private SoundType label;
    [SerializeField, Tooltip("Unique identifier used to play this sound effect. Entries cannot share a key.")] private SFXLib.Sounds key;
    [SerializeField, Tooltip("PopupSounds associated with this sound. If there are multiple, a random one is selected on play")] private List<PopupSound> sounds;

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

    public SoundType Label {

        get { return label; }

    }

    public PopupSound GetSound() => sounds[UnityEngine.Random.Range(0, sounds.Count)];

    #endregion

    // modify to suit the needs of the project. this is only used to organize the library
    public enum SoundType {

        General,
        Object,
        Item,
        Music,
        Misc

    }

}
