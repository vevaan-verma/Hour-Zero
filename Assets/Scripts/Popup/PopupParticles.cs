using System.Collections.Generic;
using UnityEngine;

public class PopupParticles : Popup {

    private ParticleSystem particlePlayer;

    // i don't remember why i reimplemented all the ParticleSystem fields here, but i'm going to assume there was a good reason

    [Header("Particle Appearance")]
    [SerializeField] private Material particle;
    [SerializeField] private ParticleSystem.MinMaxGradient startColor;
    [SerializeField] private ParticleSystem.MinMaxGradient colorOverLifetime;
    [SerializeField] private ParticleSystem.MinMaxCurve startSize;
    [SerializeField] private ParticleSystem.MinMaxCurve sizeOverLifetime;

    [Header("Emission")]
    [SerializeField] private ParticleSystem.MinMaxCurve lifetime;
    [SerializeField] private ParticleSystem.MinMaxCurve rate;
    [SerializeField] private List<PopupParticleBurst> bursts;
    [SerializeField] ParticleSystemSimulationSpace simSpace;
    [SerializeField] ParticleSystemScalingMode scalingMode;

    [Header("Emission Shape")]
    [SerializeField][Range(0, 50)] private float radius;
    [SerializeField][Range(1, 360)] private float arc;
    [SerializeField][Range(1, 360)] private float arcRotation;
    [SerializeField] private ParticleSystemShapeMultiModeValue arcMode;
    [SerializeField][Range(1, 100)] private float numDirections;
    [SerializeField][Range(0.01f, 50)] private float arcSpeed;

    [Header("Movement")]
    [SerializeField] private ParticleSystem.MinMaxCurve gravityModifier;
    [SerializeField] private ParticleSystem.MinMaxCurve startSpeed;
    [SerializeField] private ParticleSystem.MinMaxCurve xVelocity;
    [SerializeField] private ParticleSystem.MinMaxCurve yVelocity;
    [SerializeField] private ParticleSystem.MinMaxCurve speedModifier;
    [SerializeField] private ParticleSystem.MinMaxCurve startRotation;
    [SerializeField] private ParticleSystem.MinMaxCurve angularSpeed;

    [Header("Sprite Sheet Settings")]
    [SerializeField] private int rows;
    [SerializeField] private int cols;
    [SerializeField] private int fps;

    /*  [Header("Super Special Secret Sword Slash Settings")]
      [SerializeField][Tooltip("Make particles start facing away from the player")] private bool pointAwayFromPlayer;
      [SerializeField][Tooltip("Default angle on the unit circle in radians, used for the above setting")] private float slashStartRot;*/

    // todo: collisions

    public override void Initialize() {

        particlePlayer = GetComponent<ParticleSystem>();

        particlePlayer.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        ApplyValues();
        particlePlayer.Play();

        gameObject.name = "Popup Particles";

    }

    override public void SwapPopup(Popup other) {

        if (other is not PopupParticles) {

            Debug.Log("Failed to update Popup... Popup passed in must be a PopupParticles");
            return;

        }

        ((PopupParticles)other).particlePlayer = other.GetComponent<ParticleSystem>();
        particlePlayer = GetComponent<ParticleSystem>();

        ((PopupParticles)other).particlePlayer.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

        transform.localScale = other.transform.localScale;
        transform.localEulerAngles = other.transform.localEulerAngles;

        //PopupIcon other = (PopupIcon)popup;
        ParticleSystem otherParticlePlayer = other.GetComponent<ParticleSystem>();

        ParticleSystem.MainModule otherMain = otherParticlePlayer.main;
        ParticleSystem.MainModule thisMain = particlePlayer.main;
        thisMain.startLifetime = otherMain.startLifetime;
        thisMain.startSpeed = otherMain.startSpeed;
        thisMain.startSize = otherMain.startSize;
        thisMain.startRotation = otherMain.startRotation;
        thisMain.startColor = otherMain.startColor;
        thisMain.gravityModifier = otherMain.gravityModifier;
        thisMain.simulationSpace = otherMain.simulationSpace;
        thisMain.scalingMode = otherMain.scalingMode;
        thisMain.duration = otherMain.duration;

        ParticleSystem.EmissionModule otherEmission = otherParticlePlayer.emission;
        ParticleSystem.EmissionModule thisEmission = particlePlayer.emission;
        List<PopupParticleBurst> otherBursts = ((PopupParticles)other).bursts;
        thisEmission.SetBursts(new ParticleSystem.Burst[otherBursts.Count]);
        for (int i = 0; i < otherBursts.Count; i++) {

            PopupParticleBurst burstData = otherBursts[i];
            thisEmission.SetBurst(i, new ParticleSystem.Burst(burstData.time, burstData.particleCount, burstData.cycles, burstData.interval));

            ParticleSystem.Burst burst = thisEmission.GetBurst(i);
            burst.probability = otherBursts[i].probability;
            thisEmission.SetBurst(i, burst);

        }
        thisEmission.rateOverTime = otherEmission.rateOverTime;

        ParticleSystem.ShapeModule otherShape = otherParticlePlayer.shape;
        ParticleSystem.ShapeModule thisShape = particlePlayer.shape;
        thisShape.radius = otherShape.radius;
        thisShape.arc = otherShape.arc;
        thisShape.rotation = otherShape.rotation;
        thisShape.arcMode = otherShape.arcMode;
        thisShape.arcSpeed = otherShape.arcSpeed;
        thisShape.arcSpread = otherShape.arcSpread;

        ParticleSystem.VelocityOverLifetimeModule otherVel = otherParticlePlayer.velocityOverLifetime;
        ParticleSystem.VelocityOverLifetimeModule thisVel = particlePlayer.velocityOverLifetime;
        thisVel.x = otherVel.x;
        thisVel.y = otherVel.y;
        thisVel.speedModifier = otherVel.speedModifier;

        ParticleSystem.ColorOverLifetimeModule otherColor = otherParticlePlayer.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule thisColor = particlePlayer.colorOverLifetime;
        thisColor.color = otherColor.color;

        ParticleSystem.SizeOverLifetimeModule otherSize = otherParticlePlayer.sizeOverLifetime;
        ParticleSystem.SizeOverLifetimeModule thisSize = particlePlayer.sizeOverLifetime;
        thisSize.size = otherSize.size;

        ParticleSystem.RotationOverLifetimeModule otherRotation = otherParticlePlayer.rotationOverLifetime;
        ParticleSystem.RotationOverLifetimeModule thisRotation = particlePlayer.rotationOverLifetime;
        thisRotation.z = otherRotation.z;

        ParticleSystem.TextureSheetAnimationModule otherAnimation = otherParticlePlayer.textureSheetAnimation;
        ParticleSystem.TextureSheetAnimationModule thisAnimation = particlePlayer.textureSheetAnimation;
        thisAnimation.numTilesX = otherAnimation.numTilesX;
        thisAnimation.numTilesY = otherAnimation.numTilesY;
        thisAnimation.fps = otherAnimation.fps;

        ParticleSystemRenderer otherRenderer = otherParticlePlayer.GetComponent<ParticleSystemRenderer>();
        ParticleSystemRenderer thisRenderer = GetComponent<ParticleSystemRenderer>();
        thisRenderer.sharedMaterial = otherRenderer.sharedMaterial;

    }

