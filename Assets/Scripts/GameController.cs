using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public Camera MainCamera;
    [Space(10)]
    public TextMeshProUGUI PausedText;
    public GameObject Button;
    public GameObject StatusArea;
    [Space(10)]
    public GameObject EntryPrefab;
    public List<GameObject> Entries;
    [Space(10)]
    public List<Person> Characters;

    private void Start()
    {
        Entries = new List<GameObject>();

        Person[] npcs = FindObjectsOfType<Person>();
        foreach (Person npc in npcs)
        {
            Characters.Add(npc);
            GameObject newEntry = Instantiate(EntryPrefab, StatusArea.transform) as GameObject;
            newEntry.transform.Find("Name Text").GetComponent<TextMeshProUGUI>().text = npc.name;
            newEntry.transform.Find("Money Text").GetComponent<TextMeshProUGUI>().text = npc.Money.ToString();
            Entries.Add(newEntry);
        }
    }
    public void StartTheMatch()
    {
        StartCoroutine(startMatch());
    }
    public void PauseTheMatch()
    {
        foreach (Person npc in Characters)
            npc.Sleep(false);
        PausedText.gameObject.SetActive(true);
    }
    public void ResumeTheMatch()
    {
        foreach (Person npc in Characters)
            npc.WakeUp();
        PausedText.gameObject.SetActive(false);
    }
    IEnumerator startMatch()
    {
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForSeconds(1f / 60f);
            MainCamera.transform.localPosition = new Vector3(0f, 120 - (22f * i / 60f), 0f);
        }
        foreach (Person npc in Characters)
            npc.Incarnate();
        StatusArea.SetActive(true);
        StartCoroutine(updateStatus());
    }
    IEnumerator updateStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            Entries = new List<GameObject>();

            Person[] npcs = FindObjectsOfType<Person>();

            GameObject[] statusAreaChildren = GameObject.FindGameObjectsWithTag("StatusUI");
            foreach (GameObject child in statusAreaChildren)
                Destroy(child.gameObject);

            foreach (Person npc in npcs)
            {
                Characters.Add(npc);
                GameObject newEntry = Instantiate(EntryPrefab, StatusArea.transform) as GameObject;
                newEntry.transform.Find("Name Text").GetComponent<TextMeshProUGUI>().text = npc.name;
                newEntry.transform.Find("Money Text").GetComponent<TextMeshProUGUI>().text = npc.Money.ToString();
                Entries.Add(newEntry);
            }
        }
    }
    private void Update()
    {

    }
}