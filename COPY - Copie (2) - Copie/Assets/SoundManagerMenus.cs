using UnityEngine;
using System.Collections;

public class SoundManagerMenus : MonoBehaviour {

    private SoundManager sm;

    void Start()
    {
        sm = (SoundManager)FindObjectOfType(typeof(SoundManager));
    }

    public void playButtonClip()
    {
        sm.playButtonClip();
    }

    public void playSelectionClip()
    {
        sm.playSelectionClip();
    }
}
