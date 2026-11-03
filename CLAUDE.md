# Polish Tanks Remaster

Gra 2D w Unity 6 (`6000.6.7f1`), URP 2D, Input System, Unity Test Framework.
Edytor jest zwykle otwarty i użytkownik pracuje w nim **równolegle z agentem**.

## Sterowanie edytorem: Unity CLI

- Do wszystkiego, co dotyczy sceny, prefabów, assetów, Play Mode, konsoli i testów, używaj `unity` CLI (skill `unity:unity-cli`). Na start sprawdź `unity status --project-path "F:\Unity\Polish Tanks Remaster"`. Stan musi być `ready`.
- Jeśli `unity` nie ma w PATH, użyj pełnej ścieżki: `C:\Users\RhAnjiE\AppData\Local\unity\bin\unity.exe`.
- Dawaj zawsze `--project-path "F:\Unity\Polish Tanks Remaster"` oraz `--caller plugin --skill <nazwa-skilla>`.
- `script eval`: kod C# przekazuj przez `--file <ścieżka.cs>` (plik w scratchpadzie). Bezpośrednio podany kod PowerShell rozbija na argumenty.
- **Nigdy** nie edytuj ręcznie plików `.unity`, `.prefab` ani `.asset`, gdy edytor jest osiągalny.
- Jeśli edytor jest nieosiągalny albo w Safe Mode, sprawdź `unity status`. Przy Safe Mode napraw błędy kompilacji. W innych przypadkach powiedz to użytkownikowi, nie obchodź problemu.
- Przydatne komendy:
  - `console get --level warn`, `console status`: konsola i błędy kompilacji;
  - `test run --mode editor|playmode [--filter X]`: testy w otwartym edytorze. Samodzielne `unity test` nie zadziała, gdy projekt jest otwarty;
  - `editor.playmode start|stop`: Play Mode;
  - `capture`: zrzuty widoku.

## Wspólna praca w jednym edytorze

Użytkownik klika w tym samym edytorze, więc:
- **Nie zapisuj sceny** (`SaveScene`, `scene save`) bez potrzeby wynikającej z zadania. Zapis obejmuje też niezapisane zmiany użytkownika. Przed zapisem zapytaj, czy użytkownik ma otwarte, niezapisane zmiany.
- **Nie wchodź w Play Mode bez zapowiedzi.** Zawsze z niego wychodź po weryfikacji.
- **Nie zmieniaj** `Selection`, układu okien ani ustawień edytora.
- Zmiany na scenie rejestruj w Undo (`Undo.RegisterCreatedObjectUndo`, `Undo.RecordObject`).
- Gdy to możliwe, pracuj na prefabach albo na osobnych obiektach. Nie modyfikuj obiektów, nad którymi użytkownik właśnie pracuje.

## Konwencje kodu

