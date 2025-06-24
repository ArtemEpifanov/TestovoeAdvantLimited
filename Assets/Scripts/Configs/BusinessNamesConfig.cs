using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/BusinessNamesConfig")]
public class BusinessNamesConfig : ScriptableObject
{
    [Serializable]
    public struct BusinessNames
    {
        public string Id;
        public string BusinessName;
        public string Upgrade1Name;
        public string Upgrade2Name;
    }

    public BusinessNames[] Names;
}