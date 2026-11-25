# 08 – Przerabianie projektu pod własną grę

Ten zestaw to punkt wyjścia. Wszystko jest pomyślane tak, żeby dało się to zmienić **bez programowania**. Każda mechanika ma w swoim dokumencie rozdział „Jak przerobić pod swoją grę” z konkretnymi przepisami – tutaj są zasady wspólne dla wszystkich.

## Cztery poziomy zmian

| Poziom | Gdzie | Co zmienisz |
|---|---|---|
| 1. Jeden obiekt | Inspektor obiektu na scenie | zachowanie, teksty i dźwięki konkretnych drzwi, przycisku, klucza… |
| 2. Wszystkie obiekty jednego typu | prefab (dwuklik w oknie Project) | to samo, ale dla każdych drzwi / każdego klucza naraz |
| 3. Cały projekt | pliki ustawień i prefab HUD | kolory mechanik, rodzaje kluczy, interfejs |
| 4. Kod | `Skrypty/` | wszystko inne (każdy skrypt ma komentarze po polsku) |

Zmiana na scenie **nadpisuje** prefab tylko w tym jednym obiekcie (pole robi się pogrubione). Zmiana w prefabie trafia wszędzie tam, gdzie nie było nadpisania.

## Menu „Mechaniki LD”

Na górnym pasku Unity:

- **Ustawienia: kolory mechanik** – plik ze wspólnymi kolorami,
- **Rodzaje kluczy** – folder z rodzajami kluczy,
- **Edytuj interfejs (HUD)** – prefab interfejsu,
- **Edytuj gracza** – prefab postaci,
- **Pokaż prefaby mechanik**, **Dokumentacja**,
- **Nowy poziom z szablonu…** – kopia sceny `00_Szablon` pod wybraną nazwą.

---

## 1. Komunikaty i dźwięki – na każdym obiekcie

Każda mechanika ma w Inspektorze sekcję **„Komunikaty i dźwięki”**. Składa się z powiadomień – każde to dwa pola:

| Pole | Znaczenie |
|---|---|
| `Tekst` | napis pokazywany graczowi. **Puste pole = brak komunikatu.** |
| `Dzwiek` | plik dźwiękowy (`.wav`, `.ogg`, `.mp3`). **Puste pole = cisza.** |

W tekście możesz użyć `{0}` – w tym miejscu pojawi się nazwa (klucza, postaci). Możesz je też pominąć.

**Przykład – drzwi na klucz** (sekcja „Komunikaty i dźwięki” komponentu Drzwi):

| Powiadomienie | Domyślny tekst | Przykładowa przeróbka |
|---|---|---|
| `Gdy Brak Klucza` | Zamknięte. Potrzebny klucz: {0} | „Skaner odrzuca twoją kartę.” |
| `Gdy Otwarto Kluczem` | Otwarto kluczem: {0} | *(puste – bez komunikatu)* |
| `Gdy Otwieranie` | *(puste)* | sam dźwięk syku hydrauliki |
| `Gdy Zamykanie` | *(puste)* | sam dźwięk trzaśnięcia |

**Jak zmienić tekst w jednym obiekcie:** zaznacz go na scenie i wpisz tekst.
**Jak zmienić we wszystkich:** otwórz prefab (np. `Prefaby/Drzwi/Drzwi_NaKlucz`) i wpisz tekst tam.
**Jak wyłączyć:** wyczyść pole.

Podpowiedzi pod celownikiem („Otwórz”, „Podnieś skrzynię”, „Weź klucz”) to zwykłe pola tekstowe w tym samym miejscu.

Kiedy komunikat jest potrzebny? Gdy świat sam nie mówi tego wystarczająco jasno. Drzwi, które otwierają się na oczach gracza, nie potrzebują napisu „Otwarto”. Drzwi, które otworzyły się trzy pokoje dalej – potrzebują.

Głośność wszystkich dźwięków naraz: plik **UstawieniaLD**, pole `Glosnosc Dzwiekow`.

---

## 2. Rodzaje kluczy – własne, ile chcesz

Klucz i drzwi pasują do siebie, gdy wskazują **ten sam plik „Rodzaj klucza”**. Gotowe cztery leżą w `Ustawienia/Klucze`.

**Nowy rodzaj:**
1. W oknie Project kliknij prawym przyciskiem → **Create → Mechaniki LD → Rodzaj klucza**.
2. Nazwij plik i ustaw w Inspektorze:

| Pole | Znaczenie |
|---|---|
| `Nazwa` | wstawiana w komunikaty w miejscu `{0}`: „czerwony”, „karta ochrony”, „bateria” |
| `Kolor` | kolor klucza, pasa i lampek na drzwiach, ikony w interfejsie |
| `Ikona` | własna ikona w interfejsie (opcjonalnie) |
| `Barw Ikone` | odznacz, jeśli ikona jest już kolorowym obrazkiem |

