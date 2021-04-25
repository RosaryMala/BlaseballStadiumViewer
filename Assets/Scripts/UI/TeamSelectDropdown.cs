using Newtonsoft.Json;
using QuickType;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class TeamSelectDropdown : MonoBehaviour
{
    Dropdown dropdown = null;
    private List<Stadium> stadiumList;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();


        StartCoroutine(TeamLoader());
    }

    IEnumerator TeamLoader()
    {
        dropdown.options.Clear();

        UnityWebRequest stadiumRequest = UnityWebRequest.Get("https://api.sibr.dev/chronicler/v1/stadiums");
        yield return stadiumRequest.SendWebRequest();

        if(stadiumRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(stadiumRequest.error);
        }
        else
        {
            SibrStadiumList stadiumImport = JsonConvert.DeserializeObject<SibrStadiumList>(stadiumRequest.downloadHandler.text);
            stadiumList = stadiumImport.Data;

            foreach (var stadium in stadiumList)
            {

            }
        }

        yield return null;
    }
}
