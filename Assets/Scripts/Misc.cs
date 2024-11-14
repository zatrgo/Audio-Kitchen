using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using SFB;

#pragma warning disable CS0168

namespace Misc {

    public enum ButtonState {Inactive, Active, Starting, Ending}
    public class Project {
        public string name;
        public AudioClip audio;
        public List<Clip> clips = new List<Clip>();
        public float volume = 1;

        public Project(string name = "New Project") {this.name=name;}
    }
    [Serializable]
    public class SerialProject {
        public string name;
        public List<Clip> clips;
        public float[] data;
        public int samples;
        public int channels;
        public int frequency;
        public SerialProject(string name, float[] data, List<Clip> clips) {
            this.name=name;
            this.data=data;
            this.clips=clips;
        }
    }

    [Serializable]
    public class Clip {
        public string name;
        public float volume = 1;
        public bool isLoop = false;
        public float start = 0;
        public float end;
        public float fadeIn = 0;
        public float fadeOut = 0;
        public Clip(string name) {
            this.name = name;
            end = Chef.project.audio.length;
        }

        public float FadeStart {get {return start - fadeIn;}}
        public float FadeEnd {get {return end + fadeOut;}}
        public float time {get {return end - start;}}
    }

    public static class Chef {
        public static Project project;
        public static string SecondsToTime(float input) {
            if (input < 60) return $"{input:00.000}";
            int minutes = 0;
            while (input >= 60) {input -= 60; minutes += 1;}
            return $"{minutes:00}:{input:00.000}";
        }
        public static float TimeToSeconds(string input) {
            if (input == "") return -1;
            if (input.Contains(':')) return (float.Parse(input.Split(':')[0]) * 60) + float.Parse(input.Split(':')[1]);
            else return float.Parse(input);
        }

        public static T SelectByEnum<E, T>(E selector, params T[] items) where E: Enum {
            if (Enum.GetValues(typeof(E)).Length != items.Length) 
                throw new ArgumentOutOfRangeException($"The number of values in enum \"{typeof(E).Name}\" must match the number of items supplied.");
            return items[Convert.ToInt32(selector)];
        }
        public static float Diff(float min, float n, float max) {
            return (n - min) / (max - min);
        }

        public static bool SaveProject(out string path) {
            path = StandaloneFileBrowser.SaveFilePanel("Save Project As...", "", "New Project", new []{new ExtensionFilter("Audio Kitchen Project", "akproj")});
            try {
                FileStream stream = new FileStream(path, FileMode.Create);
                float[] samples = new float[project.audio.samples * project.audio.channels];
                project.name = path.Split('/').Last();
                project.audio.GetData(samples, 0);
                var newproject = new SerialProject(project.name, samples, project.clips);
                newproject.samples = project.audio.samples;
                newproject.channels = project.audio.channels;
                newproject.frequency = project.audio.frequency;
                new BinaryFormatter().Serialize(stream, newproject);
                stream.Close();
                return true;
            } catch (Exception e) {return false;}

        }
        public static bool OpenProject(out string path) {
            
            path = StandaloneFileBrowser.OpenFilePanel("Select Project File...", "", new []{new ExtensionFilter("Audio Kitchen Project", "akproj")}, false)[0];
            try {
                FileStream stream = new FileStream(path, FileMode.Open);
                var newproject = new BinaryFormatter().Deserialize(stream) as SerialProject;
                project = new Project(newproject.name);
                project.audio = AudioClip.Create(newproject.name, newproject.samples, newproject.channels, newproject.frequency, false);
                project.audio.SetData(newproject.data, 0);
                project.clips = newproject.clips;
                stream.Close();
                return true;
            } catch (Exception e) {return false;}
        }

        public static void ErrorFile(string path) {
            MiscMenu.instance.WriteError($"ERROR: File \"{path}\" failed to upload. Try resaving the file in an audio editor, or use a different file type.");
        }
        public static void ErrorConnection() {

        }

        public static IEnumerator GetAudioFromFile(string path, Action<AudioClip> next, Action connectionError = null, Action<string> fileError = null) {
            if (connectionError == null) connectionError = ErrorConnection;
            if (fileError == null) fileError = ErrorFile;
            AudioType type;
            if (path.EndsWith("mp3")) type = AudioType.MPEG;
            else type = AudioType.WAV;

            UnityWebRequest www = null;
            bool requested = true;
            try {www = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, type);} 
            catch (Exception e) {requested = false;}
            if (requested) {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError) connectionError();
                else {
                    AudioClip audio = DownloadHandlerAudioClip.GetContent(www);
                    audio.name = path.Split("/").Last();
                    if (audio.samples <= 0) fileError(path);
                    else {
                        next(audio);
                    }
                }
            }
        }

        public static Texture2D DrawWaveform(AudioClip clip, Vector2 size) {
            if(clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();
            // get sample data
            int totalSamples = clip.samples * clip.channels;
            float[] samples = new float[totalSamples];
            if (clip.GetData(samples, 0) == false)
                return null;
            // setup texture
            int width = (int) size.x;
            int height = (int) size.y;
            Texture2D texture = new Texture2D(width, height);
            // get sampleIntensity
            // OVER HERE MIGHT BE CAUSING THE ISSUE
            int resolution = totalSamples / width;
            // draw color arracy per pixel in width
            Color[] colors = new Color[height];
            float midHeight = height / 2f;
            float sampleComp = 0f;
            for (int i = 0; i < width; i++) {
                // get wave intensity
                float sampleChunk = 0;
                for (int ii = 0; ii < resolution; ii++)
                    sampleChunk += Mathf.Abs(samples[(i * resolution) + ii]);
                sampleChunk = sampleChunk / resolution * 1.5f;
                for (int h = 0; h < height; h++) {
                    // get value of height relative to totalHeight
                    if (h < midHeight)
                        sampleComp = Mathf.InverseLerp(midHeight, 0, h);
                    else
                        sampleComp = Mathf.InverseLerp(midHeight, height, h);
                    // corralate to sample height
                    if (sampleComp > sampleChunk) colors[h] = Color.clear;
                    else colors[h] = Color.white;
                }
                // set pixels
                texture.SetPixels(i, 0, 1, height, colors);
            }
            // push to graphics card
            texture.Apply();
            // return finished texture
            return texture;
        }
    }

}