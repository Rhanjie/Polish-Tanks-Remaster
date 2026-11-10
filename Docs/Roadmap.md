# Wizja i plany

Kierunek gry spisany z rozmów z właścicielem. To nie są plany implementacji. Każdy punkt przed rozpoczęciem pracy dostaje własne zadanie i klasyfikację.

## Świat i ton
- Alternatywne uniwersum w klimacie drugiej wojny światowej. Polska ma ograniczone zasoby, więc eksperymentuje z czołgami jako pionier. Stąd dziwne konstrukcje: działo z odzysku po stacjonarnej haubicy z innego kraju, części sklejone „taśmą i dużą kreatywnością w terenie”.
- **Nazwy realistyczne, ale nie historyczne.** Żadnych prawdziwych producentów, oznaczeń ani modeli, żeby uniknąć problemów z nazwami własnymi.
- Niektóre tereny mogą być skażone (promieniowanie itp.), czyli coś w stylu Stalkera w 2D, tylko o czołgach. Pod ochronę przed skażeniem jest osobna linia modułów do odblokowania.

## Misje
- Oskryptowane momenty. Przykład: po dojechaniu do punktu sterowanie gracza się blokuje, kamera przechodzi na wskazany punkt, coś pokazuje i następuje wybuch.
- Przejazdy jednostek: spawn i przejazd z punktu A do B (zwiad, zapowiedź zagrożenia).
- Wrogowie: piechota, pojazdy, czołgi.
- Samoloty jako eventy: ostrzał z góry dla efektu „wow”.
- Pociągi, budynki, miejscami dużo ognia.

## Baza
- Baza gracza z budynkami, do których trzeba fizycznie wjechać, żeby mieć z nimi interakcję:
  - warsztat: zmiana wyposażenia czołgu;
  - centrum dowodzenia: wybór misji;
  - kolejne budynki później.
- Rozbudowa bazy to osobny, niezależny moduł: umocnienia, automatyczne działka, żołnierze.
- Oblężenia bazy: gracz walczy czołgiem jako wsparcie automatycznej linii umocnień, a działkami steruje SI.

## Warsztat i moduły
- Plany modułów kupuje się albo znajduje w bazach wroga.
- Bardzo dużo modułów, wiele odblokowywanych później. Dużo wariantów silników i innych części.
- Wysoka awaryjność jako cecha świata. Część modułów i ulepszeń ją zmniejsza.
- Narzędzia debugowe do szybkiej podmiany konkretnych modułów w trakcie gry.

## Walka i model zniszczeń
- Penetracja losowana z zakresu od x do y, efektywność pancerza (kąt trafienia).
- Widowiskowe rykoszety: kąt odbicia, efekt i dźwięk.
- Widoczny stopień zniszczenia czołgu.
- Zaawansowany model uszkodzeń: zerwana gąsienica, pożar silnika, zacięty mechanizm ładowania itd.
- Efekty na ekranie przy awarii: alarm i informacja w HUD-zie.

## Mniejsze rzeczy do obecnych systemów
- Dynamiczne cienie pod obiektami (URP 2D Shadow Caster).
- Ślady gąsienic.
- Płynne przybliżanie i oddalanie kamery.
- Prosty, nowoczesny HUD.
- Później: zmienna pogoda i pory dnia (klimatyczna mgła, deszcz, burza).

## Do zrobienia przy okazji
- **Zmienić nazwy przykładowych modułów z `Assets/Data/Modules/` na fikcyjne.** Obecne są historyczne (Maybach HL120/HL230, KwK 37/40/36, MG 34, Browning M1919, TZF 9b, Merritt-Brown, Schürzen). Zmieniać `displayName` i opisy. Nazwy plików assetów można zostawić albo zmienić przez AssetDatabase, żeby nie zgubić referencji.

- **Usunąć komentarze z metod i z wnętrza kodu** w obecnych skryptach, bo nowa konwencja jest w CLAUDE.md. Zostają tylko podsumowania klas i `[Tooltip]`.

## Architektura
- **Przerobić kod pod Extenject (Zenject, dependency injection).** Zrobić to osobną gałęzią, później, żeby było widać różnicę względem obecnego podejścia (referencje przez `[SerializeField]`, `PoolRegistry` jako statyczny rejestr, `Camera.main`, `InputSystem.actions`). Kandydaci do wstrzykiwania:
  - pule;
  - wejście;
  - kalkulator statystyk;
  - źródło impulsów kamery;
  - przyszłe serwisy (misje, zapis, warsztat).

## Otwarte decyzje
- **Pełny top-down czy top-down 2.5D** (wysokie budynki mają faktyczną wysokość). Decyzja wpłynie na:
  - sortowanie i rysowanie budynków;
  - cienie;
  - zasłanianie gracza przez budynki;
  - kolizje pocisków z wysokimi obiektami;
  - samoloty i ostrzał z góry;
  - koszt grafiki (sprite'y ścian albo proste meshe).

  Trzeba ją podjąć przed pierwszymi budynkami i misjami.

  Ustalenie: przed decyzją na osobnej gałęzi powstają dwa małe prototypy (pełny top-down i 2.5D) z tymi samymi kilkoma budynkami i czołgiem, żeby porównać je w grze.
