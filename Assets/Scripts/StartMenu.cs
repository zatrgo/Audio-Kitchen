using UnityEngine;
using SFB;
using Misc;
using System.Linq;

public class StartMenu : MonoBehaviour {

    public static StartMenu instance;
    bool gettingAudio;

    void Awake() {
        instance = this;
        GetComponent<Canvas>().enabled = true;
    }

    public void Open() { GetComponent<Canvas>().enabled = true;}
    

    public void NewProject() {
        string path = StandaloneFileBrowser.OpenFilePanel("Select Audio File...", "", new []{new ExtensionFilter("Audio Files", "mp3", "wav")}, false)[0];
        StartCoroutine(Chef.GetAudioFromFile(path, CreateProject));
    }

    public void CreateProject(AudioClip audio) {
        Chef.project = new Project();
        Chef.project.audio = audio;
        GetComponent<Canvas>().enabled = false;
        EditMenu.instance.Open();
    }

    public void ViewProject() {
        if (Chef.OpenProject(out string path)) {
            GetComponent<Canvas>().enabled = false;
            ViewMenu.instance.Open();
        } else MiscMenu.instance.WriteError("Couldn't open \"" + path.Split('/').Last() + "\". The file may have been corrupted.");
    }
    public void EditProject() {
        if (Chef.OpenProject(out string path)) {
            GetComponent<Canvas>().enabled = false;
            EditMenu.instance.Open();
        } else MiscMenu.instance.WriteError("Couldn't edit \"" + path.Split('/').Last() + "\". The file may have been corrupted.");
    }    
}
