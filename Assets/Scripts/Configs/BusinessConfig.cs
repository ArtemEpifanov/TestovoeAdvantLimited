using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/BusinessConfig")]
public class BusinessConfig : ScriptableObject
{
    [Serializable]
    public class  BusinessData
    {
        public string Id;
        public float BaseIncome;
        public float BaseCost;
        public float IncomeDelay;
        public UpgradeData Upgrade1;
        public UpgradeData Upgrade2;
    }

    [Serializable]
    public struct UpgradeData
    {
        public float Cost;
        public float IncomeMultiplier;
    }

    public BusinessData[] Businesses;
}