- Kod gry: `Assets/Scripts/` z asmdef `PolishTanks.Runtime`. Kod edytorowy: `Assets/Scripts/Editor/` (`PolishTanks.Editor`). Testy: `Assets/Tests/EditMode/` (`PolishTanks.Tests.EditMode`) i `Assets/Tests/PlayMode/` (`PolishTanks.Tests.PlayMode`). Twórz te asmdefy, jeśli ich brakuje. Testy nie mogą referować `Assembly-CSharp`, dlatego kod gry musi mieć asmdef.
- Jeden `MonoBehaviour`/`ScriptableObject` na plik, nazwa pliku = nazwa klasy.
- Logikę, którą da się testować bez sceny, wydzielaj do zwykłych klas C#, które `MonoBehaviour` tylko wywołuje.
- Każdy nowy asset razem z plikiem `.meta`. Nigdy nie commituj `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `.idea/`.
- Po każdej zmianie w `.cs` sprawdź `console status` / `console get --level error`. Kod, który się nie kompiluje, nie jest skończony.

## Git flow

Gałęzie:
- `master`: tylko stabilne wydania. **Agent nigdy nie commituje ani nie merguje do `master`.**
- `development`: gałąź integracyjna. Każda funkcjonalność trafia tutaj.
- `feature/<krótki-opis>`, `fix/<krótki-opis>`, `refactor/<krótki-opis>`: zawsze odgałęzione od aktualnego `development`, nazwy kebab-case po angielsku (np. `feature/tank-movement`).

### Klasyfikacja zadania (obowiązkowa na start)

Przed rozpoczęciem pracy **napisz w jednym zdaniu klasyfikację i jej powód**, np. „Klasyfikacja: DUŻE, bo nowy system + nowe prefaby”. Użytkownik może ją zmienić.

**DUŻE** (osobna gałąź + pełny proces agentów). Wystarczy jedno z kryteriów:
- nowy system lub mechanika rozgrywki (ruch, strzelanie, AI, UI ekranu, zapis gry…);
- zmiana w więcej niż ~3 plikach `.cs` albo nowy publiczny interfejs używany przez inne systemy;
- zmiany w scenie lub prefabach razem z nowym kodem;
- zmiana ustawień projektu, pakietów, Input Actions, warstw kolizji;
- naprawa błędu, którego przyczyna nie jest oczywista;
- użytkownik użył słów „funkcjonalność”, „feature”, „system” albo poprosił o pełny proces.

**MAŁE** (bez osobnej gałęzi i bez agentów, na bieżącej gałęzi, bez commita bez prośby):
- poprawka w 1–2 plikach o oczywistej przyczynie, zmiana wartości lub tuning, rename, literówka;
- pojedyncza operacja na scenie, o którą użytkownik prosi wprost;
- pytania, analizy, eksperymenty.

W razie wątpliwości wybierz DUŻE.

### Proces dla DUŻYCH zadań

1. **Start.** Sprawdź `git status`. Jeśli są niezacommitowane zmiany, **zapytaj użytkownika**, co z nimi zrobić. Nie stashuj ich i nie commituj samodzielnie. Zapisz scenę tylko za zgodą (checkout przeładowuje assety w otwartym edytorze). Potem `git switch development && git switch -c feature/<nazwa>`. Po checkout odczekaj `unity status --until-ready`.
2. **Analiza i implementacja** w głównej sesji, sterując edytorem. Commituj małymi, logicznymi krokami (zasady w sekcji „Commity”). Przed commitem sprawdź `git status`/`git diff` i **nie dołączaj niezwiązanych zmian** (Unity potrafi samo zmieniać `ProjectSettings/` albo sceny).
3. **Weryfikacja równoległa** (agenci uruchomieni w jednej wiadomości):
   - `unity-reviewer`: przegląd diffu `development...HEAD`;
   - `unity-test-writer`: testy EditMode i PlayMode dla nowej logiki.
4. **Poprawki** znalezisk reviewera, które się potwierdziły. Fałszywe alarmy odrzuć z uzasadnieniem.
5. **`unity-playmode-verifier`**, sekwencyjnie, bo używa edytora: kompilacja, uruchomienie wszystkich testów, smoke test w Play Mode. Jeśli coś pada, wracasz do 4.
6. **`docs-writer`**, tylko jeśli zmieniło się coś, czego nie widać z kodu (nowy system, konfiguracja, decyzja architektoniczna).
7. **STOP: weryfikacja manualna.** Wypisz użytkownikowi:
   - nazwę gałęzi i listę commitów;
   - wyniki testów i review;
   - **konkretną checklistę do ręcznego sprawdzenia w edytorze** (co otworzyć, co kliknąć, czego się spodziewać);
   - znane ograniczenia.

   Potem **czekaj**. Nie merguj bez wyraźnego „ok, merguj” od użytkownika.
8. **Merge po akceptacji:** `git switch development && git merge --no-ff feature/<nazwa>`, z opisem merge'a podsumowującym funkcjonalność. Jeśli `development` uciekł do przodu, najpierw `git merge development` na gałęzi feature, rozwiąż konflikty, ponownie uruchom testy, i dopiero potem merge. Bez rebase gałęzi, które już zostały wypchnięte. Usuń lokalną gałąź feature po merge'u.
9. **Push** (`git push`) tylko na prośbę użytkownika.

### Commity

- **Zawsze po angielsku**, poprawną, naturalną angielszczyzną, w trybie rozkazującym, z wielkiej litery, bez kropki na końcu i bez prefiksów (`feat:` itp.).
- Pierwsza linia do ~72 znaków. Ma mówić **co i po co**, a nie „jakie pliki”.
- Przykłady dobre: `Add test bouncing ball to test CLI integration`, `Fix tank sliding after collision with walls`, `Move turret rotation logic to TurretAimer`.
- Przykłady złe: `fixes`, `update files`, `WIP`, `Dodano piłkę`, `Changed PlayerController.cs`.
- Jeśli zmiana wymaga wyjaśnienia, dodaj po pustej linii krótkie uzasadnienie „dlaczego” (2–4 linie).
- Commit merge do `development`: `Merge feature/<nazwa>: <jedno zdanie o funkcjonalności>`.

Uwagi:
- Nigdy nie używaj `--force`, `reset --hard` ani `clean -fd` bez wyraźnej zgody.
- Konflikty w scenach i prefabach rozwiązuje UnityYAMLMerge (`unity vcs merge-setup`). Gdy konflikt dotyczy sceny, pokaż go użytkownikowi, zamiast zgadywać.
