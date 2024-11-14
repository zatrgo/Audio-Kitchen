using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Misc;
using System.Linq;
public class EditMenu : MonoBehaviour {
    public static EditMenu instance;
    public GameObject panelPrefab;
    public List<ClipPanel> panels = new List<ClipPanel>();
    public RectTransform content;
    public RectTransform trackParent;
    public RectTransform panelParent;
    public TMP_Text title;
    public ColorButton teehee;
    public ColorButton play;

    float width;
    List<PlayButton> buttons = new List<PlayButton>();
    bool playing;
    int b_active;
    int b_next;
    bool switching;
    
    void Awake() {
        instance = this;
        GetComponent<Canvas>().enabled = false;
    }

    public void Open() {
        GetComponent<Canvas>().enabled = true;
        title.text = "Editing \"" + Chef.project.name + "\"";
        width = trackParent.sizeDelta.x - panelParent.sizeDelta.x;
        teehee.isPressed = true;
        play.isPressed = false;
        panels = new List<ClipPanel>();
        if (Chef.project.clips.Count <= 0) return;
        foreach (Clip clip in Chef.project.clips) {
            var newPanel = Instantiate(panelPrefab, panelParent).GetComponent<ClipPanel>();
            newPanel.Create(clip, panels.Count);
            panels.Add(newPanel);
        }
        Reorder();
    }

    public void SetPlaying() {
        playing = play.isPressed;
        transform.GetChild(0).GetChild(5).gameObject.SetActive(!playing);
        buttons.Clear();
        b_active = -1;
        b_next = -1;
        foreach (ClipPanel panel in panels) {
            panel.SetPlaying(playing);
            buttons.Add(panel.button);
            if (!playing) panel.button.State = ButtonState.Inactive;
        }
    }

    void Update() {

        content.sizeDelta = new Vector2(1600, trackParent.sizeDelta.y);
        if (playing && b_active != -1) {
            if (!switching) { // Not in the middle of a clip switch and not last clip
                if (b_next != -1) {
                    if (buttons[b_active].TimeLeft <= buttons[b_next].clip.fadeIn) { 
                        // Is there a next clip, and is it time for the next clip to start
                        buttons[b_next].PlayStart();
                        buttons[b_next].source.time = buttons[b_next].clip.FadeStart + (buttons[b_next].clip.fadeIn - buttons[b_active].TimeLeft);
                        switching = true;
                    }
                } 
                else {if (!(buttons[b_active].clip.isLoop || b_active >= buttons.Count-1)) b_next = b_active + 1;}
            }
            else {
                if (b_next != -1) {
                    if (buttons[b_next].State == ButtonState.Active) {
                        b_active = b_next;
                        switching = false;
                        if (buttons[b_active].clip.isLoop || b_active >= buttons.Count-1) b_next = -1;
                        else b_next += 1;
                    }

                } else switching = false;
            }
            bool nop = true;
            foreach (var button in buttons) if (button.State != ButtonState.Inactive) nop = false;
            if (nop) StopPlay();
        } else switching = false;
    }

     public void TryStart(int index) {if (!switching) {
        if (b_active != -1) {
            if (b_next != -1) buttons[b_next].State = ButtonState.Inactive;
            b_next = index;
            buttons[b_next].State = ButtonState.Starting;
            buttons[b_active].forceNext = true;
        }
        else {
            b_active = index;
            buttons[b_active].State = ButtonState.Starting;
            buttons[b_active].PlayStart();
            if (buttons[b_active].clip.isLoop || b_active >= buttons.Count-1) b_next = -1;
            else b_next = b_active + 1;

        }
    }}
    public void ForceStart(int index) {if (!switching) {
        if (b_active == index) return;
        buttons[index].State = ButtonState.Starting;
        if (b_active != -1) {
            buttons[b_active].End();
            b_active = index;
            buttons[b_active].PlayStart();
            if (buttons[index].clip.isLoop || b_active >= buttons.Count-1) b_next = -1;
            else b_next = index + 1;
        }
        else {
            b_active = index;
            buttons[b_active].PlayStart();
            if (buttons[index].clip.isLoop || b_active >= buttons.Count-1) b_next = -1;
            else b_next = index + 1;
        }
    }}
    public void TryStop() {
        if (b_next == -1) StopPlay();
    }
    public void StopPlay() {b_active = -1; b_next = -1; switching = false;}

    public void SetVolume(float n) {Chef.project.volume = n;}
    
    public void Zoom(bool zoomIn) {
        if (panels.Count <= 0) return;
        if (zoomIn) width *= 1.5f;
        else if (width > 1400) width /= 1.5f;
        foreach (var panel in panels) panel.SetSize(width);
    }

    public void Reorder() {
        panelParent.DetachChildren();
        trackParent.DetachChildren();

        foreach (var panel in panels) {
            panel.transform.SetParent(panelParent);
            panel.bar.transform.SetParent(trackParent);
        }
        foreach (var panel in panels) panel.SetButtonClips();
    }

    public void NewClip() {
        var newPanel = Instantiate(panelPrefab, panelParent).GetComponent<ClipPanel>();
        Clip newClip = new Clip("New Clip");
        Chef.project.clips.Add(newClip);
        newPanel.Create(newClip, panels.Count);
        newPanel.SetSize(width);
        panels.Add(newPanel);
        newPanel.SetButtonClips();

        if (panels.Count > 1) panels[panels.Count-2].SetButtonClips();
    }

    public void Save() {
        bool saved = Chef.SaveProject(out string path);
        if (saved) {
            Chef.project.name = path.Split('/').Last();
            title.text = "Saved at \"" + Chef.project.name + "\"";
        }
    }

    public void OpenNew() {
        if (Chef.OpenProject(out string path)) {
            StopPlay();
            for (int i = 0; i < trackParent.childCount; i++) {
                Destroy(trackParent.GetChild(i).gameObject);
                Destroy(panelParent.GetChild(i).gameObject);
            }
            Open();
        }
    }
    
    public void Return() {
        StopPlay();
        for (int i = 0; i < trackParent.childCount; i++) {
            Destroy(trackParent.GetChild(i).gameObject);
            Destroy(panelParent.GetChild(i).gameObject);
        }
        GetComponent<Canvas>().enabled = false;
        Chef.project = null;
        StartMenu.instance.Open();
    }

    public void ToView() {
        StopPlay();
        for (int i = 0; i < trackParent.childCount; i++) {
            Destroy(trackParent.GetChild(i).gameObject);
            Destroy(panelParent.GetChild(i).gameObject);
        }
        GetComponent<Canvas>().enabled = false;
        ViewMenu.instance.Open();
    }
    
}
