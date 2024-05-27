using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class importWorldManager : MonoBehaviour
{

    public TMP_InputField roomIINPUTFIELD;
    public TMP_Dropdown mapDropdown;

    private List<string> mapNames = new List<string>();
    private List<string> mapPaths = new List<string>();

    public Launcher launcherScript;

    public void updateDropdown(List<string> _mapNames, List<string> _mapPaths)
    {
        mapNames = _mapNames;
        mapPaths = _mapPaths;

        mapDropdown.options.Clear();
        mapDropdown.AddOptions(mapNames);

    }




    // Start is called before the first frame update
    void Start()
    {
        mapDropdown.onValueChanged.AddListener(OnMapDropdownValueChanged);
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnMapDropdownValueChanged(int value)
    {
        Debug.Log("Map Dropdown Value Changed: " + mapNames[value]);



    }

    public void submitNewMap()
    {
        Debug.Log("Submit New Map");

        Debug.Log("Map Name: " + roomIINPUTFIELD.text);
        Debug.Log("Map Path: " + mapPaths[mapDropdown.value]);



        launcherScript.startMonaGame(roomIINPUTFIELD.text, mapPaths[mapDropdown.value]);


    }


}
