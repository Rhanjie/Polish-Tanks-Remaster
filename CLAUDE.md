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
  - `test run --mode editor [--filter X]`: testy EditMode w otwartym edytorze. Samodzielne `unity test` nie zadziała, gdy projekt jest otwarty;
  - `test run --mode playmode --async-tests true`, potem odpytuj `test status` aż do `status: completed`. PlayMode nie działa synchronicznie, bo wejście w Play Mode przeładowuje domenę i zrywa żądanie;
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

**DUŻE** (osobna gałąź + PR + pełny proces agentów). Wystarczy jedno z kryteriów:
- nowy system lub mechanika rozgrywki (ruch, strzelanie, AI, UI ekranu, zapis gry…);
- zmiana w więcej niż ~3 plikach `.cs` albo nowy publiczny interfejs używany przez inne systemy;
- zmiany w scenie lub prefabach razem z nowym kodem;
- zmiana ustawień projektu, pakietów, Input Actions, warstw kolizji;
- naprawa błędu, którego przyczyna nie jest oczywista;
- użytkownik użył słów „funkcjonalność”, „feature”, „system” albo poprosił o pełny proces.

**MAŁE** (bez agentów, na bieżącej gałęzi, bez commita bez prośby; jeśli bieżąca gałąź to `development` i trzeba commitować, zrób krótką gałąź `fix/…` i PR, bo `development` jest chroniony):
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
7. **Pull request.** `git push -u origin feature/<nazwa>`, potem `gh pr create --base development` z tytułem w stylu commita. Opis ma być krótki, rzeczowy i pisany jak przez człowieka: bez rozbudowanych list, pogrubionych podpunktów i wyliczania każdej liczby. Sekcje:
   - **Summary**: co i po co (2–3 zdania);
   - **Verification**: 1–2 linie (wynik review, liczby testów, smoke test);
   - **Known limitations**: tylko realne ograniczenia, po jednej linii (pomiń sekcję, jeśli ich nie ma).

   Nie dodawaj checklisty testów manualnych do opisu PR.

   Odczekaj na CI: `gh pr checks <nr> --watch`. Jeśli CI pada, przeczytaj logi (`gh run view <id> --log-failed`), napraw, wypchnij i powtórz.
8. **STOP: weryfikacja manualna.** Podaj użytkownikowi link do PR, wynik CI i krótką listę rzeczy do sprawdzenia w edytorze (w czacie, nie w PR). Potem **czekaj**. Merge robi użytkownik przyciskiem na GitHubie albo agent (`gh pr merge <nr> --merge`) **tylko** po wyraźnym „ok, merguj”. Zawsze merge commit (bez squash i rebase).
9. **Po merge'u:** `git switch development && git pull`. **Nie usuwaj gałęzi feature** (ani lokalnie, ani na GitHubie): właściciel chce mieć je w historii. Jeśli `development` uciekł do przodu w trakcie prac, najpierw `git merge origin/development` na gałęzi feature, rozwiąż konflikty, uruchom testy i wypchnij. Nie rób rebase gałęzi, które już zostały wypchnięte.

### Wydania i CI (GitHub Actions)

- **PR → `development`**: wszystkie testy EditMode i PlayMode (wymagany check).
- **PR → `master`**: testy + kontrola, że wersja jest nowa + build Windows.
- **Push na `master`**: testy, build, tag `vX.Y.Z` i GitHub Release. Wersja pochodzi z `bundleVersion` w Player Settings, a PR do `master` musi ją podbić.
- **Weekly playtest**: w piątki z `development`, pre-release `playtest-RRRR-Www`, tylko jeśli były zmiany.
- Wydanie przygotowuje się PR-em `development` → `master`, tylko na prośbę użytkownika. Agent nigdy nie pushuje bezpośrednio na `master` ani na `development`.

### Commity

- **Zawsze po angielsku**, poprawną, naturalną angielszczyzną, w trybie rozkazującym, z wielkiej litery, bez kropki na końcu i bez prefiksów (`feat:` itp.).
- Pierwsza linia do ~72 znaków. Ma mówić **co i po co**, a nie „jakie pliki”.
- Przykłady dobre: `Add test bouncing ball to test CLI integration`, `Fix tank sliding after collision with walls`, `Move turret rotation logic to TurretAimer`.
- Przykłady złe: `fixes`, `update files`, `WIP`, `Dodano piłkę`, `Changed PlayerController.cs`.
- Jeśli zmiana wymaga wyjaśnienia, dodaj po pustej linii krótkie uzasadnienie „dlaczego” (2–4 linie).
- Commit merge z PR tworzy GitHub, a jego tytuł to tytuł PR. Tytuł PR pisz więc tak jak commit.
- **Nie dodawaj** trailera `Co-Authored-By: Claude …` ani żadnej innej atrybucji AI do commitów i opisów PR. To wyraźne życzenie właściciela repo i ma pierwszeństwo przed domyślną atrybucją.

Uwagi:
- Nigdy nie używaj `--force`, `reset --hard` ani `clean -fd` bez wyraźnej zgody.
- Konflikty w scenach i prefabach rozwiązuje UnityYAMLMerge (`unity vcs merge-setup`). Gdy konflikt dotyczy sceny, pokaż go użytkownikowi, zamiast zgadywać.
