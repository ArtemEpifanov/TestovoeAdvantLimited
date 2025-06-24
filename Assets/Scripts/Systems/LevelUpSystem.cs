using Leopotam.EcsLite;
using UnityEngine;

public class LevelUpSystem : IEcsRunSystem
{
    private readonly BusinessConfig _businessConfig;
    private readonly BusinessNamesConfig _namesConfig;
    private readonly GameData _gameData;

    public LevelUpSystem(BusinessConfig businessConfig, BusinessNamesConfig namesConfig, GameData gameData)
    {
        _businessConfig = businessConfig;
        _namesConfig = namesConfig;
        _gameData = gameData;
    }

    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var businessPool = world.GetPool<Business>();
        var viewPool = world.GetPool<BusinessView>();
        var balancePool = world.GetPool<PlayerBalance>();
        var balanceEntities = world.Filter<PlayerBalance>().End();

        if (balanceEntities.GetEntitiesCount() == 0)
        {
            return;
        }
        
        var balanceEntity = balanceEntities.GetRawEntities()[0];
        ref var balance = ref balancePool.Get(balanceEntity);

        var filter = world.Filter<Business>().Inc<BusinessView>().End();
        
        foreach (var entity in filter)
        {
            ref var business = ref businessPool.Get(entity);
            ref var view = ref viewPool.Get(entity);

            var config = GetBusinessConfig(ref business);
            float levelUpPrice = (business.Level + 1) * config.BaseCost;

            view.LevelUpButton.interactable = balance.Value >= levelUpPrice;
            
            if (view.LevelUpButton != null && IsButtonClicked(view.LevelUpButton))
            {
                balance.Value -= levelUpPrice;
                business.Level++;
                
                _gameData.UpdateBalanceDisplay(balance.Value);
                UpdateBusinessIncome(ref business, config);
                _gameData.SetBusinessLevel(business.Id, business.Level);
                UpdateBusinessView(entity, balance.Value, world);
            }
        }
    }

    private bool IsButtonClicked(UnityEngine.UI.Button button)
    {
        return button.GetComponent<ButtonClickComponent>()?.IsClicked ?? false;
    }

    private BusinessNamesConfig.BusinessNames GetNamesForBusiness(string id)
    {
        foreach (var names in _namesConfig.Names)
        {
            if (names.Id == id)
            {
                return names;
            }
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
            if (config.Id == business.Id)
            {
                return config;
            }
        }
        return default;
    }
}