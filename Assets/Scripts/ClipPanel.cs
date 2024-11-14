using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Misc;
using System;

public class ClipPanel : MonoBehaviour {
    Clip clip;
    public int index;
    AudioSource source;
    public GameObject barPrefab;
    public RectTransform bar;
    TMP_InputField clipName;
    ColorButton delete;
    ColorButton loop;
    ColorButton clipUp;
    ColorButton clipDown;
    TMP_InputField startField;
    TMP_InputField endField;
    TMP_InputField fadeInField;
    TMP_InputField fadeOutField;
    [NonSerialized]
    public PlayButton button;
    AudioClip Audio {get {return Chef.project.audio;} set {Chef.project.audio = value;}}

    RectTransform rectTransform {get {return GetComponent<RectTransform>();}}

    void Awake() {
        source = GetComponent<AudioSource>();

        clipName = transform.GetChild(1).GetComponent<TMP_InputField>();
        delete = transform.GetChild(2).GetComponent<ColorButton>();
        loop = transform.GetChild(3).GetComponent<ColorButton>();
        clipUp = transform.GetChild(4).GetComponent<ColorButton>();
        clipDown = transform.GetChild(5).GetComponent<ColorButton>();
        startField = transform.GetChild(6).GetComponent<TMP_InputField>();
        endField = transform.GetChild(7).GetComponent<TMP_InputField>();
        fadeInField = transform.GetChild(8).GetComponent<TMP_InputField>();
        fadeOutField = transform.GetChild(9).GetComponent<TMP_InputField>();
        button = transform.GetChild(11).GetComponent<PlayButton>();

    }

    public void Create(Clip clip, int index) {
        this.clip = clip;
        this.index = index;

        gameObject.name = $"Clip {index} Panel";
        
        bar = Instantiate(barPrefab, EditMenu.instance.trackParent).GetComponent<RectTransform>();
        bar.anchoredPosition = new Vector2(250, rectTransform.anchoredPosition.y);
        bar.name = $"Clip {index}";
        bar.GetComponent<RawImage>().texture = Chef.DrawWaveform(Audio, bar.sizeDelta);
        bar.GetComponent<RawImage>().color = Color.HSVToRGB(UnityEngine.Random.Range(0, 255f)/255, .3f, 1);

        clipName.text = clip.name;
        loop.isPressed = clip.isLoop;
        startField.text = Chef.SecondsToTime(clip.start);
        endField.text = Chef.SecondsToTime(clip.end);
        fadeInField.text = Chef.SecondsToTime(clip.fadeIn);
        fadeOutField.text = Chef.SecondsToTime(clip.fadeOut);
        
        SetClipStart();
        SetClipEnd();
        SetClipFadeIn();
        SetClipFadeOut();
        SetButtonClips();
    }

    void Update() {
        bar.anchoredPosition = new Vector2(bar.anchoredPosition.x, rectTransform.anchoredPosition.y);
    }

    public void SetClipName(string n) {clip.name = n;}
    public void SetVolume(float n) {clip.volume = n;}

    float BarPos(float n) {return bar.sizeDelta.x * (n / Audio.length);}

    public void SetButtonClips() {
        if (index == 0) clipUp.gameObject.SetActive(false); else clipUp.gameObject.SetActive(true);
        if (index == EditMenu.instance.panels.Count-1) clipDown.gameObject.SetActive(false); else clipDown.gameObject.SetActive(true);
    }

    public void SetClipStart() {
        float n = 0;
        try {n = Chef.TimeToSeconds(startField.text);}
# pragma warning disable CS0168 
        catch (Exception e) {}
        if (n < 0) n = 0;
        if (n > clip.end) n = clip.end;
        clip.start = n;
        bar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(BarPos(clip.start - clip.fadeIn), 200);
        startField.text = Chef.SecondsToTime(n);
    }

    public void SetClipEnd() {
        float n = Chef.TimeToSeconds(endField.text);
        if (n < clip.start || n > Audio.length) n = Audio.length;
        clip.end = n;
        bar.transform.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(BarPos(Audio.length - (clip.end + clip.fadeOut)), 200);
        endField.text = Chef.SecondsToTime(n);
    }

    public void SetClipFadeIn() {
        float n = Chef.TimeToSeconds(fadeInField.text);
        if (n < 0) n = 0;
        if (n > clip.start) n = clip.start;
        clip.fadeIn = n;
        bar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(BarPos(clip.start- clip.fadeIn), 200);
        bar.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(BarPos(clip.fadeIn), 200);
        bar.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(BarPos(clip.start - clip.fadeIn), 0);
        fadeInField.text = Chef.SecondsToTime(n);
    }

    public void SetClipFadeOut() {
        float n = Chef.TimeToSeconds(fadeOutField.text);
        if (n > Audio.length - clip.end) n = Audio.length - clip.end;
        clip.fadeOut = n;
        bar.transform.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(BarPos(Audio.length - (clip.end + clip.fadeOut)), 200);
        bar.transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(BarPos(clip.fadeOut), 200);
        bar.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(-BarPos(Audio.length - (clip.end + clip.fadeOut)), 0);
        fadeOutField.text = Chef.SecondsToTime(n);
    }


    public void SetSize(float size) {
        RectTransform rect = bar;
        rect.sizeDelta = new Vector2(size, 200);
        bar.GetComponent<RawImage>().texture = Chef.DrawWaveform(Audio, rect.sizeDelta);
        SetClipStart();
        SetClipEnd();
        SetClipFadeIn();
        SetClipFadeOut();
    }

    public void Shift(bool up) {
        int newIndex;
        if (up) newIndex = index-1;
        else newIndex = index+1;
        var otherPanel = EditMenu.instance.panels[newIndex];
        EditMenu.instance.panels[newIndex] = this;
        EditMenu.instance.panels[index] = otherPanel;
        otherPanel.index = index;
        index = newIndex;
        EditMenu.instance.Reorder();
    }

    public void SetLoop() {clip.isLoop = loop.isPressed;}

    public void SetPlaying(bool playing) {
        clipName.gameObject.SetActive(!playing);
        delete.gameObject.SetActive(!playing);
        loop.gameObject.SetActive(!playing);
        clipUp.gameObject.SetActive(!playing);
        clipDown.gameObject.SetActive(!playing);
        startField.gameObject.SetActive(!playing);
        endField.gameObject.SetActive(!playing);
        fadeInField.gameObject.SetActive(!playing);
        fadeOutField.gameObject.SetActive(!playing);
        
        button.gameObject.SetActive(playing);
        button.Create(clip, index, true);
        if (!playing) SetButtonClips();
    }
    public void Delete() {
        Destroy(bar.gameObject);
        EditMenu.instance.panels.RemoveAt(index);
        Chef.project.clips.RemoveAt(index);
        EditMenu.instance.Reorder();
        Destroy(gameObject);
    }

}