3. Przeciągnij plik na pole `Rodzaj` klucza i na pole `Wymagany Klucz` drzwi.

Zmiana koloru w pliku od razu przemalowuje klucze i drzwi na otwartej scenie.

---

## 3. Kolory mechanik

Plik `Resources/UstawieniaLD` (menu **Mechaniki LD → Ustawienia**):

| Pole | Co koloruje |
|---|---|
| `Kolor Aktywny` | wciśnięty przycisk, otwarte drzwi, aktywny punkt kontrolny |
| `Kolor Nieaktywny` | przycisk, przełącznik, zamknięte drzwi, punkt kontrolny – wszystko, co czeka na gracza |
| `Kolor Niebezpieczny` | pasek wykrycia, światło strażnika tuż przed wykryciem |
| `Kolor Haka`, `Kolor Tras`, `Kolor Straznika`, `Kolor Polaczen` | linie i podglądy w oknie Scene |

> Plik musi nazywać się `UstawieniaLD` i leżeć w folderze `Resources`. Nie przenoś go.

Część elementów ma kolor zapisany w **materiale** (`Materialy/`): `Mech_Hak` (punkty haka), `Mech_Zielony` (meta), `Mech_Platforma`, `Mech_Drzwi`, `Mech_Skrzynia`, `Mech_Straznik`, `Mech_StrefaSmierci`, `Mech_Ciemny` (ramy, podstawy). Materiały `Blok_*`: `Color` to kolor bloku, `GridColor` – kolor siatki.

Kolor postaci i przedmiotów zmiany skali ustawia się na nich samych (`Kolor Ciala`, `Kolor`).

---

## 4. Interfejs (HUD)

Prefab `Prefaby/UI/HUD` (menu **Mechaniki LD → Edytuj interfejs**). Każdy element jest osobnym obiektem. Prefab **Gracz** zawiera HUD jako dziecko, więc zmiana w prefabie HUD pojawia się we wszystkich scenach.

W oknie Scene włącz tryb **2D** i wciśnij **F** przy zaznaczonym HUD. W prefabie (i w oknie Game poza trybem Play) widać wszystkie elementy naraz z przykładowym tekstem – w grze pojawiają się tylko w odpowiedniej sytuacji.

| Obiekt | Kiedy widoczny w grze | Co warto zmienić |
|---|---|---|
| `Celownik` (+ `Ramka interakcji`, `Ramka haka`) | zawsze; ramki – gdy można czegoś użyć / zaczepić hak | grafika, rozmiar, kolory |
| `Tekst pod celownikiem` | gdy gracz patrzy na coś użytecznego | położenie, rozmiar czcionki |
| `Klucze` | gdy postać ma klucz | położenie, etykieta, ikona |
| `Nazwa postaci` | gdy na scenie jest więcej niż jedna postać | położenie, czcionka |
| `Odliczanie` | przełącznik czasowy, czasowa zmiana skali | położenie, kolory paska |
| `Wykrywanie` | gdy strażnik widzi postać | napis, kolory paska |
| `Komunikat` | krótko po zdarzeniu | tło, czcionka, położenie |
| `Podpowiedz` | w strefie Podpowiedź | tło, kolory, szerokość |
| `Sterowanie` | zawsze | treść ściągi, położenie |
| `Zaciemnienie` | przy odradzaniu | kolor (np. czerwony błysk) |
| `Ekran konca` | po dojściu do mety | napisy, układ. **Jest wyłączony** – włącz obiekt, żeby go edytować, i wyłącz z powrotem |

Skrypt **Gracz HUD** na obiekcie `HUD` wskazuje, którym elementem steruje. **Każde pole może być puste** – wtedy tej części interfejsu po prostu nie ma.

**Najczęstsze zmiany**

