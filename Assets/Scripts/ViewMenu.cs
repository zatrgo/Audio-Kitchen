using System.Collections.Generic;
using UnityEngine;
using Misc;
using TMPro;

public class ViewMenu : MonoBehaviour
{
    public static ViewMenu instance;
    public TMP_Text title;
    public ColorButton teehee;
    public RectTransform parent;
    public GameObject buttonPrefab;
    public List<PlayButton> buttons = new List<PlayButton>();
    int b_active = -1;
    int b_next = -1;
    bool switching;

    void Awake() {
        instance = this;
        GetComponent<Canvas>().enabled = false;
    }

    public void Open() {
        GetComponent<Canvas>().enabled = true;
        title.text = "Viewing \"" + Chef.project.name + "\"";
        teehee.isPressed = false;
        buttons = new List<PlayButton>();
        foreach (Clip clip in Chef.project.clips) {
            var newButton = Instantiate(buttonPrefab, parent).GetComponent<PlayButton>();
            newButton.Create(clip, buttons.Count, false);
            buttons.Add(newButton);
        }
        StopPlay();
    }
    void Update() {

        if (b_active != -1) {
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
    
    public void OpenNew() {
        if (Chef.OpenProject(out _)) {
            StopPlay();
            for (int i = 0; i < parent.childCount; i++) {
                Destroy(parent.GetChild(i).gameObject);
            }
            Open();
        }
    }

    public void Return() {
        StopPlay();
        for (int i = 0; i < parent.childCount; i++) {
            Destroy(parent.GetChild(i).gameObject);
        }
        GetComponent<Canvas>().enabled = false;
        Chef.project = null;
        StartMenu.instance.Open();
    }
    public void ToEdit() {
        StopPlay();
        for (int i = 0; i < parent.childCount; i++) {
            Destroy(parent.GetChild(i).gameObject);
        }
        GetComponent<Canvas>().enabled = false;
        EditMenu.instance.Open();
    }
}
