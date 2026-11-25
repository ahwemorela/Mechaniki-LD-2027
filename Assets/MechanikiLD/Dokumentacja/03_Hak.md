# 03 – Hak z liną

**Scena przykładowa:** `Sceny/03_Hak`

## Co to jest

Gracz strzela hakiem w wyznaczony punkt, zawisa na linie i buja się jak na wahadle. Puszczając linę w odpowiednim momencie, leci dalej z rozpędu. Może też wciągnąć się w stronę punktu zaczepienia.

| Sterowanie | Działanie |
|---|---|
| LPM (przytrzymaj) | zaczep hak i trzymaj linę; puszczenie przycisku odczepia |
| PPM (przytrzymaj) | wciąganie – lina się skraca |
| Spacja | puszczenie liny z dodatkowym wybiciem w górę |
| W S A D | rozbujanie się i korekta lotu |

Gdy gracz celuje w miejsce, o które da się zaczepić, celownik zmienia się w **żółtą ramkę**.

## Z czego się składa

| Element | Rola |
|---|---|
| komponent **Gracz Hak** (na postaci) | cała logika haka i liny |
| `Prefaby/Hak/PunktHaka_Kula` | żółta kula – lina łapie zawsze jej środek |
| `Prefaby/Hak/PunktHaka_Belka` | żółta belka 4 m – lina łapie tam, gdzie celował gracz |
| `Prefaby/Hak/PunktHaka_Panel` | żółty panel 3×3 m, np. na ścianę albo sufit |

## Jak użyć – krok po kroku

1. Upewnij się, że na postaci włączony jest komponent **Gracz Hak**.
2. Przeciągnij na scenę `PunktHaka_Kula` i zawieś ją nad przepaścią.
3. Zaznacz kulę: żółta sfera w oknie Scene pokazuje **zasięg haka (22 m)**. Krawędź, z której gracz ma strzelać, musi leżeć wewnątrz sfery.
4. Przetestuj w trybie Play. Hak wymaga strojenia „na czuja” bardziej niż inne mechaniki.

---

## Jak przerobić pod swoją grę

### Zmień znaczenie

| W grze o… | Hak to… | Punkt haka to… |
|---|---|---|
| superbohaterze | pajęcza nić, lina z kotwiczką | latarnia, gzyms, dźwig |
| dżungli | liana | gałąź |
| kosmosie | wiązka przyciągająca | magnetyczny węzeł |
| magii | zaklęcie przyciągania | runa, kryształ |

### Przepisy

| Chcę, żeby… | Zrób to |
|---|---|
| …**dowolny obiekt** był punktem haka | Dodaj mu komponent **Punkt Haka** (musi mieć collider). Belka, lampa, gałąź, ruchoma platforma. |
| …hak łapał **każdą powierzchnię** | Gracz → PlayerCapsule → **Gracz Hak** → odznacz `Tylko Punkty Haka`. To zupełnie inna gra – projektant traci kontrolę nad trasą. |
| …hak sięgał **dalej / bliżej** | `Zasieg`. Zmienia skalę całego poziomu: przy 12 m poziom jest ciasny i precyzyjny, przy 40 m – otwarty i szybki. Ustaw tę samą wartość w `Podglad Zasiegu` punktów haka. |
| …hak **tylko przyciągał**, bez bujania | Zwiększ `Predkosc Wciagania` i naucz gracza trzymać PPM; punkty stawiaj na ścianach i sufitach nad półkami. |
| …hak **tylko bujał**, bez wciągania | Ustaw `Predkosc Wciagania` = 0. |
| …łatwiej było **trafić** w punkt | Zwiększ `Pomoc W Celowaniu` (promień, w jakim hak sam znajdzie punkt obok celownika). |
| …gracz miał więcej **kontroli w powietrzu** | Zwiększ `Sterowanie W Powietrzu`. |
| …lina wyglądała **inaczej** | `Material Liny` i `Grubosc Liny`. |
| …hak **wydawał dźwięk** | „Komunikaty i dźwięki” → `Gdy Zaczepiony` / `Gdy Puszczony`. |
| …napis pod celownikiem był inny niż „Zaczep hak” | Prefab HUD → **Gracz HUD** → `Tekst Haka`. Kolor ramki: `Celownik Hak`. |
| …punkty haka miały **inny kolor** | Materiał `Mech_Hak`. Pamiętaj: ten kolor ma znaczyć „hak” i nic innego. |
| …hak był pod **innym klawiszem** | **Gracz Hak** → `Klawisz Haka`, `Klawisz Wciagania`. |
| …hak miał tylko **jedno z ciał** (zamiana ciał) | Wyłącz **Gracz Hak** w drugim ciele. |

