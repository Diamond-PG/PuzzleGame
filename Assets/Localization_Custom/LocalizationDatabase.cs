using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LocalizationDatabase",
    menuName = "Localization/Localization Database"
)]
public class LocalizationDatabase : ScriptableObject
{
    public List<LocalizationEntry> entries = new();
}

[Serializable]
public class LocalizationEntry
{
    public string key;

    [TextArea] public string english;
    [TextArea] public string russian;
    [TextArea] public string hebrew;
    [TextArea] public string arabic;
    [TextArea] public string spanish;
    [TextArea] public string french;
    [TextArea] public string chinese;
}