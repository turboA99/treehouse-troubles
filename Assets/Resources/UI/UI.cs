using Cinemachine;
using myUI;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CinemachineFreeLook cinemachineFreeLook;
    [SerializeField] private AudioClip focusAudio;
    [SerializeField] private AudioClip clickAudio;
    [SerializeField] private AudioClip phoneAudio;
    [SerializeField] private AudioClip phoneClickAudio;
    [SerializeField] private AudioClip gameMusic;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceMusic;
    private VisualElement root;

    public static UI instance;
    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;
    private float sensitivity;
    private bool isFullscreen;

    [SerializeField]private float sliderStep = 2f;

    public bool isPhoneOpened { get; private set; } = false;

    private VisualElement phone;
    private Objectives objectives;
    private Housing housing;


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

        //Awake setup
        root = GetComponent<UIDocument>().rootVisualElement;
        objectives = root.Q<VisualElement>("ObjectivesScreen").Q<Objectives>("Objectives");
        phone = root.Q<VisualElement>("Phone");
        housing = root.Q<Housing>("Housing");
    }
    public void Start()
    {
        //Getting settings from GameManager class
        masterVolume = GameManager.instance.currentSettings.masterVolume;
        musicVolume = GameManager.instance.currentSettings.musicVolume;
        sfxVolume = GameManager.instance.currentSettings.sfxVolume;
        sensitivity = GameManager.instance.currentSettings.sensitivity;
        isFullscreen = GameManager.instance.currentSettings.isFullscreen;

        VisualElement viewPort = root.Q<VisualElement>("ViewPort");

        //Buttons
        Button buttonStart = root.Q<Button>("Start");
        Button buttonSettings = root.Q<Button>("Settings");
        Button buttonExit = root.Q<Button>("Exit");

        Button buttonObjectivesApp = root.Q<Button>("ObjectivesApp");
        Button buttonSettingsApp = root.Q<Button>("SettingsApp");
        Button buttonHousingApp = root.Q<Button>("HousingApp");

        //Containers
        VisualElement columnMain = root.Q<VisualElement>("ColumnMain");
        VisualElement columnSettings = root.Q<VisualElement>("ColumnSettings");
        VisualElement objectivesScreen = root.Q<VisualElement>("ObjectivesScreen");
        VisualElement housingScreen = root.Q<VisualElement>("HousingScreen");
        Objectives objectivesList = objectivesScreen.Q<Objectives>("Objectives");

        //Setting options
        Slider sliderMasterVolume = root.Q<Slider>("MasterVolume");
        Slider sliderMusicVolume = root.Q<Slider>("MusicVolume");
        Slider sliderSfxVolume = root.Q<Slider>("SfxVolume");
        Slider sliderSensitivity = root.Q<Slider>("Sensitivity");
        VisualElement fullscreenOption = root.Q<VisualElement>("Fullscreen");

        //Initial set up
        buttonStart.Focus();
        sliderMasterVolume.value = masterVolume;
        sliderMusicVolume.value = musicVolume;
        sliderSfxVolume.value = sfxVolume;
        sliderSensitivity.value = sensitivity;
        objectives = objectivesList;

        if (fullscreenOption.ElementAt(0) is Toggle) (fullscreenOption.ElementAt(0) as Toggle).value = isFullscreen;

        //Button actions
        buttonStart.clicked += () =>
        {
            audioSourceMusic.clip = gameMusic;
            audioSourceMusic.Play();
            root.Q<VisualElement>("ColumnMain").AddToClassList("hidden");
            foreach (var item in columnMain.Children())
            {
                item.focusable = false;
            };
            playerInput.SwitchCurrentActionMap("CharacterControls");
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            cinemachineFreeLook.enabled = true;
            
        };

        buttonStart.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });

        buttonSettings.clicked += () =>
        {
            audioSource.clip = clickAudio;
            audioSource.Play();
            columnMain.AddToClassList("inactive");
            columnSettings.RemoveFromClassList("hidden");
            foreach (var item in columnSettings.Children())
            {
                item.focusable = true;
            }
            foreach (var item in columnMain.Children())
            {

                if (!(item is Label))
                {
                    item.focusable = false;
                    item.SetEnabled(false);
                }
            }
            columnSettings.ElementAt(0).Focus();
        };
        buttonSettings.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });

        buttonExit.clicked += () =>
        {
            audioSource.clip = clickAudio;
            audioSource.Play();
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.ExitPlaymode();
            }
