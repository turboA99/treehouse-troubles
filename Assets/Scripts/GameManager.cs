using myUI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private AudioMixer audioMixer;

    //Settings architecture
    public class Settings
    {
        private AudioMixer audioMixer;

        public bool isFullscreen;
        private float _masterVolume;
        public float masterVolume {
            get {
                return _masterVolume;
            }
            set
            {
                _masterVolume = value;
                audioMixer.SetFloat("MasterVolume", _masterVolume);
            }
        }
        private float _musicVolume;
        public float musicVolume
        {
            get
            {
                return _musicVolume;
            }
            set
            {
                _musicVolume = value;
                audioMixer.SetFloat("MusicVolume", _musicVolume);
            }
        }
        private float _sfxVolume;
        public float sfxVolume
        {
            get
            {
                return _sfxVolume;
            }
            set
            {
                _sfxVolume = value;
                audioMixer.SetFloat("SfxVolume", _sfxVolume);
            }
        }
        public float sensitivity;
        public Settings(AudioMixer audioMixer, bool isFullscreen, float masterVolume, float musicVolume, float sfxVolume, float sensitivity)
        {
            this.audioMixer = audioMixer;
            this.isFullscreen = isFullscreen;
            this.masterVolume = masterVolume;
            this.musicVolume = musicVolume;
            this.sfxVolume = sfxVolume;
            this.sensitivity = sensitivity;
        }
    }

    public Settings currentSettings;

    [SerializeField] private HousingObj defaultHousing => HousingObj.CreateInstance<HousingObj>();

    private void Awake()
    {
        //Singletone
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //Loading Settings
        currentSettings = new Settings( 
            audioMixer,
            PlayerPrefs.GetInt("isFullscreen", true ? 1 : 0) == 1 ? true : false,
            PlayerPrefs.GetFloat("masterVolume", 0f),
            PlayerPrefs.GetFloat("musicVolume", 0f),
            PlayerPrefs.GetFloat("sfxVolume", -15f),
            PlayerPrefs.GetFloat("sensitivity", 0.5f)
        );

        Screen.fullScreen = currentSettings.isFullscreen;
    }

    //First objective
    Objective objective;

    private void Start()
    {
        //Setting volume of mixer
        audioMixer.SetFloat("MasterVolume", currentSettings.masterVolume);
        audioMixer.SetFloat("MusicVolume", currentSettings.musicVolume);
        audioMixer.SetFloat("SfxVolume", currentSettings.sfxVolume);

        //Setting first housing and objective
        objective = AddObjective("Go check out the first house!");
        SetHousing(defaultHousing);
    }

    //Just an API to complete first objective
    public void CompleteFirstObjective()
    {
        objective.SetCompleted(true);
    }

    //An API to add objectives that returns an Objectve class object, that has method to mark it as completed
    public Objective AddObjective(string title)
    {
        return UI.instance.AddObjective(title);
    }

    //An API for changing housing which requires a ScriptableObject of type HousingObj which contains all data to display
    public void SetHousing(HousingObj housingObj)
    {
        UI.instance.SetHousing(housingObj);
    }
}