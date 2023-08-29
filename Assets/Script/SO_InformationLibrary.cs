using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Trivia Library", menuName = "Data/Trivia Library")]
public class SO_InformationLibrary : ScriptableObject
{
    public List<Information> infoLibrary;
}

[Serializable]
public class Information{
    [TextArea] public string info;
}