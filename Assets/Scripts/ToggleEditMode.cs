using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleEditMode : MonoBehaviour
{

    public GameObject GrayboxRoot;
    public GameObject CharacterRoot;

    private bool _playing;

    private void Start()
    {
        EnablePlayMode(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnablePlayMode(!_playing);
        }
    }

    private void EnablePlayMode(bool enabled)
    {
        _playing = enabled;
        GrayboxRoot.gameObject.SetActive(!enabled);
        CharacterRoot.gameObject.SetActive(enabled);
    }

}
