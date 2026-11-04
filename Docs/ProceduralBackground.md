# Proceduralne tło

Warstwy (niebo, wzgórza, drzewa, mgła) to meshe generowane w kodzie, bez sprite'ów.

## Jak działa
- `ProceduralMeshBehaviour` jest `[ExecuteAlways]`. Mesh powstaje w `OnEnable` i po zmianie w Inspectorze, z flagą `HideAndDontSave`.
- W scenie `m_Mesh` jest zawsze `{fileID: 0}`. To celowe, nie błąd: mesh nie jest zapisywany w assetach.
- Warianty: `SilhouetteLayer` (wzgórza/drzewa, geometria w `SilhouetteBuilder`) i `GradientQuad` (niebo, mgła).

## Kolory
Kolory wierzchołków **nie** są konwertowane z sRGB do linear (inaczej niż `SpriteRenderer.color`). Projekt jest w Linear, więc kod konwertuje je ręcznie. Nie dodawaj drugiej konwersji.

## Kolejność rysowania
Materiał: `Sprite-Unlit-Default` z URP. Kolejność daje `MeshRenderer.sortingOrder`:

| Warstwa | order |
|---|---|
| niebo | -100 |
| wzgórza | -90 / -80 / -70 |
| mgły | -85 / -75 / -65 (między wzgórzami) |

Gra (Ground, piłka) ma order >= 0.

## Dodanie warstwy
1. Nowy GameObject pod `Background`.
2. `MeshRenderer` z materiałem `Sprite-Unlit-Default`, `sortingOrder` między sąsiadami.
3. Komponent `SilhouetteLayer` albo `GradientQuad`.
