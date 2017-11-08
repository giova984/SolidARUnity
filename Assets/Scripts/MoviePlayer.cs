using System;
using System.IO;
using UnityEngine;
using System.Collections;
 
[RequireComponent(typeof(AudioSource))]
public class MoviePlayer : MonoBehaviour
{
    public string file;
    public MovieTexture movieTexture;
    
    protected bool streamReady = false;

    void Start()
    {
        file = "file://" + Path.Combine(Application.streamingAssetsPath, file);
        StartCoroutine(StartStream(file));
    }

    protected IEnumerator StartStream(String url)
    {
        WWW videoStreamer = new WWW(url);
        Debug.Log("Asset: " + url + " size:" + videoStreamer.bytesDownloaded);
        movieTexture = videoStreamer.movie;
        //audio.clip = movieTexture.audioClip;

        while (!movieTexture.isReadyToPlay)
        {
            yield return 0;
        }

        //audio.Play();
        movieTexture.Play();
        streamReady = true;
    }

    void OnGUI()
    {
        if (streamReady)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.height, Screen.width), movieTexture);
        }
    }
}
