# Czołg gracza

Prototyp: jazda, wieża z opóźnieniem i działo bez pocisków. Komponenty w `Assets/Scripts/Tank/`.

## Konwencja osi
- Lokalne up kadłuba (`PlayerTank`) to przód.
- `Turret` to dziecko kadłuba. Jego up wskazuje lufę.
- `TurretSprite` jest obrócony o +90°, bo w `TankTurret_01.png` lufa jest skierowana w +X.
- `Muzzle` leży na (0, 4) w układzie `Turret`.
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

## Parametry
`TankDriveSettings`, `TurretSettings` i `GunSettings` to tymczasowe struktury w polach komponentów. Zastąpią je moduły ScriptableObject w kolejnej funkcjonalności.

## Grafiki
- Tekstury trawy w starych mediach to BMP z rozszerzeniem `.png`. Przed importem trzeba je przekonwertować.
- Muzzle flash jest generowany proceduralnie: pivot na dole, płomień w +Y.

## Scena
- `TankSandbox` ma indeks 0 w buildzie.
- Kamera: Cinemachine (`CinemachineCamera` + `PositionComposer`).
