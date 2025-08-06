using UnityEditor;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
public class PopupSound : Popup {


    // this is the maximum value of the cutoff frequency in the Audio Low Pass Filter
    private const float defaultFreq = 22000;
    // this is the freq used when occluding the audio
    private const float muffledFreq = 9000;
    // volume used when occluding 
    private const float muffledVolMult = 0.8f;
    private float defaultVol;
    // for optimization purposes, the occlusion check (for doOcclusion) is only done every nth tick 
    private const int fixedUpdateOcclusionTicks = 3;
    private int occlusionTickCounter = 0;

    private AudioSource audioSource;
    // Everything that plays sound uses an AudioPlayer to do so, and the AudioPlayer keeps track of all the 
    //      PopupSounds it spawned in that are active. The PopupSound tells the AudioPlayer when it is done existing. 
    private AudioPlayer owner;
    private SFXLib sfx;
    // Simiular to above, each of these SoundPopups needs to know what key it corresponds to in the SFXLib.
    // note that the SFXLib can only have each popup appear once, meaning no two keys have any popups in common
    private SFXLib.Sounds? key;
    private AudioLowPassFilter filter;

    [Header("Occlusion")]
    [SerializeField, Tooltip("When an object comes between the audio source and the player (listener), apply an affect to the audio for realism")] private bool doOcclusion;
    private AudioListener listener;

    [Header("Auto Assign Directory")]
    [SerializeField, ReadOnly] private string audioClipDirectory = "Assets/Art/Sounds/";

    /// <summary>
    /// 
    /// PopupSounds play sound, looping or unlooping
    /// 
    /// </summary>

    public override void Initialize() {

        audioSource = GetComponent<AudioSource>();
        sfx = FindAnyObjectByType<SFXLib>();
        listener = FindFirstObjectByType<AudioListener>();
        filter = GetComponent<AudioLowPassFilter>();

        defaultVol = audioSource.volume;
        filter.cutoffFrequency = defaultFreq;
        audioSource.mute = false;
        occlusionTickCounter = 0;
        ForceSetDuration();


        gameObject.name = "Popup Sound";

    }

    // set the AudioPlayer to report back to when done playing
    public void SetOwner(AudioPlayer owner) => this.owner = owner;

    // tell this PopupSound which key from SFXLib it corresponds to
    public void SetKey(SFXLib.Sounds key) => this.key = key;

    // check which key this PopupSound corresponds to in the SFXLib
    public SFXLib.Sounds? GetKey() => key;

    // replace this's AudioSource
    // with the one from other
    override public void SwapPopup(Popup other) {

        if (other is not PopupSound) {

            Debug.Log("Failed to update Popup... Popup passed in must be a PopupSound");
            return;

        }

        PopupSound otherPopupSound = other as PopupSound;
        AudioSource otherAudioSource = otherPopupSound.GetComponent<AudioSource>();

        duration = otherPopupSound.duration;
        doOcclusion = otherPopupSound.doOcclusion;
        // key is not swapped out here, it is assigned in AudioPlayer.Play

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
        defaultVol = otherAudioSource.volume;

        owner = otherPopupSound.owner;

    }
    protected override void OnFinish() {

        base.OnFinish();

        if (owner != null)
            owner.OnSoundComplete(this);

    }

    // the "duration" field of Popup does not work for PopupSounds... this is a very bad bandaid fix
    // TODO: recode Popup to allow PopupSound to not have a duration field
    private void OnValidate() {

        audioSource = GetComponent<AudioSource>();
        ForceSetDuration();

    }

    private void ForceSetDuration() {

        if (audioSource.resource != null)
            duration = ((AudioClip)audioSource.resource).length;
        else
            duration = 0;

    }


#if UNITY_EDITOR

    // attempt to find a clip in the assets directory with the same name as this prefab
    public void TryAutoAssignClip() {

        audioSource = GetComponent<AudioSource>();
        AudioClip found = AssetDatabase.LoadAssetAtPath<AudioClip>(audioClipDirectory + gameObject.name + ".mp3");

        if (found == null)
            print("Could not find file " + audioClipDirectory + gameObject.name + ".mp3");
        else {

            audioSource.clip = found;
            print("Clip assigned succesfully (" + audioClipDirectory + gameObject.name + ".mp3)");

        }

    }

#endif

    // occlusion
    private void FixedUpdate() {

        if (doOcclusion) {

            occlusionTickCounter++;

            if (gameObject.activeSelf && occlusionTickCounter == fixedUpdateOcclusionTicks) {

                occlusionTickCounter = 0;

                // if there is an object between the player and the audio source, muffle the audio
                if (Physics.Raycast(transform.position, listener.transform.position - transform.position, out RaycastHit hit)) {

                    // layer 3 is the environment
                    if (hit.collider.gameObject.layer == 3) {

                        audioSource.volume = defaultVol * muffledVolMult;
                        filter.cutoffFrequency = muffledFreq;

                    }

                    else {

                        audioSource.volume = defaultVol;
                        filter.cutoffFrequency = defaultFreq;

                    }

                }

            }

        }

    }



}