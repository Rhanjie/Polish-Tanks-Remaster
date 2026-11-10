# Czołg gracza

Prototyp: jazda, wieża z opóźnieniem, strzelanie z działa i karabinu oraz parametry z modułów ScriptableObject. Komponenty w `Assets/Scripts/Tank/`.

## Konwencja osi
- Lokalne up kadłuba (`PlayerTank`) to przód.
- `Turret` to dziecko kadłuba. Jego up wskazuje lufę.
- `TurretSprite` jest obrócony o +90°, bo w `TankTurret_01.png` lufa jest skierowana w +X.
- `Muzzle` leży na (0, `TurretModule.MuzzleDistance`) w układzie `Turret`, a `Turret` na `HullModule.TurretMount` w kadłubie.
- Kąt wieży jest lokalny względem kadłuba. Obrót kadłuba ciągnie wieżę, a `TurretAimer` musi ją potem dogonić.

## Sterowanie
- `TankInput` dostaje wstrzyknięte `IPlayerInput` i `IGameCamera` (patrz `DependencyInjection.md`). Akcje z project-wide Input Actions: `Player/Move`, `Player/Aim`, `Player/Attack` (działo, LPM) i `Player/FireSecondary` (karabin sprzężony, PPM).
- Kompozyt WASD ma tryb Digital (`Dpad(mode=1)`). Domyślna normalizacja obcinała gaz przy skręcie.
- Wyłącz `TankInput`, żeby sterować z kodu (testy, AI):
  - `TankMovement.SetInput(throttle, steer)`
  - `TurretAimer.SetAimPoint(worldPoint)`
  - `TankGun.TryFire()` / `SetTrigger(bool)`

## Ruch
- `TankMovement` w każdym `FixedUpdate` najpierw czyta faktyczną prędkość wzdłuż osi (sync po kolizji), potem ustawia prędkość `Rigidbody2D`.
- Kierunek bierze z ciała, nie z transformu. Przy interpolacji transform spóźnia się o krok fizyki.

- Recoil i trafienia nie mogą zmieniać `linearVelocity` bezpośrednio, bo napęd nadpisuje ją co krok. Służy do tego `TankMovement.AddImpulse`: impuls jest nakładany na prędkość z gąsienic i wygasa wykładniczo (`ExternalImpulse`). Prędkość zabrana przez kolizję najpierw gasi impuls, więc czołg nie odbija od ściany.

## Strzelanie
- Czołg ma dwa komponenty `TankGun`: działo (`Muzzle`) i karabin sprzężony (`CoaxMuzzle`). `TankController` ustawia oba z `TankStats.Gun` i `TankStats.MachineGun`.
- Pociski (`Projectile`) nie mają colliderów. Co klatkę robią `CircleCast` po odcinku lotu, pomijając Rigidbody2D strzelca. Dlatego karabin może strzelać spod wieży, z wnętrza własnego kadłuba, a szybkie pociski nie przenikają przez obiekty. Matryca kolizji warstw nie jest potrzebna.
- Pooling: wstrzyknięty `IPoolService.For(prefab)` daje jedną pulę na prefab dla danej sceny. Pule giną razem z kontenerem sceny. Obiekty oddaje się przez `ReturnToPool()`, nigdy przez `Destroy`. `TankGun` rozgrzewa pule pocisku i efektu trafienia w `Start`.
- `ReloadTimer` przenosi resztę czasu z klatki, w której padł strzał, a `TankGun` może oddać kilka strzałów na klatkę. Dzięki temu szybkostrzelność karabinu nie zależy od FPS.
- „Walnięcie” jest konfigurowane w `MainGunModule`: `recoilImpulse` (odrzut kadłuba), `shakeForce` (wstrzyknięty `ICameraShake`: impulse source w `SceneScope/CameraShake`, kanał 1, listener na `Tank Follow Camera`), `barrelKick`/`barrelReturnTime` (`BarrelRecoil` cofa `TurretSprite`) i `impactImpulse` (pchnięcie celu). Impulsy liczą się w tonach × j/s.
- Reakcja na trafienie: komponent z `IProjectileHitHandler` na colliderze albo jego rodzicu (np. `HitFlash` na skrzyniach).

