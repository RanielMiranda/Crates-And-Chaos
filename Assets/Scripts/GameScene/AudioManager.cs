using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager {

    private static void PlaySound(AudioClip audioClip) {
        GameObject soundGameObject = new GameObject("Sound");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(audioClip);

        // Destroy the GameObject after the sound finishes playing
        Object.Destroy(soundGameObject, audioClip.length);
    }

    public static void MoveBox() {
        PlaySound(GameManager.Instance.MoveBox);
    }

    public static void MovePlayer() {
        PlaySound(GameManager.Instance.MovePlayer);
    }    

}