---

## Najważniejsze ustawienia (komponent Gracz Hak)

| Pole | Znaczenie |
|---|---|
| `Zasieg` | maksymalna odległość strzału (22 m) |
| `Minimalna Odleglosc` | hak nie zaczepi się o coś tuż przed nosem |
| `Tylko Punkty Haka` | łapie tylko obiekty z komponentem Punkt Haka |
| `Pomoc W Celowaniu` | promień „przyciągania” celownika do punktu |
| `Predkosc Wciagania` | jak szybko lina się skraca (13 m/s) |
| `Minimalna Dlugosc Liny` | bliżej punktu gracz się nie wciągnie (1,5 m) |
| `Sterowanie W Powietrzu` | wpływ WSAD na ruch na linie |
| `Wybicie Przy Puszczeniu` | dodatkowa prędkość w górę po puszczeniu spacją |

Komponent **Punkt Haka**: `Przyciagaj Do Srodka` (lina łapie środek obiektu albo miejsce trafienia), `Podglad Zasiegu`.

## Jak działa wahadło – liczby do projektowania

Lina ma taką długość, jaka była odległość gracza od punktu w chwili zaczepienia.

- **Wahadło oddaje wysokość, ale jej nie dodaje.** Platforma docelowa powinna być **niżej** niż startowa – w scenie o 3 m.
- **Sprawdzony układ „pierwszego wahadła”:** punkt haka 8 m wyżej i 8 m dalej niż krawędź; lądowanie 3 m niżej, zaczynające się ok. 14,5 m od krawędzi.
- **Punkt haka nad środkiem przepaści**, nie nad jej końcem.
- **Wciąganie:** gracz zawisa ze stopami ok. **2,7 m poniżej** punktu. Żeby wylądował na platformie pod punktem, punkt musi wisieć co najmniej ok. 3,5 m nad nią.
- **Łańcuch:** w scenie punkty wiszą co 18 m na tej samej wysokości, a meta jest 8 m niżej niż start.
- **Pod trasą haka zawsze Strefa Śmierci** i punkt kontrolny na każdej platformie.

## Wskazówki projektowe

- **Naucz jednego ruchu naraz:** wahadło → wciąganie → łańcuch. Każdy etap na własnej platformie.
- **Pierwszy skok ma wybaczać:** szeroka platforma docelowa, wyraźnie niżej.
- **Żółty = hak, zawsze i tylko.**
- **Pokaż punkt na tle nieba albo ciemnej ściany** – gracz musi go zauważyć z daleka.
- **Hak zmienia skalę poziomu:** przepaści 15–20 m, różnice wysokości 5–10 m.

## Pomysły na połączenie z inną mechaniką

- Punkt haka na ruchomej platformie.
- Hak ma tylko jedno z dwóch ciał – drugie musi mu otworzyć drogę.
- Klucz na półce, na którą da się tylko wciągnąć; hak nad polem widzenia strażnika.

## Najczęstsze problemy

| Objaw | Rozwiązanie |
|---|---|
| Celownik nie robi się żółty | Punkt jest dalej niż zasięg, bliżej niż 2 m albo zasłonięty; albo obiekt nie ma komponentu **Punkt Haka** lub collidera. |
| Gracz uderza w ścianę platformy docelowej | Platforma jest za wysoko. Obniż ją albo przesuń punkt haka dalej nad przepaść. |
| Gracz nie dolatuje | Punkt wisi za nisko albo za blisko startu. |
| LPM rzuca skrzynią zamiast strzelać hakiem | Gdy postać coś niesie, LPM to rzut. Najpierw odłóż przedmiot (E). |
