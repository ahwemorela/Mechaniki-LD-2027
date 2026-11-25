# 07 – Elementy wspólne

Elementy, które przydają się w każdym poziomie, niezależnie od wybranych mechanik. Przy każdym jest krótka ściąga **„Jak przerobić”**.

## Gracz

`Prefaby/Gracz/Gracz` – postać, kamera i interfejs w jednym. Na scenie powinien być **jeden** (drugą postacią jest prefab `DrugieCialo` – patrz [04_Zamiana_cial.md](04_Zamiana_cial.md)). Miejsce, w którym stoi w edytorze, jest punktem startu i pierwszym punktem kontrolnym. Niebieska strzałka = kierunek patrzenia po starcie.

**Ustaw start świadomie.** To, co gracz widzi w pierwszej sekundzie, mówi mu, dokąd iść.

**Jak przerobić:** umiejętności włącza się polami wyboru przy komponentach na obiekcie `PlayerCapsule`; prędkość i skok są w **First Person Controller**; interfejs to dziecko `HUD`. Szczegóły: [08_Dostosowywanie.md](08_Dostosowywanie.md).

## Punkt kontrolny

`Prefaby/Poziom/PunktKontrolny`

Po przejściu przez niego postać odradza się właśnie tutaj. Płyta i flaga zmieniają kolor z pomarańczowego na zielony.

- Dziecko `Miejsce Odrodzenia` wyznacza pozycję i kierunek patrzenia po odrodzeniu.
- Klawisz **R** przenosi postać do ostatniego punktu kontrolnego (wyjście awaryjne).
- Postać, która spadnie poniżej Y = −30, odradza się automatycznie.

**Zasada:** punkt kontrolny przed trudnym fragmentem i po każdym zaliczonym. Nigdy w zasięgu zagrożenia.

| Jak przerobić | |
|---|---|
| inny napis albo brak napisu | `Po Aktywacji` → zmień lub wyczyść `Tekst` |
| dźwięk zapisu | `Po Aktywacji` → `Dzwiek` |
| niewidzialny punkt kontrolny | usuń dzieci `Plyta`, `Maszt`, `Flaga` – strefa działa dalej |
| punkt na całą szerokość korytarza | zmień `Size` w Box Collider |
| inny model (ognisko, terminal) | podmień dzieci; wyczyść `Kolorowane`, jeśli kolor ma się nie zmieniać |
| komunikat po wciśnięciu R / wypadnięciu z mapy | Gracz → PlayerCapsule → **Gracz Odrodzenie** |

## Strefa śmierci

`Prefaby/Poziom/StrefaSmierci`

Półprzezroczysty czerwony obszar. Postać, która go dotknie, wraca do punktu kontrolnego; skrzynie wracają na miejsce startowe. Rozmiar zmieniasz skalą obiektu.

| Jak przerobić | |
|---|---|
| napis po odrodzeniu („Spadłeś w przepaść”, „Lawa!”) | pole `Komunikat` (puste = bez napisu) |
| niewidzialna strefa | wyłącz komponent **Mesh Renderer** |
| lawa, woda, kolce | zmień materiał albo wstaw własny model jako dziecko |
| ruchome zagrożenie (walec, piła) | zrób strefę dzieckiem `Platforma` w prefabie RuchomaPlatforma |

## Meta poziomu

`Prefaby/Poziom/MetaPoziomu`

Zielona strefa kończąca poziom. Pokazuje ekran z **czasem przejścia** i **liczbą odrodzeń**; R uruchamia poziom od nowa. Te liczby to dane do testów: dużo odrodzeń w jednym miejscu = tam jest problem.

| Jak przerobić | |
|---|---|
| wygląd ekranu końcowego | prefab HUD → obiekt `Ekran konca` (włącz go, żeby edytować) |
| inny model (portal, winda, drzwi) | podmień dzieci `Plyta` i `Slupek` |
| do mety muszą dojść wszystkie ciała | zaznacz `Wymaga Wszystkich Postaci` |
| dźwięk / animacja / Timeline na koniec | `Po Ukonczeniu` (dźwięk) albo zdarzenie `Gdy Ukonczono` |

## Podpowiedź

`Prefaby/Poziom/Podpowiedz`

Niewidzialna strefa. Gdy sterowana postać w niej stoi, na dole ekranu wyświetla się panel z tekstem.

- `Tytul`, `Tresc` – tekst panelu; `Tylko Raz` – pokaż tylko przy pierwszym wejściu.
- Rozmiar strefy: `Size` w **Box Collider**.

**Tekst to ostatnia deska ratunku.** Najpierw spróbuj nauczyć gracza samą przestrzenią.

**Jak przerobić:** tej samej strefy użyj do narracji – myśli bohatera, notatka na ścianie, głos z radia. Wygląd panelu: prefab HUD → obiekt `Podpowiedz`.

## Przełącznik

`Prefaby/Przyciski/Przelacznik` i `Przelacznik_Czasowy`

Pulpit z przyciskiem. Gracz patrzy na niego i wciska E; przełącznik wysyła sygnał do obiektów z listy `Cele` (tak jak przycisk naciskowy).

