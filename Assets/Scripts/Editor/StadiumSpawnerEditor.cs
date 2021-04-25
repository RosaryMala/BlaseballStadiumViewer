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
                stadiumSpawner.stadiumOptions.Clear();
                foreach (var stadium in stadiumSpawner.stadiums)
                {
                    UnityWebRequest teamRequest = UnityWebRequest.Get("https://www.blaseball.com/database/team?id=" + stadium.Data.TeamID);
                    teamRequest.SendWebRequest();
                    while (!teamRequest.isDone)
                        Thread.Sleep(50);
                    TeamData team = null;
                    if(teamRequest.result == UnityWebRequest.Result.Success)
                    {
                        team = JsonConvert.DeserializeObject<TeamData>(teamRequest.downloadHandler.text);
                    }
                    string stadiumLabel;
                    if(team != null)
                    {
                        stadiumLabel = string.Format("{0}, ({1}), {2}", stadium.Data.Name, stadium.Data.Nickname, team.FullName);
                    }
                    else
                    {
                        stadiumLabel = string.Format("{0}, ({1})", stadium.Data.Name, stadium.Data.Nickname);
                    }

                    stadiumSpawner.stadiumOptions.Add(stadiumLabel);
                }
                stadiumSpawner.LoadStadium(stadiumSpawner.stadiums[stadiumSpawner.selectedStadium].Data);
            }
            else
            {
                Debug.Log(webRequest.error);
            }
        }
        if (stadiumSpawner.stadiums != null)
        {
            if(stadiumSpawner.stadiumOptions.Count != stadiumSpawner.stadiums.Count)
            {
                stadiumSpawner.stadiumOptions.Clear();
                foreach (var stadium in stadiumSpawner.stadiums)
                {
                    stadiumSpawner.stadiumOptions.Add(string.Format("{0}, ({1})", stadium.Data.Name, stadium.Data.Nickname));
                }
            }
            EditorGUI.BeginChangeCheck();
            stadiumSpawner.selectedStadium = EditorGUILayout.Popup(stadiumSpawner.selectedStadium, stadiumSpawner.stadiumOptions.ToArray());
            if(EditorGUI.EndChangeCheck())
            {
                Debug.Log(stadiumSpawner.stadiumOptions[stadiumSpawner.selectedStadium]);
                stadiumSpawner.LoadStadium(stadiumSpawner.stadiums[stadiumSpawner.selectedStadium].Data);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
