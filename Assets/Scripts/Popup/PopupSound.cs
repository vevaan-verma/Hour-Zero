using UnityEngine;

[ExecuteAlways]
public class PopupSound : Popup {

    private AudioSource audioSource;

    // Everything that plays sound uses an AudioPlayer to do so, and the AudioPlayer keeps track of all the 
    //      PopupSounds it spawned in that are active. The PopupSound tells the AudioPlayer when it is done existing. 
    private AudioPlayer owner;
    private SFXLib sfx;

    /// <summary>
    /// 
    /// PopupSounds play sound, looping or unlooping
    /// 
    /// </summary>

    public override void Initialize() {

        audioSource = GetComponent<AudioSource>();
        sfx = FindAnyObjectByType<SFXLib>();
        audioSource.mute = false;

        gameObject.name = "Popup Sound";

    }

    public void SetOwner(AudioPlayer owner) => this.owner = owner;

    // replace this's AudioSource
    // with the ones from other
    override public void SwapPopup(Popup other) {

        if (other is not PopupSound) {

            Debug.Log("Failed to update Popup... Popup passed in must be a PopupSound");
            return;

        }

        PopupSound otherPopupSound = other as PopupSound;
        AudioSource otherAudioSource = otherPopupSound.GetComponent<AudioSource>();

        duration = otherPopupSound.duration;
        owner = otherPopupSound.owner;

        audioSource.resource = otherAudioSource.resource;
        audioSource.outputAudioMixerGroup = otherAudioSource.outputAudioMixerGroup;
        audioSource.bypassEffects = otherAudioSource.bypassEffects;
        audioSource.bypassListenerEffects = otherAudioSource.bypassListenerEffects;
        audioSource.bypassReverbZones = otherAudioSource.bypassReverbZones;
        audioSource.playOnAwake = otherAudioSource.playOnAwake;
        audioSource.loop = otherAudioSource.loop;
        audioSource.priority = otherAudioSource.priority;
        audioSource.volume = otherAudioSource.volume;
        audioSource.pitch = otherAudioSource.pitch;
        audioSource.panStereo = otherAudioSource.panStereo;
        audioSource.spatialBlend = otherAudioSource.spatialBlend;
        audioSource.reverbZoneMix = otherAudioSource.reverbZoneMix;
        audioSource.dopplerLevel = otherAudioSource.dopplerLevel;
        audioSource.spread = otherAudioSource.spread;
        audioSource.minDistance = otherAudioSource.minDistance;
        audioSource.maxDistance = otherAudioSource.maxDistance;
        audioSource.rolloffMode = otherAudioSource.rolloffMode;

        owner = otherPopupSound.owner;

    }

    public SFXLib.Sounds? GetKey() {

        return sfx.GetKey(this);

    }

    protected override void OnFinish() {

        owner.OnSoundComplete(this);

    }

    // the "duration" field of Popup does not work for PopupSounds... this is a very bad bandaid fix
    // TODO: recode Popup to allow PopupSound to not have a duration field
    private void OnValidate() {

        audioSource = GetComponent<AudioSource>();
        duration = ((AudioClip)audioSource.resource).length;

    }


}