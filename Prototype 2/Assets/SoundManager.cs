using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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

    public void PlaySound(int whatSound) {
        _mySource.PlayOneShot(soundEffects[whatSound]);
    }

    [PunRPC]
    public void PlaySoundServer(int whatSound) {
        _mySource.PlayOneShot(soundEffects[whatSound]);
    }

}
