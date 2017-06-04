using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

    private GameManager gm;

    private bool firstTimeBackgroundSound = false;

    public AudioClip SelectionAudioClip;
    public AudioClip ButtonAudioClip;
    public AudioClip BackgroundMusicClip;
    public AudioClip match3Clip;
    public AudioClip ElectroAudioClip;
    public AudioClip BoosterExplosionClip;
    public AudioClip BombExplosionClip;
    public AudioClip brickSmashClip;

    public AudioClip boosterCreationClip;
    public AudioClip bombCreationClip;
    public AudioClip electroCreationClip;

    public AudioClip[] attackClip;
    public AudioClip[] superAttackClip;
    public AudioClip[] dieClip;

    public AudioClip clockCountdownClip;

    //Explosion sounds sources
    AudioSource Background;
    AudioSource Button;
    AudioSource selection;
    AudioSource match3;
    AudioSource Electro;
    AudioSource BoosterExplosion;
    AudioSource BombExplosion;
    AudioSource brickSmash;

    //power up creation sounds sources
    AudioSource bombCreation;
    AudioSource boosterCreation;
    AudioSource electroCreation;

    AudioSource clock;

    //Character Sounds sources
    AudioSource userAttack;
    AudioSource userSuperAttack;
    AudioSource userDie;

    AudioSource oppoAttack;
    AudioSource oppoSuperAttack;
    AudioSource oppoDie;

    public bool soundFxSettingsOption = true;
    public bool musicFxSettingsOption = true;

    public static SoundManager smInstance;

    public static SoundManager instance
    {
        get
        {
            if (smInstance == null)
            {
                smInstance = GameObject.FindObjectOfType<SoundManager>();
                DontDestroyOnLoad(smInstance.gameObject);
            }
            return smInstance;
        }
    }

    void Awake()
    {
        if (smInstance == null)
        {
            smInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != smInstance)
            {
                Destroy(this.gameObject);
            }
        }

        gm = (GameManager)FindObjectOfType(typeof(GameManager));
    }

    void Start()
    {
		Background = AddAudio(BackgroundMusicClip);
        Button = AddAudio(ButtonAudioClip);
        selection = AddAudio(SelectionAudioClip);

        

        match3 = AddAudio(match3Clip);
        Electro = AddAudio(ElectroAudioClip);
        BoosterExplosion = AddAudio(BoosterExplosionClip);
        BombExplosion = AddAudio(BombExplosionClip);

        bombCreation = AddAudio(bombCreationClip);
        boosterCreation = AddAudio(boosterCreationClip);
        electroCreation = AddAudio(electroCreationClip);

        clock = AddAudio(clockCountdownClip);
        brickSmash = AddAudio(brickSmashClip);
        
    }

    void Update()
    {
        if (!firstTimeBackgroundSound)
        {
            firstTimeBackgroundSound = true;
            startBackgroundMusic();
        }        
    }

    public void InitializeUserSounds()
    {

        userAttack = AddAudio(attackClip[gm.getCharacterSelectedInt()]);
        userSuperAttack = AddAudio(superAttackClip[gm.getCharacterSelectedInt()]);
        //  userDie = AddAudio(dieClip[gm.getCharacterSelectedInt()]);
    }
    public void InitializeComputerSounds()
    {
        oppoAttack = AddAudio(attackClip[gm.OppoCharacterID-1]);
        oppoSuperAttack = AddAudio(superAttackClip[gm.OppoCharacterID-1]);
        //     oppoDie = AddAudio(dieClip[gm.OppoCharacterID]);
    }

    AudioSource AddAudio( AudioClip audioClip)
    {
        AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
        if (audioClip == BackgroundMusicClip)
        {
            audioSource.loop = true;
			audioSource.playOnAwake = true;
        }
        audioSource.clip = audioClip;
        return audioSource;
    }

    public void PlayCrincle() 
    {
        if (soundFxSettingsOption)
        {
            match3.Play();
        }        
    }

    public void PlayElectro()
    {
        if (soundFxSettingsOption)
        {
            Electro.Play();
        }
    }

    public void PlayBoosterExplosionSound()
    {
        if (soundFxSettingsOption)
        {
            BoosterExplosion.Play();
        }
    }

    public void PlayBombExplosionSound()
    {
        if (soundFxSettingsOption)
        {
            BombExplosion.Play();
        }
    }

    public void PlayBoosterCreationSound()
    {
        if (soundFxSettingsOption)
        {
            boosterCreation.Play();
        }
    }

    public void PlayBombCreationSound()
    {
        if (soundFxSettingsOption)
        {
            bombCreation.Play();
        }
    }

    public void PlayElectroCreationSound()
    {
        if (soundFxSettingsOption)
        {
            electroCreation.Play();
        }
    }

    public void PlayUserAttackSound()  
    {
        if (soundFxSettingsOption)
        {
            userAttack.Play();
        }
    }

    public void PlayUserSuperAttackSound()
    {
        if (soundFxSettingsOption)
        {
            userSuperAttack.Play();
        }
    }

    public void PlayUserDieSound()
    {
        if (soundFxSettingsOption)
        {
            userDie.Play();
        }
    }

    public void PlayOppoAttackSound()
    {
        if (soundFxSettingsOption)
        {
            oppoAttack.Play();
        }
    }

    public void PlayOppoSuperAttackSound()
    {
        if (soundFxSettingsOption)
        {
            oppoSuperAttack.Play();
        }
    }

    public void PlayOppoDieSound()
    {
        if (soundFxSettingsOption)
        {
            oppoDie.Play();
        }
    }

    public void PlayBricksSmashingSound()
    {
        if (soundFxSettingsOption)
        {
            brickSmash.Play();
        }
    }

    public void PlayClockSound()
    {
        if (soundFxSettingsOption)
        {
            clock.Play();
        }
    }

    public void playButtonClip()
    {
        if (soundFxSettingsOption)
        {
            Button.Play();
        }
    }

    public void playSelectionClip()
    {
        if (soundFxSettingsOption)
        {
            selection.Play();
        }
    }

	public void pauseMusic()
	{
		Background.Pause ();
	}
	public void resumeMusic()
	{
		Background.UnPause ();
	}
    public void playBackgroundMusic()
    {
//        Background.Play();
		Background.mute = false;
    }
    public void stopBackgroundMusic()
    {
       // Background.Stop();
		Background.mute = true;
    }

	public void startBackgroundMusic()
	{
	    Background.Play();
	}
}
