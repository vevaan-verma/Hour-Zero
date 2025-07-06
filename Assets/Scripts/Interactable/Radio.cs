using UnityEngine;

public class Radio : Interactable {


    [Header("Config")]
    [SerializeField][Tooltip("Ordered")] private SFXLib.Sounds[] playlist;
    [SerializeField] private bool startPlaying;
    [Header("Hop")]
    [SerializeField] private float hopForceMin;
    [SerializeField] private float hopForceMax;
    [SerializeField] private float hopTorqueMin;
    [SerializeField] private float hopTorqueMax;
    private int nowPlayingIdx;
    private bool isPlaying;

    ParticleSystem particles;
    AudioPlayer audioPlayer;
    Rigidbody rb;

    new private void Start() {

        base.Start();
        particles = GetComponent<ParticleSystem>();
        audioPlayer = GetComponent<AudioPlayer>();
        rb = GetComponent<Rigidbody>();

        nowPlayingIdx = 0;
        isPlaying = startPlaying;

        if (isPlaying) {

            audioPlayer.Play(playlist[nowPlayingIdx], true);
            particles.Play();

        }

    }

    // hey buddy, ik u came to look at this, idk how to use ur class i tried my best
    public override bool Interact() {

        if (!base.Interact()) return false;

        if (!isPlaying) {

            audioPlayer.Play(playlist[nowPlayingIdx], true);

            isPlaying = true;

            particles.Play();
            rb.AddTorque(new Vector3(Random.Range(hopTorqueMin, hopTorqueMax), Random.Range(hopTorqueMin, hopTorqueMax), Random.Range(hopTorqueMin, hopTorqueMax)));
            rb.AddForce(Vector3.up * Random.Range(hopForceMin, hopForceMax));


        }
        else if (isPlaying) {

            audioPlayer.Stop(playlist[nowPlayingIdx]);

            nowPlayingIdx++;
            if (nowPlayingIdx >= playlist.Length)
                nowPlayingIdx = 0;

            isPlaying = false;

            particles.Stop();

        }

        return true;
    }

}
