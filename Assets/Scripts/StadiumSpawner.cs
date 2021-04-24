using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickType;
using Newtonsoft.Json;
using System;

[RequireComponent(typeof(ProceduralField))]
public class StadiumSpawner : MonoBehaviour
{
    [SerializeField]
    public List<Stadium> stadiums;
    public int selectedStadium = 0;
    public List<string> stadiumOptions = new List<string>();

    ProceduralField field;
    private void Awake()
    {
        field = GetComponent<ProceduralField>();
    }

    public void LoadStadium(StadiumData data)
    {
        if(field == null)
            field = GetComponent<ProceduralField>();
        field.LoadStadium(data);
    }
}
