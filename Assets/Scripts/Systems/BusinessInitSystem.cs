using Leopotam.EcsLite;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BusinessInitSystem : IEcsInitSystem
{
    private readonly BusinessConfig _businessConfig;
    private readonly BusinessNamesConfig _namesConfig;
    private readonly GameData _gameData;

    public BusinessInitSystem(BusinessConfig businessConfig, BusinessNamesConfig namesConfig, GameData gameData)
    {
        _businessConfig = businessConfig;
        _namesConfig = namesConfig;
        _gameData = gameData;
    }

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var balancePool = world.GetPool<PlayerBalance>();
        var businessPool = world.GetPool<Business>();
        var progressPool = world.GetPool<IncomeProgress>();
        var viewPool = world.GetPool<BusinessView>();
        var upgradePool = world.GetPool<Upgrade>();
        
        var balanceEntity = world.NewEntity();
        ref var balance = ref balancePool.Add(balanceEntity);
        balance.Value = _gameData.SavedBalance;
        
        foreach (var config in _businessConfig.Businesses)
        {
            var entity = world.NewEntity();
            
            ref var business = ref businessPool.Add(entity);
            business.Id = config.Id;
            business.Level = _gameData.GetBusinessLevel(config.Id);
            business.UpgradeOnePurchased = _gameData.IsUpgradePurchased(config.Id, 1);
            business.UpgradeTwoPurchased = _gameData.IsUpgradePurchased(config.Id, 2);
            UpdateBusinessIncome(ref business, config);
            
            ref var progress = ref progressPool.Add(entity);
            progress.Progress = _gameData.GetBusinessProgress(config.Id);
            progress.TimePassed = progress.Progress * config.IncomeDelay;
            
            ref var view = ref viewPool.Add(entity);
            var viewObj = Object.Instantiate(_gameData.BusinessViewPrefab, _gameData.BusinessesContainer);
            
            view.NameText = viewObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            view.LevelText = viewObj.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
            view.IncomeText = viewObj.transform.Find("IncomeText").GetComponent<TextMeshProUGUI>();
            view.ProgressBar = viewObj.transform.Find("ProgressBarBack/ProgressBarFront").GetComponent<Image>();
            view.LevelUpPriceText = viewObj.transform.Find("LevelUpButton/PriceText").GetComponent<Text>();
            view.LevelUpButton = viewObj.transform.Find("LevelUpButton").GetComponent<Button>();
            view.Upgrade1Text = viewObj.transform.Find("Upgrade1Button/UpgradeText").GetComponent<Text>();
            view.Upgrade1Button = viewObj.transform.Find("Upgrade1Button").GetComponent<Button>();
            view.Upgrade2Text = viewObj.transform.Find("Upgrade2Button/UpgradeText").GetComponent<Text>();
            view.Upgrade2Button = viewObj.transform.Find("Upgrade2Button").GetComponent<Button>();
            
            UpdateBusinessView(entity, balance.Value, world);
            
            if (!business.UpgradeOnePurchased)
            {
                var upgrade1Entity = world.NewEntity();
                ref var upgrade1 = ref upgradePool.Add(upgrade1Entity);
                upgrade1.BusinessId = config.Id;
                upgrade1.UpgradeNumber = 1;
            }

            if (!business.UpgradeTwoPurchased)
            {
                var upgrade2Entity = world.NewEntity();
                ref var upgrade2 = ref upgradePool.Add(upgrade2Entity);
                upgrade2.BusinessId = config.Id;
                upgrade2.UpgradeNumber = 2;
            }
        }
    }

    private BusinessNamesConfig.BusinessNames GetNamesForBusiness(string id)
    {
        foreach (var names in _namesConfig.Names)
        {
            if (names.Id == id) return names;
        }
        return default;
    }

    private void UpdateBusinessIncome(ref Business business, BusinessConfig.BusinessData config)
    {
        float multiplier = 1f;

        if (business.UpgradeOnePurchased)
        {
            multiplier += config.Upgrade1.IncomeMultiplier / 100f;
        }

        if (business.UpgradeTwoPurchased)
        {
            multiplier += config.Upgrade2.IncomeMultiplier / 100f;
        }
        
        business.CurrentIncome = business.Level * config.BaseIncome * multiplier;
    }

    private void UpdateBusinessView(int entity, float playerBalance, EcsWorld world)
    {
        var businessPool = world.GetPool<Business>();
        var viewPool = world.GetPool<BusinessView>();
        var config = GetBusinessConfig(ref businessPool.Get(entity));

        ref var business = ref businessPool.Get(entity);
        ref var view = ref viewPool.Get(entity);

        var names = GetNamesForBusiness(business.Id);
        
        view.NameText.text = names.BusinessName;
        view.LevelText.text = $"Lvl\n{_gameData.GetBusinessLevel(business.Id)}";
        view.IncomeText.text = $"Доход\n{business.CurrentIncome:F1}$";
        
        float levelUpPrice = (business.Level + 1) * config.BaseCost;

        view.LevelUpPriceText.text = $"LVL UP:\nЦена: {levelUpPrice}$";
        view.LevelUpButton.interactable = playerBalance >= levelUpPrice;
        
        if (business.UpgradeOnePurchased)
        {
            view.Upgrade1Text.text = "Purchased";
            view.Upgrade1Button.interactable = false;
        }
        else
        {
            view.Upgrade1Text.text = $"{names.Upgrade1Name}\nДоход: +{config.Upgrade1.IncomeMultiplier}%\nЦена: {config.Upgrade1.Cost}$";
            view.Upgrade1Button.interactable = playerBalance >= config.Upgrade1.Cost;
        }

        if (business.UpgradeTwoPurchased)
        {
            view.Upgrade2Text.text = "Purchased";
            view.Upgrade2Button.interactable = false;
        }
        else
        {
            view.Upgrade2Text.text = $"{names.Upgrade2Name}\nДоход: +{config.Upgrade2.IncomeMultiplier}%\nЦена: {config.Upgrade2.Cost}$";
            view.Upgrade2Button.interactable = playerBalance >= config.Upgrade2.Cost;
        }
    }

    private BusinessConfig.BusinessData GetBusinessConfig(ref Business business)
    {
        foreach (var config in _businessConfig.Businesses)
        {
            if (config.Id == business.Id) return config;
        }
        return default;
    }
}