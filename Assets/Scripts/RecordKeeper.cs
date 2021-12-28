using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordKeeper : MonoBehaviour
{

    public static RecordKeeper Instance;

    Dictionary<Person, List<Person>> Characters = new Dictionary<Person, List<Person>>();
    Dictionary<Building, List<Building>> Buildings = new Dictionary<Building, List<Building>>();
    Dictionary<IdlePoint, List<IdlePoint>> IdlePoints = new Dictionary<IdlePoint, List<IdlePoint>>(); //doesnt need to be a dictionay but for the symmetry's sake it is

    void Awake()
    {
        Instance = this;
    }

    #region Person(al) Methods
    public void AddPerson<T>(GameObject characterObj) where T : Person
    {
        var npc = characterObj.GetComponent<T>();

        bool found = false;
        foreach (var charList in Characters)
            if (charList.Key is T)
                found = true;

        if (!found)
            Characters.Add(npc, new List<Person>());

        foreach (var charList in Characters)
            if (charList.Key is T)
                charList.Value.Add(npc);
    }
    public void RemovePerson<T>(Person person) where T : Person
    {
        foreach (var charList in Characters)
            if (charList.Key is T && charList.Value.Contains(person))
                charList.Value.Remove(person);
    }
    public List<Person> GetPeople<T>() where T : Person 
    {
        foreach (var charList in Characters)
            if (charList.Key is T)
                return charList.Value;
        return null;
    }
    public List<Person> GetEveryone()
    {
        List<Person> everyone = new List<Person>();

        foreach (var characterList in Characters)
            foreach (Person character in characterList.Value)
                everyone.Add(character);

        return everyone;
    }
    #endregion
    #region Building Methods
    public void AddBuilding<T>(GameObject buildingObj) where T : Building
    {
        var building = buildingObj.GetComponent<T>();

        bool found = false;
        foreach (var buildingList in Buildings)
            if (buildingList.Key is T)
                found = true;

        if (!found)
            Buildings.Add(building, new List<Building>());

        foreach (var buildingList in Buildings)
            if (buildingList.Key is T)
                buildingList.Value.Add(building);
    }
    public void RemoveBuilding<T>(Building building) where T : Building
    {
        foreach (var buildingList in Buildings)
            if (buildingList.Key is T)
                if (buildingList.Value.Contains(building))
                    buildingList.Value.Remove(building);
    }
    public List<Building> GetBuildings<T>() where T : Building
    {
        foreach (var buildingList in Buildings)
            if (buildingList.Key is T)
                return buildingList.Value;
        return null;
    }
    #endregion
    #region IdlePoint Methods
    public void AddIdlePoint<T>(GameObject pointObj) where T : IdlePoint
    {
        var idlePoint = pointObj.GetComponent<T>();

        bool found = false;
        foreach (var pointsList in IdlePoints)
            if (pointsList.Key is T)
                found = true;

        if (!found)
            IdlePoints.Add(idlePoint, new List<IdlePoint>());

        foreach (var pointsList in IdlePoints)
            if (pointsList.Key is T)
                pointsList.Value.Add(idlePoint);
    }
    public void RemoveIdlePoint<T>(IdlePoint idlePoint) where T : IdlePoint
    {
        foreach (var pointsList in IdlePoints)
            if (pointsList.Key is T)
                if (pointsList.Value.Contains(idlePoint))
                    pointsList.Value.Remove(idlePoint);
    }
    public List<IdlePoint> GetIdlePoints<T>() where T : IdlePoint
    {
        foreach (var pointsList in IdlePoints)
            if (pointsList.Key is T)
                return pointsList.Value;
        return null;
    }
    #endregion
}
