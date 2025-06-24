using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BusinessLevelData
{
    public string Id;
    public int Level;
}

[Serializable]
public class BusinessUpgradeData
{
    public string Id;
    public bool Upgrade1;
    public bool Upgrade2;
}

[Serializable]
public class BusinessProgressData
{
    public string Id;
    public float Progress;
}

[Serializable]
public class BusinessIncomeTimeData
{
    public string Id;
    public float Time;
}

[Serializable]
public class GameData
{
    public float SavedBalance;
    
    public List<BusinessLevelData> BusinessLevels = new();
    public List<BusinessUpgradeData> Upgrades = new();
    public List<BusinessProgressData> Progresses = new();
    public List<BusinessIncomeTimeData> IncomeTimes = new();
    
    [NonSerialized] public Transform BusinessesContainer;
    [NonSerialized] public GameObject BusinessViewPrefab;
    [NonSerialized] public Text BalanceText;

    private const string SaveKey = "GameSaveData";

    public void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            JsonUtility.FromJsonOverwrite(json, this);
            Debug.Log("Данные загружены: " + json);
        }
        else
        {
            const string firstBusinessId = "FirstBusiness"; 
        
            SetBusinessLevel(firstBusinessId, 1);
            SetUpgradePurchased(firstBusinessId, 1, false);
            SetUpgradePurchased(firstBusinessId, 2, false);
            SetBusinessProgress(firstBusinessId, 0f);
            SetLastIncomeTime(firstBusinessId, Time.time);
            SavedBalance = 0f;

            Debug.Log($"Инициализирован первый бизнес: {firstBusinessId}");
        }
        
        UpdateBalanceDisplay(SavedBalance);
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public float AddBusinessIncome(string businessId, float playerBalance, float income)
    {
        playerBalance += income;
        SetLastIncomeTime(businessId, Time.time);
        UpdateBalanceDisplay(playerBalance);
        Debug.Log($"Доход {income}$ от {businessId}. Баланс: {playerBalance}$");
        return playerBalance;
    }

    public void UpdateBalanceDisplay(float playerBalance)
    {
        if (BalanceText != null)
        {
            BalanceText.text = $"Баланс: {playerBalance:F1}$";
        }
    }

    public int GetBusinessLevel(string id)
    {
        var data = BusinessLevels.Find(x => x.Id == id);
        return data?.Level ?? 0;
    }

    public void SetBusinessLevel(string id, int level)
    {
        var data = BusinessLevels.Find(x => x.Id == id);
        if (data != null) data.Level = level;
        else BusinessLevels.Add(new BusinessLevelData { Id = id, Level = level });
    }

    public bool IsUpgradePurchased(string id, int upgradeNumber)
    {
        var data = Upgrades.Find(x => x.Id == id);
        return data != null && ((upgradeNumber == 1 && data.Upgrade1) || (upgradeNumber == 2 && data.Upgrade2));
    }

    public void SetUpgradePurchased(string id, int upgradeNumber, bool purchased = true)
    {
        var data = Upgrades.Find(x => x.Id == id);
        
        if (data == null)
        {
            data = new BusinessUpgradeData { Id = id };
            Upgrades.Add(data);
        }

        switch (upgradeNumber)
        {
            case 1:
                data.Upgrade1 = purchased;
                break;
            case 2:
                data.Upgrade2 = purchased;
                break;
        }
    }

    public float GetBusinessProgress(string id)
    {
        var data = Progresses.Find(x => x.Id == id);
        return data?.Progress ?? 0f;
    }

    public void SetBusinessProgress(string id, float progress)
    {
        var data = Progresses.Find(x => x.Id == id);
        
        if (data != null)
        {
            data.Progress = progress;
        }
        else
        {
            Progresses.Add(new BusinessProgressData { Id = id, Progress = progress });
        }
    }

    public float GetLastIncomeTime(string id)
    {
        var data = IncomeTimes.Find(x => x.Id == id);
        return data?.Time ?? Time.time;
    }

    public void SetLastIncomeTime(string id, float time)
    {
        var data = IncomeTimes.Find(x => x.Id == id);
        
        if (data != null)
        {
            data.Time = time;
        }
        else
        {
            IncomeTimes.Add(new BusinessIncomeTimeData { Id = id, Time = time });
        }
    }

    public bool IsBusinessPurchased(string id) => GetBusinessLevel(id) > 0;
}