## Świecenie
- Shader `Polish Tanks/Sprite Glow` jest addytywny i ma kolor HDR (`_GlowColor` > 1). Bloom w `Assets/Settings/TankSandbox_PostFX.asset` (threshold ~1) robi z tego poświatę. Kamera musi mieć włączony Post Processing.
- Shader ignoruje `SpriteRenderer.color` i flip (`unity_SpriteColor`). Kolor ustawia się w materiale, a TrailRenderer i cząsteczki tintują go kolorem wierzchołków.
- Cienkie smugi prawie nie łapią bloomu. Gdy efekt jest niewidoczny, najpierw powiększ sprite albo ślad, a dopiero potem podkręcaj intensywność.

## Moduły i loadout
- Parametry czołgu pochodzą z modułów ScriptableObject (`Assets/Scripts/Tank/Modules/`, assety w `Assets/Data/Modules/`). Każdy moduł ma masę, koszt i tier pod warsztat.
- `TankLoadout` to zestaw: jeden moduł na slot plus lista wyposażenia (`EquipmentModule`). Wymagane sloty: kadłub, silnik, przekładnia, gąsienice, wieża, działo. Liczba wyposażenia nie może przekroczyć `HullModule.EquipmentSlots`.
- `TankController` na prefabie stosuje loadout w `Awake` (albo przez `Apply`). Niepoprawny loadout jest odrzucany z błędem w konsoli, a czołg zostaje przy poprzednim.
- `TankStatsCalculator` liczy z loadoutu `TankStats`, a z nich struktury `TankDriveSettings`, `TurretSettings` i `GunSettings`. Pola tych struktur w inspektorach komponentów to tylko wartości domyślne, gdy czołg nie ma `TankController`.
- Zasady (stałe na górze kalkulatora):
  - prędkość ~ √(KM/t), przyspieszenie ~ KM/t;
  - masa ponad `TracksModule.MaxLoad` obniża proporcjonalnie prędkość i przyspieszenie;
  - działo cięższe niż `TurretModule.MaxGunMass` spowalnia traverse;
  - optyka ustawia `ViewRange` (ortho size kamery przez `OpticsCameraZoom`) i mnoży rozrzut.
- Nowe wyposażenie: podklasa `EquipmentModule` z własnym `Modify(TankStats)`. Kalkulatora nie trzeba ruszać. `Modify` wywołuje się w kolejności listy.
- Masa Rigidbody2D to masa czołgu w tonach.
- Moduły tylko się czyta. Nigdy nie zmieniaj assetów SO w runtime, bo są współdzielone między czołgami.

## Grafiki
- Tekstury trawy w starych mediach to BMP z rozszerzeniem `.png`. Przed importem trzeba je przekonwertować.
- Muzzle flash jest generowany proceduralnie: pivot na dole, płomień w +Y. `MainGunModule.MuzzleFlashScale` mnoży skalę ustawioną w prefabie.
- Warianty `_02_Desert` i `_03_Grey` to przebarwienia `_01`. Brązowy obrys (odcień < 0.11, nasycenie > 0.5) jest przyciemniany osobno, żeby kontur pozostał czytelny.

## Scena
- `TankSandbox` ma indeks 0 w buildzie.
- Kamera: Cinemachine (`CinemachineCamera` + `PositionComposer` + `CinemachineImpulseListener`).
- Skrzynie (`Assets/Prefabs/Props/`) to dynamiczne Rigidbody2D bez grawitacji, z dużym tłumieniem, które imituje tarcie o ziemię. Drewniana waży 0,8 t, metalowa 4 t.
