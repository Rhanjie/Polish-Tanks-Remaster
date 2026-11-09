# Prototypy widoku: top-down czy 2.5D

Dwie sceny z tym samym czołgiem, skrzyniami i sześcioma budynkami o wysokości od 1,5 (mur) do 10 (wieża ciśnień):
- `Assets/Scenes/Prototypes/ViewPrototype_TopDown.unity`
- `Assets/Scenes/Prototypes/ViewPrototype_25D.unity`

| Top-down | 2.5D |
|---|---|
| ![top-down](ViewPrototypes/topdown.png) | ![2.5D](ViewPrototypes/25d.png) |

## Jak to działa
- `Building` przechowuje footprint i wysokość. Collider jest zawsze na poziomie gruntu, w obu wariantach taki sam.
- `BuildingShadow` w obu wariantach rzuca cień: footprint przesunięty wzdłuż słońca proporcjonalnie do wysokości.
- **Top-down** (`FlatBuildingView`): sam dach na footprincie. Wysokość widać tylko po długości cienia.
- **2.5D** (`ExtrudedBuildingView`, styl wczesnego GTA): dach przesuwa się od środka kamery o `wysokość × perspective`. Ściany zwrócone do kamery to mesh przebudowywany co klatkę. Budynek zanika, gdy zasłania wskazany obiekt.

## Obserwacje
- **2.5D daje dużo więcej klimatu.** Budynki mają bryłę, widać okna i piętra, a przesuwanie kamery tworzy paralaksę. Wysoki obiekt od razu wygląda na wysoki.
- **Kamera wycentrowana na czołgu sprawia, że budynki zawsze odchylają się od gracza.** Czołg nigdy nie jest zasłonięty przez budynek, przy którym stoi. Zasłaniane są tylko rzeczy przy krawędziach ekranu, czyli zwykle wrogowie i pociski. Dla czytelności walki to ważne: wróg za blokiem może być niewidoczny, więc 2.5D potrzebuje sylwetek albo zanikania budynków także dla wrogów.
- **Pociski lecą na poziomie gruntu.** W 2.5D wyglądają, jakby wpadały pod dach, bo kolizja jest z podstawą, a nie z pochyloną bryłą. To akceptowalne, bo tak samo jest w GTA 1/2. Pociski „nad budynkami” (artyleria, samoloty) wymagałyby wysokości w logice pocisków.
- **Przy dużym oddaleniu kamery (optyka 13) wysokie budynki przy krawędzi mocno się pochylają.** Wieża ciśnień przesuwa się o ~6,5 jednostki. Warto ograniczyć `perspective` albo maksymalną wysokość.
- **Samoloty i wybuchy z góry pasują do 2.5D naturalnie.** Wystarczy ten sam wzór przesunięcia dla obiektów w powietrzu: samolot na wysokości 15 przesuwa się jak dach.
- **Top-down jest prostszy do wszystkiego.** Grafika to same dachy, nie trzeba rozwiązywać zasłaniania, sortowania ścian ani ich tekstur. Za to budynki są „płaskimi kafelkami”, a wysokość czyta się tylko z cienia.
- **Koszt produkcji 2.5D:**
  - ściany potrzebują tekstur dla każdego typu budynku;
  - budynki muszą być prostopadłościanami równoległymi do osi (obrót i nieregularne kształty wymagają więcej pracy);
  - ściany sortują się względem siebie tylko w obrębie jednego budynku, więc stykające się budynki mogą mieć artefakty;
  - kod jest prosty: jeden mesh na budynek i brak alokacji co klatkę.

## Rekomendacja
**2.5D**, z trzema zastrzeżeniami:
- zasłanianie rozwiązać od razu ogólnie (zanikanie budynku, gdy zasłania dowolnego aktora albo pocisk, albo sylwetka wroga za budynkiem);
- ograniczyć pochylenie (`perspective` zależne od zoomu albo limit przesunięcia);
- budynki projektować jako zestaw prostokątnych brył. Złożony budynek to kilka `Building` obok siebie, a nie jeden nieregularny kształt.

Pełny top-down warto wybrać, jeśli priorytetem jest szybka produkcja dużej liczby map i czytelność walki ponad klimat.
