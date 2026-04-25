using System;
using UnityEngine;

public class vehicleFX : MonoBehaviour
{
    [SerializeField]
    Movement movement;

    [Header("BasicFX")]
    [SerializeField] AudioSource engineSound;


    [Header("SkidFX")]
    [SerializeField] TrailRenderer[] skidMarks;
    [SerializeField] ParticleSystem[] skidSmokes;
    [SerializeField] AudioSource skidSound;
    [SerializeField, Range(0, 1)] float minPitch = 1f;
    [SerializeField, Range(0, 5)] float maxpitch = 5f;

    private void LateUpdate()
    {
        engineSound.pitch = Mathf.Lerp(minPitch, maxpitch, Mathf.Abs(movement.CarVelocityRatio));
    }

    private void OnEnable()
    {
        movement.OnSkidStart += EnableSkidFX;
        movement.OnSkidStop += DisableSkidFX;
    }

    private void OnDisable()
    {
        movement.OnSkidStart -= EnableSkidFX;
        movement.OnSkidStop -= DisableSkidFX;
    }

    void EnableSkidFX()
    {
        //Play skid effects
        foreach (var skidMark in skidMarks)
        {
            skidMark.emitting = true;
        }

        //Play smoke effects
        foreach (var smoke in skidSmokes)
        {
            smoke.Play();
        }

        //Play the skid sound
        skidSound.Play();
    }

    void DisableSkidFX()
    {
        //Stop skid effects
        foreach (var skidMark in skidMarks)
        {
            skidMark.emitting = false;
        }

        //Stop smoke effects
        foreach (var smoke in skidSmokes)
        {
            smoke.Stop();
        }

        //Stop playing skid sound
        skidSound.Stop();
    }
}
