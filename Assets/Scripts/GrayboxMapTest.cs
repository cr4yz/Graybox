using Graybox;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GrayboxMapTest : MonoBehaviour
{

    private string _testFilePath;

    private void Start()
    {
        gb_Map.Create("Test Map!");
        _testFilePath = Path.Combine(Application.streamingAssetsPath, "Maps", "Test");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gb_Map.ActiveMap.Save(_testFilePath);
            Debug.Log("MAP SAVED: " + gb_Map.ActiveMap.FilePath);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gb_Map.Load(_testFilePath);
        }
    }
}
