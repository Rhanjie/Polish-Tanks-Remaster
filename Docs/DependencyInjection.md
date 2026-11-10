# Dependency injection (Reflex)

Projekt używa [Reflex](https://github.com/gustavopsantos/Reflex) 14.4 z UPM (adres git w `Packages/manifest.json`).

## Jak to jest złożone
- `Assets/Resources/ReflexSettings.asset` musi leżeć w `Resources`, bez niego Reflex rzuca asercję przy pierwszej scenie ze scope. Poziom logów to Warning, żeby bindingi nie zaśmiecały konsoli.
- **Root:** prefab `Assets/Settings/Reflex/RootScope.prefab` jest wpisany w `ReflexSettings` (lista RootScopes). Ma `GameInstaller`, który binduje `IPlayerInput` z project-wide Input Actions. Root żyje przez całą grę.
- **Scena:** obiekt `SceneScope` (komponent `ContainerScope`) z dzieckiem `SceneInstaller`. Binduje dla tej sceny:
  - `IGameCamera` (kamera gry, zamiast `Camera.main`);
  - `ICameraShake` (impulse source w `SceneScope/CameraShake`, zamiast komponentu na czołgu);
  - `PlayerTank` (czołg gracza, np. dla zoomu optyki);
  - `IPoolService` (pule pod `SceneScope/SceneInstaller/[Pools]`, zamiast statycznego rejestru).

  Kontener sceny jest niszczony razem ze sceną, a `PoolService.Dispose` usuwa pule.
- Scena bez `SceneScope` nie dostaje kontenera i nic w niej nie jest wstrzykiwane.

## Kiedy działa wstrzykiwanie
- `ContainerScope` ma `Awake` z kolejnością -1 000 000 000. Buduje kontener i wstrzykuje pola `[Inject]` we wszystkich MonoBehaviour sceny, zanim te dostaną `Awake`. Zależności można więc używać już w `Awake` i `OnEnable`.
- Obiekty tworzone w trakcie gry nie są wstrzykiwane automatycznie. Trzeba wywołać `GameObjectInjector.InjectRecursive(go, scene.GetSceneContainer())` albo dodać komponent `GameObjectSelfInjector`. Pociski z puli tego nie potrzebują, bo dostają pulę efektów w `Launch`.
- Każde pole `[Inject]` musi mieć binding w kontenerze. Brak bindingu to wyjątek przy wstrzykiwaniu, a nie cichy null.

## Co celowo nie jest wstrzykiwane
- Referencje między komponentami jednego czołgu (`TankMovement`, `TurretAimer`, `TankGun`, sprite'y) zostają w inspektorze. To części jednego prefabu, nie usługi.
- `TankStatsCalculator` zostaje statyczny, bo to czysta funkcja bez stanu.
- Dane (moduły, loadouty, definicje pocisków) zostają ScriptableObjectami podpinanymi w inspektorze.

## Testy
Testy nie mają `SceneScope`, więc same budują mały kontener:

```csharp
var container = new ContainerBuilder()
    .RegisterValue(pools, new[] { typeof(IPoolService) })
    .RegisterValue(new RecordingCameraShake(), new[] { typeof(ICameraShake) })
    .Build();
GameObjectInjector.InjectRecursive(tankObject, container);
```

Wstrzykuj przed `SetActive(true)`. Gotowe helpery: `WeaponFixture` (pule i shake) i `BuildingRig` (kamera).

## Nowa usługa
1. Interfejs i implementacja jako zwykłe klasy.
2. Binding w `GameInstaller` (globalne) albo `SceneInstaller` (per scena).
3. Pole `[Inject] private IMojaUsluga usluga;` w konsumencie.
4. Binding w helperze testów, jeśli konsument jest testowany.
