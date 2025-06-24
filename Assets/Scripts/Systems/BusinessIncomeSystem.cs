using Leopotam.EcsLite;
using UnityEngine;

public class BusinessIncomeSystem : IEcsRunSystem
{
    private readonly GameData _gameData;
    private readonly BusinessConfig _businessConfig;

    public BusinessIncomeSystem(GameData gameData, BusinessConfig businessConfig)
    {
        _gameData = gameData;
        _businessConfig = businessConfig;
    }

    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var filter = world.Filter<Business>().Inc<IncomeProgress>().End();
        var businessPool = world.GetPool<Business>();
        var progressPool = world.GetPool<IncomeProgress>();
        var viewPool = world.GetPool<BusinessView>();
        var balancePool = world.GetPool<PlayerBalance>();
        var balanceEntities = world.Filter<PlayerBalance>().End();

        if (balanceEntities.GetEntitiesCount() == 0)
        {
            return;
        }
        
        var balanceEntity = balanceEntities.GetRawEntities()[0];
        ref var balance = ref balancePool.Get(balanceEntity);

        foreach (var entity in filter)
        {
            ref var business = ref businessPool.Get(entity);
            ref var progress = ref progressPool.Get(entity);
            ref var view = ref viewPool.Get(entity);

            if (!_gameData.IsBusinessPurchased(business.Id))
            {
                continue;
            }
            
            var businessConfig = GetBusinessConfig(business.Id);

            if (businessConfig == null)
            {
                continue;
            }
            
            progress.TimePassed += Time.deltaTime;
            progress.Progress = progress.TimePassed / businessConfig.IncomeDelay;
            
            if (view.ProgressBar != null)
            {
                view.ProgressBar.fillAmount = progress.Progress;
            }

            if (progress.Progress >= 1f)
            {
                balance.Value = _gameData.AddBusinessIncome(business.Id, balance.Value, business.CurrentIncome);
                progress.TimePassed = 0f;
                progress.Progress = 0f;
            }
        }
    }

    private BusinessConfig.BusinessData GetBusinessConfig(string businessId)
    {
        foreach (var config in _businessConfig.Businesses)
        {
            if (config.Id == businessId)
            {
                return config;
            }
        }
        return null;
    }
}