| Pole | Znaczenie |
|---|---|
| `Tryb` | **Przełączany** – włącz/wyłącz; **Czasowy** – działa przez określony czas (na ekranie odliczanie); **Jednorazowy** |
| `Czas Dzialania` | ile sekund działa w trybie Czasowym |
| `Opis` | tekst po `[E]` – napisz, CO przełącznik robi („Otwórz śluzę”) |

| Jak przerobić | |
|---|---|
| zagadka „wciśnij i zdąż dobiec” | tryb Czasowy; zmierz trasę biegiem i dodaj 2–3 s zapasu |
| dźwięk / napis po użyciu | `Gdy Wlaczony Komunikat`, `Gdy Wylaczony Komunikat` |
| dźwignia, terminal, zawór | podmień model; pole `Dzwignia` pozwala wskazać część, która się przechyla |
| uruchomienie czegokolwiek | zdarzenia `Gdy Wlaczony` / `Gdy Wylaczony` |

## Ruchoma platforma (Ruch Po Trasie)

`Prefaby/Platformy/RuchomaPlatforma`

```
RuchomaPlatforma      <- komponent Ruch Po Trasie
 ├─ Platforma         <- to się rusza
 ├─ Punkt A           <- puste obiekty wyznaczające trasę
 └─ Punkt B
```

Przesuń `Punkt A` i `Punkt B` – turkusowa linia i ramki w oknie Scene pokażą trasę. Platforma startuje zawsze w pierwszym punkcie. Postać i skrzynie stojące na platformie jadą razem z nią, także w górę i w dół.

| Pole | Znaczenie |
|---|---|
| `Tryb` | **Tam I Z Powrotem** (A→B→A), **W Pętli** (A→B→C→A), **Na Sygnał Do Końca** (winda / most: z sygnałem jedzie do końca, bez – wraca) |
| `Predkosc` | m/s (gracz idzie 4, biegnie 6) |
| `Czas Postoju` | postój na końcach – czas na wejście i zejście |
| `Aktywny Bez Sygnalu` | zaznaczone: jeździ zawsze. Odznacz, żeby ruszała dopiero po sygnale |

| Jak przerobić | |
|---|---|
| więcej punktów trasy | zduplikuj punkt, ustaw go, dodaj do listy `Punkty` |
| inny rozmiar | skala dziecka `Platforma/Bryla` |
| winda | punkty jeden nad drugim, tryb `Na Sygnał Do Końca`, przycisk lub przełącznik w `Cele` |
| wysuwany most | długa, wąska platforma; punkt A schowany w ścianie |
| ruchome zagrożenie | Strefa Śmierci jako dziecko `Platforma` |
| ruchomy punkt haka | komponent **Punkt Haka** na `Platforma/Bryla` |

Szczelina między platformą a brzegiem: do 0,5 m, jeśli gracz ma wejść bez skoku. Postój min. 1,5 s.

## Notatka projektanta

`Prefaby/Poziom/NotatkaProjektanta`

Tekst widoczny **tylko w oknie Scene** – w grze go nie ma. Zapisuj nią zamiar („tu gracz ma zobaczyć klucz przez kratę”), uwagi z testów i rzeczy do poprawy. Pole `Kolor` pozwala odróżnić rodzaje notatek – w scenach przykładowych żółte dotyczą projektowania, niebieskie przerabiania.

## Metryki gracza (miarka)

`Prefaby/Poziom/MetrykiGracza`

Rysuje w oknie Scene sylwetkę postaci, wysokość skoku oraz łuki skoku z chodu (zielony) i z biegu (żółty). Postaw ją przy krawędzi przepaści: jeśli żółty łuk nie sięga drugiego brzegu, skoku nie da się wykonać. `Pokaz Zasieg Haka` rysuje sferę zasięgu haka.

Łuki są policzone ostrożnie (skok 1,2 m w górę, 3,2 m i 4,8 m w dal) – w grze postać skacze odrobinę dalej. **Jeśli zmienisz ruch postaci, wpisz te same liczby tutaj.**

## Blockout

`Prefaby/Blockout` – kostki 1×1×1 m w sześciu kolorach, rampa (1×1×2 m) i schody (2×1×2 m).

Materiały `Materialy/Blok_*` mają siatkę co 1 m rysowaną **w przestrzeni świata**: skalowanie bloku nie rozciąga siatki, więc zawsze widać prawdziwe wymiary.

| Materiał | Proponowane użycie |
|---|---|
| `Blok_Podloga` | podłogi |
| `Blok_Sciana` | ściany |
| `Blok_Jasny` | platformy, półki, osłony |
| `Blok_Ciemny` | tło, konstrukcje |
| `Blok_Niebieski`, `Blok_Bezowy` | punkty orientacyjne, strefy, tory różnych postaci |

**Jak przerobić:** w materiale pole `Color` to kolor bloku, `GridColor` – kolor siatki. Własny kolor: zduplikuj materiał (Ctrl+D) i zmień.

Wygodne wymiary: korytarz min. 3 m szerokości, drzwi 2×3 m, ściana 4–5 m wysokości, stopień do 0,25 m, półka „na skok” do 1,2 m.
