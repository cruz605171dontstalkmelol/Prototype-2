using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else { Destroy(this.gameObject); }
    }

    private AudioSource _mySource;
    public AudioClip[] soundEffects;

    private void Start() {
        _mySource = GetComponent<AudioSource>();
    }


}
