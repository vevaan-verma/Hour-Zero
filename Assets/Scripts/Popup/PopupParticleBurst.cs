using System;
using UnityEngine;

[Serializable]
public class PopupParticleBurst {

    [SerializeField][Min(0)] private float _time;
    [SerializeField] private ParticleSystem.MinMaxCurve _particleCount;
    [SerializeField][Min(0)] private int _cycles;
    [SerializeField][Min(0)] private float _interval;
    [SerializeField][Range(0, 1)] private float _probability;


    public PopupParticleBurst(float time, ParticleSystem.MinMaxCurve particleCount, int cycles, int interval, int probability) {

        _time = time;
        _particleCount = particleCount;
        _cycles = cycles;
        _interval = interval;
        _probability = probability;

    }

    public ParticleSystem.MinMaxCurve particleCount {

        get {
            return _particleCount;
        }
        set {
            _particleCount = value;
        }

    }

    public int cycles {

        get {
            return _cycles;
        }
        set {

            if (value < 0) {

                Debug.Log("PopupParticleBurst cycles must be a nonzero positive integer");
                cycles = 0;

            } else
                _cycles = value;

        }
    }

    public float probability {

        get {
            return _probability;
        }
        set {

            if (value < 0) {

                Debug.Log("PopupParticleBurst probability must be greater than 0");
                _probability = 0;

            } else if (value > 1) {

                Debug.Log("PopupParticleBurst probability must be less than 1");
                _probability = 1;

            } else
                _probability = probability;

        }
    }

    public float interval {

        get {
            return _interval;
        }
        set {

            _interval = interval;

        }

    }

    public float time {

        get {
            return _time;
        }
        set {

            if (value < 0) {

                Debug.Log("PopupParticleBurst time must be greater than zero");
                time = 0;

            } else
                _interval = value;

        }

    }

}