- **Przesunąć / przeskalować** – zaznacz element, narzędzie **Rect Tool (T)**. Kotwice (Anchor Presets) decydują, do którego rogu ekranu element jest przypięty.
- **Wyłączyć** – odznacz obiekt albo go usuń.
- **Czcionka całego interfejsu** – wrzuć plik `.ttf`/`.otf`, PPM → **Create → TextMeshPro → Font Asset → SDF**, przeciągnij wynik na pole `Czcionka` w **Gracz HUD**.
- **Grafika** (celownik, ikona klucza, tła) – wrzuć własny `.png` (Texture Type = **Sprite (2D and UI)**) i przeciągnij na `Source Image`. Gotowe białe grafiki są w `UI/Grafiki`.
- **Stałe napisy** („WIDZĄ CIĘ!”, „Klucze:”, „POZIOM UKOŃCZONY”) – zaznacz obiekt tekstu i wpisz własny.
- **Wzory tekstów** w **Gracz HUD**: `Format Interakcji` (`[E]  {0}`), `Tekst Haka`, `Format Odliczania`, `Format Wyniku` ({0} = czas, {1} = odrodzenia), `Format Postaci`.
- **Ściąga sterowania** – układa się sama z linijek `Sterowanie Ruch`, `Sterowanie Hak`, `Sterowanie Zamiana`… tylko dla umiejętności, które postać ma włączone. Odznacz `Sterowanie Automatyczne`, żeby wpisać własny tekst.
- **Paski** – ustaw w Image `Image Type` = **Filled**, a pasek będzie się wypełniał (np. kołowo) zamiast skracać.
- **Własny element** (logo, nazwa poziomu, ramka) – dodaj dowolny obiekt UI jako dziecko `HUD`.

**Inny interfejs w jednej scenie:** zaznacz na scenie `Gracz → HUD` i zmień – zapisze się tylko tam.
**Zachowanie oryginału:** PPM na prefabie `HUD` → **Create → Prefab Variant**; w prefabie Gracz podmień HUD na wariant i przeciągnij go na pole `Hud` komponentu **Gracz**.

---

## 5. Wygląd mechanik

Każdy prefab ma tę samą budowę: **korzeń ze skryptem** i **dzieci z wyglądem**. Skrypt nie zależy od tego, jak wygląd jest zrobiony.

**Podmiana modelu:**
1. Otwórz prefab (dwuklik).
2. Usuń lub wyłącz dzieci tworzące wygląd i wstaw własny model w to samo miejsce.
3. Jeśli usunięty element miał **collider** (skrzydło drzwi, płytka przycisku), dodaj collider do nowego.
4. Lista `Kolorowane` (elementy zmieniające kolor ze stanem): przeciągnij tam Renderer nowego modelu albo zostaw pustą, jeśli kolor ma się nie zmieniać.
5. Pola wskazujące ruchome części (`Skrzydlo`, `Plytka`, `Obiekt Ruchomy`, `Model`) muszą wskazywać nowy obiekt.

Elementy z listy `Kolorowane` powinny używać materiału `Mech_Swiecacy` (albo innego materiału URP/Lit z włączoną emisją).

**Własny wariant prefabu:** PPM → **Create → Prefab Variant**, np. „Drzwi_Krata” albo „Klucz_Karta”. Oryginał zostaje nietknięty.

---

## 6. Gracz

Prefab `Prefaby/Gracz/Gracz` (menu **Mechaniki LD → Edytuj gracza**), obiekt `PlayerCapsule`:

| Co | Gdzie |
|---|---|
| prędkość, wysokość skoku, grawitacja | **First Person Controller**: `Move Speed`, `Sprint Speed`, `Jump Height`, `Gravity` |
| czułość myszy | **First Person Controller**: `Rotation Speed` |
| kąt widzenia kamery | `PlayerFollowCamera` → **Cinemachine Camera** → `Lens` → `Vertical FOV` (65) |
| nazwa i kolor postaci | **Gracz**: `Nazwa Postaci`, `Kolor Ciala` |
| umiejętności | komponenty **Gracz Interakcja / Podnoszenie / Hak / Zamiana Cial / Skala** – pole wyboru przy nazwie włącza i wyłącza |
| klawisze | pola `Klawisz …` w komponentach umiejętności |
| komunikaty odradzania | **Gracz Odrodzenie**: `Komunikat Powrotu`, `Komunikat Poza Mapa`, `Dzwiek Odrodzenia` |
| przepychanie skrzyń ciałem | **Basic Rigid Body Push**: `Can Push` |

**Po zmianie ruchu postaci zmierz poziom od nowa** i zaktualizuj te same liczby w prefabie **MetrykiGracza**.

---

## 7. Dla chętnych: kod

- `Podstawy/` – sygnały (`Aktywator`, `ObiektAktywowalny`), `Powiadomienie`, `RodzajKlucza`, kolory (`Paleta`, `UstawieniaLD`).
- `Gracz/` – umiejętności postaci i interfejs.
- `Mechaniki/` – po jednym pliku na mechanikę; każdy zaczyna się komentarzem „JAK PRZEROBIĆ POD SWOJĄ GRĘ”.

Własny cel dla przycisków to klasa dziedzicząca po `ObiektAktywowalny` z jedną metodą `GdyZmianaStanu`. Bez kodu to samo osiągniesz zdarzeniami (`Gdy Wlaczony`, `Gdy Aktywowany`, `Gdy Ukonczono`) – podepniesz pod nie światło, animację albo Timeline.
