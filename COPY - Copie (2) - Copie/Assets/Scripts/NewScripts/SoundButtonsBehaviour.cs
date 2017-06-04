using UnityEngine;
using System.Collections;

public class SoundButtonsBehaviour : MonoBehaviour {
	
    private SoundManager sm;

	// Use this for initialization
	void Start () {
        sm = (SoundManager)FindObjectOfType(typeof(SoundManager));

        //ToggleMusic();
        //ToggleSound();
    }

    void Update()
    {
        if (sm.musicFxSettingsOption)
        {
            ChangeMusicButton(true);
        }
        else
        {
            ChangeMusicButton(false);
        }

        if (sm.soundFxSettingsOption)
        {
            ChangeSoundButton(true);
        }
        else
        {
            ChangeSoundButton(false);
        }
    }

    public void ToggleMusic()
    {
        if (GameObject.Find("Musicfx").transform.GetChild(0).gameObject.GetActive())
        {
            sm.musicFxSettingsOption = false;
            sm.stopBackgroundMusic();

        }
        else if (GameObject.Find("Musicfx").transform.GetChild(1).gameObject.GetActive())
        {
            sm.musicFxSettingsOption = true;
            sm.playBackgroundMusic();
            
        }
        
    }

    public void ToggleSound()
    {
        if (GameObject.Find("Soundfx").transform.GetChild(0).gameObject.GetActive())
        {
            sm.soundFxSettingsOption = false;
        }
        else if (GameObject.Find("Soundfx").transform.GetChild(1).gameObject.GetActive())
        {
            sm.soundFxSettingsOption = true;            
        }
    }


    public void ChangeMusicButton(bool check)
    {
        if (check)
        {
            GameObject.Find("Musicfx").transform.GetChild(0).gameObject.SetActive(true);
            GameObject.Find("Musicfx").transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            GameObject.Find("Musicfx").transform.GetChild(0).gameObject.SetActive(false);
            GameObject.Find("Musicfx").transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    public void ChangeSoundButton(bool check)
    {
        if (check)
        {
            GameObject.Find("Soundfx").transform.GetChild(0).gameObject.SetActive(true);
            GameObject.Find("Soundfx").transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            GameObject.Find("Soundfx").transform.GetChild(0).gameObject.SetActive(false);
            GameObject.Find("Soundfx").transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
