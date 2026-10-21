using Zenject;
using UnityEngine;

public class GameSceneInstaller : MonoInstaller
{
    [Header("Ссылки на компоненты Сцены")]
    [SerializeField] private VisualEffectsService visualEffectsService;
    [SerializeField] private PeepholeController peepholeController;

    public override void InstallBindings()
    {
        // 1. Регистрируем сервис визуальных эффектов как синглтон на сцене
        Container.Bind<VisualEffectsService>()
            .FromComponentInHierarchy(visualEffectsService)
            .AsSingle();

        // 2. Регистрируем контроллер глазка
        Container.Bind<PeepholeController>()
            .FromComponentInHierarchy(peepholeController)
            .AsSingle();

        Debug.Log("Zenject: Все зависимости успешно связаны!");
    }
}
