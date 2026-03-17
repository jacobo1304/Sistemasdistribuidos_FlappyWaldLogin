using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // Required for MMSoundManagerSoundPlayEvent

public class Sonidos : MonoBehaviour
{
    [Header("Sound Effects")]
    public List<AudioClip> sfxList;
    public AudioClip checkpointSound; // Assign your checkpoint sound effect in the Inspector

    /// <summary>
    /// Plays a sound effect from the sfxList by its index.
    /// </summary>
    /// <param name="sfxIndex">The index of the sound effect in the sfxList.</param>
    public void PlaySFX(int sfxIndex)
    {
        if (sfxList == null || sfxIndex < 0 || sfxIndex >= sfxList.Count || sfxList[sfxIndex] == null)
        {
            Debug.LogWarning("Sonidos: SFX at index " + sfxIndex + " is not valid or sfxList is not set up.");
            return;
        }
        MMSoundManagerSoundPlayEvent.Trigger(sfxList[sfxIndex], MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
    }

    /// <summary>
    /// Plays the provided AudioClip as a sound effect.
    /// </summary>
    /// <param name="sfxClip">The AudioClip to play.</param>
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null)
        {
            Debug.LogWarning("Sonidos: The provided sfxClip is null.");
            return;
        }
        MMSoundManagerSoundPlayEvent.Trigger(sfxClip, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
    }

    /// <summary>
    /// Public method to be called by the CheckPoint's UnityEvent.
    /// </summary>
    public void PlayCheckpointSound()
    {
        if (checkpointSound != null)
        {
            PlaySFX(checkpointSound);
        }
        else
        {
            Debug.LogWarning("Sonidos: Checkpoint sound is not assigned in the Sonidos script.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
