using Leopotam.EcsLite;

public class SaveLoadSystem : IEcsDestroySystem
{
    private readonly GameData _gameData;

    public SaveLoadSystem(GameData gameData)
    {
        _gameData = gameData;
    }

    public void Destroy(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var balancePool = world.GetPool<PlayerBalance>();
        var businessPool = world.GetPool<Business>();
        var progressPool = world.GetPool<IncomeProgress>();
        
        var balanceEntities = world.Filter<PlayerBalance>().End();
        
        if (balanceEntities.GetEntitiesCount() > 0)
        {
            _gameData.SavedBalance = balancePool.Get(balanceEntities.GetRawEntities()[0]).Value;
        }
        
        var businessEntities = world.Filter<Business>().End();
        
        foreach (var entity in businessEntities)
        {
            ref var business = ref businessPool.Get(entity);
            _gameData.SetBusinessLevel(business.Id, business.Level);
            _gameData.SetUpgradePurchased(business.Id, 1, business.UpgradeOnePurchased);
            _gameData.SetUpgradePurchased(business.Id, 2, business.UpgradeTwoPurchased);

            if (progressPool.Has(entity))
            {
                _gameData.SetBusinessProgress(business.Id, progressPool.Get(entity).Progress);
            }
        }

        _gameData.Save();
    }
}