# Budynki 2.5D

**Decyzja:** gra używa widoku 2.5D w stylu wczesnego GTA. Pełny top-down został przetestowany w prototypie i odrzucony. Prototyp: `Assets/Scenes/Prototypes/ViewPrototype_25D.unity`, kod: `Assets/Scripts/Prototypes/`.

![2.5D](Buildings25D/25d.png)

## Jak powstaje efekt 3D w scenie 2D
Cała logika gry (fizyka 2D, kolizje, pociski) działa na płaszczyźnie gruntu. Wysokość to wyłącznie efekt wizualny.

1. **Dach:** sprite przesunięty od środka kamery o `(pozycja - kamera) × wysokość × perspective`. To przybliżenie rzutu perspektywicznego: punkt na wysokości `h` przy kamerze w odległości `D` przesuwa się na ekranie o ok. `h / D` swojej odległości od środka. Stąd `perspective ≈ 1 / D` (0,035). Im wyżej i dalej od środka, tym mocniejsze pochylenie.
2. **Ściany:** jeden mesh na budynek, przebudowywany co klatkę. Każda krawędź footprintu łączy się quadem z odpowiadającą krawędzią przesuniętego dachu. Rysowane są tylko ściany zwrócone do kamery (normalna przeciwna do przesunięcia dachu): maksymalnie 2 quady, czyli 8 wierzchołków. Pozostałe chowają się pod dachem. Cieniowanie ścian to kolor wierzchołka zależny od kąta do światła. UV pionowe liczone jest z wysokości, więc piętra z oknami ściskają się razem z perspektywą.
3. **Kolejność rysowania:**
   - cień `-50`, pod czołgiem;
   - grunt i obiekty: `-100…21`;
   - ściany `28`;
   - dach `29`.

   Budynek zawsze przykrywa wszystko, co stoi na ziemi.
4. **Cień:** footprint przesunięty wzdłuż słońca o `wysokość × długość`, jako wypukła otoczka 8 punktów (monotone chain). Przebudowywany tylko po zmianie.
5. **Zanikanie:** jeśli obserwowany obiekt leży w obrysie „podstawa + dach”, dach i ściany schodzą do alfy 0,35.
6. **Kolejność wykonania:** widok liczy się po `CinemachineBrain` (`DefaultExecutionOrder(200)`), inaczej dachy spóźniałyby się o klatkę za kamerą.

## Ograniczenia
- **Tylko prostopadłościany równoległe do osi.** Bez obrotu i kształtów L/U. Złożony budynek to kilka `Building` obok siebie.
- **Sortowanie między budynkami jest stałe.** Wszystkie ściany mają `28`, a dachy `29`. Gdy wysoki budynek zachodzi na ekranie na niski, jego ściany mogą narysować się pod dachem niskiego. Rozwiązanie: kolejność zależna od wysokości albo odległości od środka ekranu.
- **Rozgrywka jest płaska.** Pociski trafiają w podstawę, nie w pochyloną bryłę. Obiekty na dachach (działka, snajperzy) albo strzelanie nad budynkami wymagają wysokości w logice pocisków.
- **Budynki zasłaniają wrogów przy krawędziach ekranu.** Zanikanie trzeba rozszerzyć na wszystkich aktorów albo dodać sylwetki. Gracza budynki nie zasłaniają, bo kamera trzyma go w środku.
- **Pochylenie rośnie z odległością od środka.** Przy dużym oddaleniu wysokie budynki przy krawędzi mocno się kładą, więc potrzebny jest limit przesunięcia albo `perspective` zależne od zoomu.
- **Oświetlenie 2D (Light2D, ShadowCaster2D) nie zna wysokości.** Ściany są cieniowane na sztywno, a światła 2D oświetlają je jak płaski grunt. Pora dnia i pogoda muszą to uwzględniać (np. kolor ścian i długość cieni z jednego „słońca”).
- **Każdy typ budynku potrzebuje tekstury ściany i dachu.**

## Zniszczenia
Model dobrze się do tego nadaje, bo ściany i cień są i tak generowane z danych (`Size`, `Height`):
- **etapy uszkodzeń:** podmiana tekstur dachu i ścian na warianty zniszczone, plus dym i ogień w miejscach trafień;
- **zawalenie:** animacja `Height` do niskiej wartości (gruzowisko), z pyłem. Ściany i cień dopasowują się same, bo przebudowują się po zmianie wysokości;
- **częściowe zniszczenia:** budynek złożony z kilku brył (skrzydła, wieża), z których każda ma własne HP i może się zawalić osobno;
- **gruz:** collider można zmniejszyć albo zamienić na przejezdny dla czołgu, a niski gruz blokujący tylko piechotę też jest możliwy.

Nie da się tanio zrobić dowolnych dziur w ścianach (wycinania fragmentów bryły). Zastąpić to mogą decale (dziury, osmalenia) na teksturze ściany albo podział ściany na segmenty.

## Wydajność
Oszacowanie, nie pomiar. Profilowanie warto zrobić na pierwszej prawdziwej mapie.
- **Na budynek na klatkę:**
  - przesunięcie sprite'a dachu;
  - mesh ≤8 wierzchołków wysłany do GPU;
  - test otoczki z 8 punktów.

  Bez alokacji. To mikrosekundy na budynek, więc kilkaset budynków to rząd 1 ms CPU w najgorszym razie.
- **Draw calle:** 3 na budynek (cień, ściany, dach). Wspólne materiały się batchują, ale przy setkach budynków warto przejść na jeden mesh na fragment mapy.
- **Do zrobienia przy prawdziwych mapach:**
  - pomijać budynki poza ekranem;
  - przebudowywać ściany tylko po ruchu kamery;
  - wspólny mesh ścian i cieni na chunk mapy, czyli jeden draw call zamiast setek.
