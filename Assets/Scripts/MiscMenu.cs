using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiscMenu : MonoBehaviour
{
    public static MiscMenu instance;
    public TMP_Text logText;
    public TMP_Text errorText;
    Dictionary<int, string> logs = new Dictionary<int, string>();
    float time = 5;
    bool debugging = true;

    void Awake() {
        instance = this;
        logText.text = "";
        errorText.text = "";
    }

    void Update() {
        time += Time.deltaTime;
        if (time > 5) errorText.text = "";
    }

    public void WriteError(string text) {if (debugging) {
        errorText.text = text;
        time = 0;
    }}

    public void Log(int key, string text) {if (debugging) {
        logs[key] = text;

        text = "";
        foreach (var log in logs.Values) text += log + "\n";
        logText.text = text;
    }}
}