    public void ApplyValues() {

        // need to reassign this reference for the ApplyValues button in the inspector to function
#if UNITY_EDITOR
        particlePlayer = GetComponent<ParticleSystem>();
#endif

        ParticleSystem.MainModule main = particlePlayer.main;
        main.startLifetime = lifetime;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        main.startRotation = startRotation;
        main.startColor = startColor;
        main.gravityModifier = gravityModifier;
        main.simulationSpace = simSpace;
        main.scalingMode = scalingMode;
        main.duration = duration;

        ParticleSystem.EmissionModule emission = particlePlayer.emission;
        emission.SetBursts(new ParticleSystem.Burst[bursts.Count]);
        for (int i = 0; i < bursts.Count; i++) {

            PopupParticleBurst burstData = bursts[i];
            emission.SetBurst(i, new ParticleSystem.Burst(burstData.time, burstData.particleCount, burstData.cycles, burstData.interval));

            // why did unity not make the probability a part of the constructor? no one knows!
            ParticleSystem.Burst burst = emission.GetBurst(i);
            burst.probability = bursts[i].probability;
            emission.SetBurst(i, burst);

        }
        emission.rateOverTime = rate;

        ParticleSystem.ShapeModule shape = particlePlayer.shape;
        shape.radius = radius;
        shape.arc = arc;
        shape.rotation = new Vector3(0, 0, arcRotation);
        shape.arcMode = arcMode;
        shape.arcSpeed = arcSpeed;
        shape.arcSpread = 1 / numDirections;

        ParticleSystem.VelocityOverLifetimeModule vel = particlePlayer.velocityOverLifetime;
        vel.x = xVelocity;
        vel.y = yVelocity;
        vel.speedModifier = speedModifier;

        ParticleSystem.ColorOverLifetimeModule color = particlePlayer.colorOverLifetime;
        color.color = colorOverLifetime;

        ParticleSystem.SizeOverLifetimeModule size = particlePlayer.sizeOverLifetime;
        size.size = sizeOverLifetime;

        ParticleSystem.RotationOverLifetimeModule rotation = particlePlayer.rotationOverLifetime;
        rotation.z = angularSpeed;

        ParticleSystem.TextureSheetAnimationModule animation = particlePlayer.textureSheetAnimation;
        animation.numTilesX = cols;
        animation.numTilesY = rows;
        animation.timeMode = ParticleSystemAnimationTimeMode.FPS;
        animation.fps = fps;

        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = particle;

    }


    private void OnDrawGizmosSelected() {

        Gizmos.color = new Color(1, 1, 1, (5 / numDirections));

        float radianInterval = (2 * Mathf.PI) / numDirections;

        for (int i = 0; i < numDirections; i++) {

            float theta = radianInterval * i;

            Vector3 dirVector = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta));

            Ray dir = new Ray(transform.position, dirVector);

            Gizmos.DrawRay(dir);

        }

    }

}