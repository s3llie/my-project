using System.ComponentModel;
using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [Header("Ссылки на компоненты Сцены")]
    [SerializeField] private VisualEffectsService visualEffectsService;
    [SerializeField] private PeepholeController peepholeController;
    [SerializeField] private RadioController radioController;
    [SerializeField] private PlayerMovement playerMovement;

    public override void InstallBindings()
    {
        Container.Bind<VisualEffectsService>()
            .FromComponentInHierarchy(visualEffectsService)
            .AsSingle();
        Container.Bind<PeepholeController>()
            .FromComponentInHierarchy(peepholeController)
            .AsSingle();
        Container.Bind<RadioController>().FromComponentInHierarchy(radioController).AsSingle();
        Container.Bind<PlayerMovement>().FromComponentInHierarchy(playerMovement).AsSingle();


        Debug.Log("Zenject: Все зависимости успешно связаны!");
    }
}

