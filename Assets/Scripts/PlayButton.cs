using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Misc;

public class PlayButton : MonoBehaviour
{
    public Clip clip;
    public Sprite loopIcon;
    public Sprite forwardIcon;
    public string text;
    public int textSize = 20;
    public float TimeLeft { get {return clip.end - source.time;}}
    public bool forceNext;

    ButtonState state = ButtonState.Inactive;
    public ButtonState State {
        get {return state;}
        set {
            state = value;
            _light.color = Chef.SelectByEnum(value, new Color(.6f, .1f, .1f), new Color(0, 1, 0), new Color(0, .8f, .8f), new Color(.8f, .8f, 0));
        }
    }
    [NonSerialized]
    public AudioSource source;
    Image buttonbg;
    Button button;
    TMP_Text _text;
    Image loop;
    Image lightbg;
    Image _light;

    int index;
    bool isEditing;

    void Awake() {
        source = GetComponent<AudioSource>();
        buttonbg = transform.GetChild(0).GetComponent<Image>();
        button = transform.GetChild(1).GetComponent<Button>();
        _text = transform.GetChild(2).GetComponent<TMP_Text>();
        loop = transform.GetChild(3).GetComponent<Image>();
        lightbg = transform.GetChild(4).GetComponent<Image>();
        _light = transform.GetChild(5).GetComponent<Image>();
    }

    public void Create(Clip clip, int index, bool isEditing) {
        this.clip = clip;
        this.index = index;
        this.isEditing = isEditing;
        text = clip.name;
        _text.text = text;
        _text.fontSize = textSize;
        if (clip.isLoop) loop.sprite = loopIcon;
        else loop.sprite = forwardIcon;
        buttonbg.fillAmount = 0;
        lightbg.fillAmount = 0;
        source.clip = Chef.project.audio;
        source.volume = 1;
    }
    
    float tempEnd;

    void Update() {
        float progress = Chef.Diff(clip.start, source.time, clip.end);
        float inProgress = Mathf.Clamp(Chef.Diff(clip.FadeStart, source.time, clip.start+0.01f), -1, 1);
        float outProgress = Mathf.Clamp(Chef.Diff(tempEnd+clip.fadeOut+0.01f, source.time, tempEnd), 0, 1);

        switch (state) {
            case ButtonState.Starting: {
                source.volume = Chef.project.volume * clip.volume * inProgress;
                lightbg.fillAmount = inProgress;
                if (inProgress >= 1) State = ButtonState.Active;
                break;
            }
            case ButtonState.Active: {
                source.volume = Chef.project.volume * clip.volume;
                buttonbg.fillAmount = progress;
                if (progress >= 1) {
                    if (clip.isLoop && !forceNext) source.time = clip.start;
                    else End();
                }
                break;
            }
            case ButtonState.Ending: {
                source.volume = Chef.project.volume * clip.volume * outProgress;
                lightbg.fillAmount = outProgress;

                if (outProgress <= 0) {
                    source.Stop();
                    State = ButtonState.Inactive;
                    buttonbg.fillAmount = 0;
                }
                break;
            }
            case ButtonState.Inactive: {
                if (source.isPlaying) source.Stop();
                source.time = clip.FadeStart;
                buttonbg.fillAmount = 0;
                lightbg.fillAmount = 0;
                tempEnd = clip.end;
                break;
            }
        }
    }

    public void PlayStart() {
        State = ButtonState.Starting; 
        source.Play();
        source.time = clip.FadeStart;
    }
    public void End() {
        State = ButtonState.Ending; 
        tempEnd = source.time;
        forceNext = false;
    }

    public void OnPress() {
        switch (state) {
            case ButtonState.Inactive: 
                if (isEditing) EditMenu.instance.TryStart(index);
                else ViewMenu.instance.TryStart(index); 
                break;
            case ButtonState.Starting:
                if (isEditing) EditMenu.instance.ForceStart(index);
                else ViewMenu.instance.ForceStart(index); 
                break;
            case ButtonState.Active: 
                End();
                break;
            
            case ButtonState.Ending: State = ButtonState.Inactive; break;
        }
    }
}