#endif
            Application.Quit();
        };
        buttonExit.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });


        //Exit on esc
        columnSettings.RegisterCallback<NavigationCancelEvent>(e =>
        {
            if(!isPhoneOpened)
            {
                columnMain.RemoveFromClassList("inactive");
                columnSettings.AddToClassList("hidden");
                foreach (var item in columnSettings.Children())
                {
                    item.focusable = false;
                }
                foreach (var item in columnMain.Children())
                {
                    if (!(item is Label))
                    {
                        item.focusable = true;
                        item.SetEnabled(true);
                    }
                }
                buttonSettings.Focus();
            }
            else
            {
                columnSettings.AddToClassList("hidden");
                IEnumerator coroutine()
                {
                    yield return new WaitForSeconds(.5f);
                    columnSettings.RemoveFromClassList("from-phone");
                }
                GameManager.instance.StartCoroutine(coroutine());
                foreach (var item in columnSettings.Children())
                {
                    item.focusable = false;
                }
                var apps = root.Query(className: "phone-app").ToList();
                apps.ForEach((app) =>
                {
                    if (app is Button) app.focusable = false;
                }
                );
                SetPhoneOpened(false);
            }
        });

        phone.RegisterCallback<NavigationCancelEvent>(e =>
        {
            SetPhoneOpened(false);
            root.Query(className: "phone-app").ForEach((app) =>
            {
                if (app is Button) app.focusable = false;
            }
            );
            playerInput.SwitchCurrentActionMap("CharacterControls");
            cinemachineFreeLook.enabled = true;
        });

        objectivesScreen.RegisterCallback<NavigationCancelEvent>(e =>
        {
            objectivesScreen.ToggleInClassList("active");
            objectivesList.SetActive(false);
            var apps = root.Query(className: "phone-app").ToList();
            apps.ForEach((app) =>
            {
                if (app is Button) app.focusable = true;
            }
            );
        });
        housingScreen.RegisterCallback<NavigationCancelEvent>(e =>
        {
            housingScreen.ToggleInClassList("active");
            housingScreen.focusable = false;
        });

        //Option actions
        fullscreenOption.RegisterCallback<NavigationSubmitEvent>(e =>
        {
            audioSource.clip = clickAudio;
            audioSource.Play();
            isFullscreen = !isFullscreen;
            if(fullscreenOption.ElementAt(0) is Toggle)
            {
                (fullscreenOption.ElementAt(0) as Toggle).value = isFullscreen;
            }
            GameManager.instance.currentSettings.isFullscreen = isFullscreen;
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("isFullscreen", isFullscreen ? 1 : 0);
        });
        fullscreenOption.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });

        sliderMasterVolume.RegisterCallback<NavigationMoveEvent>(e =>
        {
            audioSource.clip = clickAudio;
            audioSource.Play();
            if (e.direction == NavigationMoveEvent.Direction.Left)
            {
                sliderMasterVolume.value = Mathf.Clamp(sliderMasterVolume.value - sliderStep, sliderMasterVolume.lowValue, sliderMasterVolume.highValue);
            }
            if (e.direction == NavigationMoveEvent.Direction.Right)
            {
                sliderMasterVolume.value = Mathf.Clamp(sliderMasterVolume.value + sliderStep, sliderMasterVolume.lowValue, sliderMasterVolume.highValue);
            }
        });
        sliderMasterVolume.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });
        sliderMusicVolume.RegisterCallback<NavigationMoveEvent>(e =>
        {
            audioSource.clip = clickAudio;
            audioSource.Play();
            if (e.direction == NavigationMoveEvent.Direction.Left)
            {
                sliderMusicVolume.value = Mathf.Clamp(sliderMusicVolume.value - sliderStep, sliderMusicVolume.lowValue, sliderMusicVolume.highValue);
            }
            if (e.direction == NavigationMoveEvent.Direction.Right)
            {
                sliderMusicVolume.value = Mathf.Clamp(sliderMusicVolume.value + sliderStep, sliderMusicVolume.lowValue, sliderMusicVolume.highValue);
            }
        });
        sliderMusicVolume.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });
        sliderSfxVolume.RegisterCallback<NavigationMoveEvent>(e =>
        {
            audioSource.clip = clickAudio;
            audioSource.Play();
            if (e.direction == NavigationMoveEvent.Direction.Left)
            {
                sliderSfxVolume.value = Mathf.Clamp(sliderSfxVolume.value - sliderStep, sliderSfxVolume.lowValue, sliderSfxVolume.highValue);
            }
            if (e.direction == NavigationMoveEvent.Direction.Right)
            {
                sliderSfxVolume.value = Mathf.Clamp(sliderSfxVolume.value + sliderStep, sliderSfxVolume.lowValue, sliderSfxVolume.highValue);
            }
        });
        sliderSfxVolume.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });
        sliderSensitivity.RegisterCallback<NavigationMoveEvent>(e =>
        {
            audioSource.clip = clickAudio;
            audioSource.Play();
            if (e.direction == NavigationMoveEvent.Direction.Left)
            {
                sliderSensitivity.value = Mathf.Clamp(sliderSensitivity.value - 0.025f, sliderSensitivity.lowValue, sliderSensitivity.highValue);
            }
            if (e.direction == NavigationMoveEvent.Direction.Right)
            {
                sliderSensitivity.value = Mathf.Clamp(sliderSensitivity.value + 0.025f, sliderSensitivity.lowValue, sliderSensitivity.highValue);
            }
        });
        sliderSensitivity.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });

        sliderMasterVolume.RegisterValueChangedCallback(e =>
        {
            masterVolume = e.newValue;
            GameManager.instance.currentSettings.masterVolume = masterVolume;
            PlayerPrefs.SetFloat("masterVolume", masterVolume);
        });

        sliderMusicVolume.RegisterValueChangedCallback(e =>
        {
            musicVolume = e.newValue;
            GameManager.instance.currentSettings.musicVolume = musicVolume;
            PlayerPrefs.SetFloat("musicVolume", musicVolume);
        });

        sliderSfxVolume.RegisterValueChangedCallback(e =>
        {
            sfxVolume = e.newValue;
            GameManager.instance.currentSettings.sfxVolume = sfxVolume;
            PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        });

        sliderSensitivity.RegisterValueChangedCallback(e =>
        {
            sensitivity = e.newValue;
            GameManager.instance.currentSettings.sensitivity = sensitivity;
            PlayerPrefs.SetFloat("sensitivity", sensitivity);
            playerInput.gameObject.GetComponent<MovementController>()?.SetSensitivity(sensitivity);
        });

        //App opening in phone

        buttonObjectivesApp.clicked += () =>
        {
            audioSource.clip = phoneClickAudio;
            audioSource.Play();
            objectivesScreen.ToggleInClassList("active");
            var objectivesList = objectivesScreen.Q<Objectives>("Objectives");
            objectivesList.SetActive(true);
            var apps = root.Query(className: "phone-app").ToList();
            apps.ForEach((app) =>
            {
                if (app is Button) app.focusable = false;
            }
            );
        };
        buttonObjectivesApp.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });
        buttonHousingApp.clicked += () =>
        {
            audioSource.clip = phoneClickAudio;
            audioSource.Play();
            housingScreen.ToggleInClassList("active");
            housingScreen.focusable = true;
            housingScreen.Focus();
            var apps = root.Query(className: "phone-app").ToList();
            apps.ForEach((app) =>
            {
                if (app is Button) app.focusable = false;
            }
            );
        };
        buttonHousingApp.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });
        buttonSettingsApp.clicked += () =>
        {
            audioSource.clip = phoneClickAudio;
            audioSource.Play();
            columnSettings.RemoveFromClassList("hidden");
            columnSettings.AddToClassList("from-phone");
            foreach (var item in columnSettings.Children())
            {
                item.focusable = true;
            }
            columnSettings.ElementAt(0).Focus();
            var apps = root.Query(className: "phone-app").ToList();
            apps.ForEach((app) =>
            {
                if (app is Button) app.focusable = false;
            }
            );
        };
        buttonSettingsApp.RegisterCallback<FocusEvent>((e) =>
        {
            audioSource.clip = focusAudio;
            audioSource.Play();
        });
    }

    //An API to open phone
    public void SetPhoneOpened(bool state)
    {
        if (state)
        {
            if (phone.ClassListContains("hidden")) phone.RemoveFromClassList("hidden");
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            root.Q<VisualElement>("Clock").Q<Label>("Label").text = DateTime.Now.ToString("HH:mm");
        }
        else
        {
            if (!phone.ClassListContains("hidden")) phone.AddToClassList("hidden");
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            playerInput.SwitchCurrentActionMap("CharacterControls");
        }
        isPhoneOpened = state;
        var apps = root.Query(className: "phone-app").ToList();
        apps.ForEach((app) =>
        {
            if(app is Button) app.focusable = state;
        }
        );
        if(state) apps.First().Focus();
        audioSource.clip = phoneAudio;
        audioSource.Play();
    }

    //An API to add objectives to the UI
    public Objective AddObjective(string title)
    {
        return objectives.AddObjective(title);
    }

    //An API to add housing to the UI
    public void SetHousing(HousingObj housingObj)
    {
        housing.SetHousing(housingObj);
    }
}
