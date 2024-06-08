using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    public List<AudioClip> kabouterDefeat = new();
    public List<AudioClip> kabouterSpotted = new();
    public List<AudioClip> kabouterAttack = new();
    public List<AudioClip> kabouterTrick = new();
    public enum SFXGroup { KabouterDefeat, KabouterSpotted, KabouterAttack, KabouterTrick }


    void Start()
    {
        Instance = this;
    }

    public AudioClip GetRandomSFX(SFXGroup group)
    {
        List<AudioClip> list;
        switch (group)
        {
            case SFXGroup.KabouterDefeat: list = kabouterDefeat; break;
            case SFXGroup.KabouterSpotted: list = kabouterSpotted; break;
            case SFXGroup.KabouterAttack: list = kabouterAttack; break;
            default: list = kabouterTrick; break;

        }
        return list[Random.Range(0, list.Count)];
    }
}
