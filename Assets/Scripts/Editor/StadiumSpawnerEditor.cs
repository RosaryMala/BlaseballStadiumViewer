using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Threading;
using QuickType;
using Newtonsoft.Json;

[CustomEditor(typeof(StadiumSpawner))]
public class StadiumSpawnerEditor : Editor
{
    int selectedStadium = 0;
    List<string> stadiumOptions = new List<string>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        StadiumSpawner stadiumSpawner = target as StadiumSpawner;

        if (GUILayout.Button("Refresh Stadiums"))
        {

            UnityWebRequest webRequest = UnityWebRequest.Get("https://api.sibr.dev/chronicler/v1/stadiums");

            webRequest.SendWebRequest();

            while(!webRequest.isDone)
            {
                ///wait.
                Thread.Sleep(50);
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                SibrStadiumList stadiumList = JsonConvert.DeserializeObject<SibrStadiumList>(webRequest.downloadHandler.text);
                stadiumSpawner.stadiums = stadiumList.Data;
                stadiumOptions.Clear();
                foreach (var stadium in stadiumSpawner.stadiums)
                {
                    stadiumOptions.Add(string.Format("{0}, ({1})", stadium.Data.Name, stadium.Data.Nickname));
                }
                stadiumSpawner.LoadStadium(stadiumSpawner.stadiums[selectedStadium].Data);
            }
            else
            {
                Debug.Log(webRequest.error);
            }
        }
        if (stadiumSpawner.stadiums != null)
        {
            if(stadiumOptions.Count != stadiumSpawner.stadiums.Count)
            {
                stadiumOptions.Clear();
                foreach (var stadium in stadiumSpawner.stadiums)
                {
                    stadiumOptions.Add(string.Format("{0}, ({1})", stadium.Data.Name, stadium.Data.Nickname));
                }
            }
            EditorGUI.BeginChangeCheck();
            selectedStadium = EditorGUILayout.Popup(selectedStadium, stadiumOptions.ToArray());
            if(EditorGUI.EndChangeCheck())
            {
                Debug.Log(stadiumOptions[selectedStadium]);
                stadiumSpawner.LoadStadium(stadiumSpawner.stadiums[selectedStadium].Data);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
