# Licznik odbić i HUD

## Przepływ
```
BounceCounter (OnCollisionEnter2D, logika w BounceTracker)
  └─ event Bounced
       ├─ BounceColorChanger  → zmienia kolor piłki → event ColorChanged
       └─ BounceHud           ← akcent koloru też z ColorChanged
```
`BounceHud` ma w Inspectorze referencje do obu komponentów piłki (`BounceCounter`, `BounceColorChanger`). Bez nich HUD nic nie pokaże.

## UI Toolkit
- `Assets/UI/PanelSettings.asset`: ScaleWithScreenSize 1920×1080, match = height.
- Motyw: `Assets/UI/PolishTanksTheme.tss` (tylko import motywu domyślnego).
- Własne elementy rysowane Painter2D (`RadialGauge`, `Sparkline`, `ColorHistoryStrip` w `Assets/Scripts/UI`) są użyte w UXML przez `xmlns:hud="PolishTanks.UI"`.

## Pułapki
- `FillGradient` w Painter2D (Unity 6000.6) ignoruje alfę i rysuje pełny blok. Dlatego `Sparkline` robi zanik warstwami. Nie zamieniaj tego na gradient.
- W trybie edycji HUD bywa niewidoczny w Game View, dopóki widok się nie odświeży. W Play Mode renderuje się normalnie.
- Kolejność ładowania nie ma znaczenia: HUD synchronizuje akcent i drzewo niezależnie od tego, który komponent wstał pierwszy.

## Testy
Czysta logika (`BounceTracker`, `BounceColorSequence`, `FloatRingBuffer`, `HudFormat`) ma testy EditMode. Piłka na scenie: `Assets/Tests/PlayMode/BounceBallTests.cs`.
