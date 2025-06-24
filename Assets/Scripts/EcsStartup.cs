using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EcsStartup : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private BusinessConfig businessConfig;
    [SerializeField] private BusinessNamesConfig namesConfig;
    
    [Header("UI References")]
    [SerializeField] private Transform businessesContainer;
    [SerializeField] private GameObject businessViewPrefab;
    [SerializeField] private Text balanceText;

    private EcsWorld _world;
    private IEcsSystems _systems;
    private GameData _gameData;

    private void Start()
    {
        _gameData = new GameData
        {
            BusinessesContainer = businessesContainer,
            BusinessViewPrefab = businessViewPrefab,
            BalanceText = balanceText
        };

        _gameData.Load();
        _world = new EcsWorld();
        _systems = new EcsSystems(_world);
        
        _systems
            .Add(new BusinessInitSystem(businessConfig, namesConfig, _gameData))
            .Add(new BusinessIncomeSystem(_gameData, businessConfig))
            .Add(new LevelUpSystem(businessConfig, namesConfig, _gameData))
            .Add(new UpgradeSystem(businessConfig, namesConfig, _gameData))
            .Add(new SaveLoadSystem(_gameData))
            .Init();
    }

    private void Update()
    {
        _systems?.Run();
    }

    private void OnDestroy()
    {
        if (_systems != null)
        {
            _systems.Destroy();
            _systems = null;
        }
        
        if (_world != null)
        {
            _world.Destroy();
            _world = null;
        }
    }
}