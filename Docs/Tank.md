# Czołg gracza

Prototyp: jazda, wieża z opóźnieniem, działo bez pocisków i parametry z modułów ScriptableObject. Komponenty w `Assets/Scripts/Tank/`.

## Konwencja osi
- Lokalne up kadłuba (`PlayerTank`) to przód.
- `Turret` to dziecko kadłuba. Jego up wskazuje lufę.
- `TurretSprite` jest obrócony o +90°, bo w `TankTurret_01.png` lufa jest skierowana w +X.
- `Muzzle` leży na (0, `TurretModule.MuzzleDistance`) w układzie `Turret`, a `Turret` na `HullModule.TurretMount` w kadłubie.
- Kąt wieży jest lokalny względem kadłuba. Obrót kadłuba ciągnie wieżę, a `TurretAimer` musi ją potem dogonić.

## Sterowanie
- `TankInput` czyta project-wide Input Actions: `Player/Move`, `Player/Aim`, `Player/Attack`. `FireSecondary` jest zarezerwowane pod karabiny.
- Kompozyt WASD ma tryb Digital (`Dpad(mode=1)`). Domyślna normalizacja obcinała gaz przy skręcie.
- Wyłącz `TankInput`, żeby sterować z kodu (testy, AI):
  - `TankMovement.SetInput(throttle, steer)`
  - `TurretAimer.SetAimPoint(worldPoint)`
  - `TankGun.TryFire()` / `SetTrigger(bool)`

## Ruch
- `TankMovement` w każdym `FixedUpdate` najpierw czyta faktyczną prędkość wzdłuż osi (sync po kolizji), potem ustawia prędkość `Rigidbody2D`.
- Kierunek bierze z ciała, nie z transformu. Przy interpolacji transform spóźnia się o krok fizyki.

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
- Kamera: Cinemachine (`CinemachineCamera` + `PositionComposer`).
