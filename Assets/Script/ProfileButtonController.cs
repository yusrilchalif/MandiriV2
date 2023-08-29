using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileButtonController : MonoBehaviour
{
    [SerializeField] GameObject profileMenu;

    // Start is called before the first frame update
    void Start()
    {
        profileMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleActiveMenu()
    {
        profileMenu.SetActive(!profileMenu.activeSelf);
    }
}
