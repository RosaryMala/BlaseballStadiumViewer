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
#if UNITY_WEBGL && !UNITY_EDITOR
    const string BasePoint = "https://api.sibr.dev/proxy/";
#else
    const string BasePoint = "https://www.blaseball.com/";
#endif


    Dropdown dropdown = null;
    private List<Stadium> stadiumList;

    public ProceduralField field;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();


        StartCoroutine(TeamLoader());
    }

    public void SelectionCallback()
    {
        field.LoadStadium(stadiumList[dropdown.value].Data);
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
                UnityWebRequest teamRequest = UnityWebRequest.Get(BasePoint + "database/team?id=" + stadium.Data.TeamID);
                yield return teamRequest.SendWebRequest();

                string stadiumName;

                if (teamRequest.result == UnityWebRequest.Result.Success)
                {
                    var team = JsonConvert.DeserializeObject<TeamData>(teamRequest.downloadHandler.text);
                    stadiumName = string.Format("{0} ({1}), {2}", stadium.Data.Name, stadium.Data.Nickname, team.FullName);
                }
                else
                {
                    Debug.LogError(teamRequest.error + " at url " + teamRequest.url);
                    stadiumName = string.Format("{0} ({1})", stadium.Data.Name, stadium.Data.Nickname);
                }

                Dropdown.OptionData stadiumOption = new Dropdown.OptionData(stadiumName);

                dropdown.options.Add(stadiumOption);
                yield return null;
            }
            dropdown.SetValueWithoutNotify(1);
            dropdown.value = 0;
        }

        yield return null;
    }
}
