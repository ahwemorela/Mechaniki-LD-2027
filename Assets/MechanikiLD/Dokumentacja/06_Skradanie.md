# 06 – Skradanie

**Scena przykładowa:** `Sceny/06_Skradanie`

## Co to jest

Na poziomie są „oczy” – kamery i strażnicy. Każde ma **pole widzenia**: stożek o określonym kącie i zasięgu. Gdy postać stoi w stożku i nic jej nie zasłania, rośnie pasek wykrycia. Gdy się zapełni, postać zostaje wykryta.

Gracz widzi pole widzenia jako **plamę światła** na podłodze. Światło ma ten sam kąt i zasięg co wzrok, a cień rzucają te same bloki, które zasłaniają postać. Zasada jest jedna: **w świetle cię widzą, w cieniu nie**.

## Z czego się składa

| Prefab | Rola |
|---|---|
| `Prefaby/Skradanie/KameraOchrony` | kamera obracająca się w lewo i prawo |
| `Prefaby/Skradanie/Straznik` | strażnik chodzący po trasie (dzieci `Punkt A`, `Punkt B`) |

| Komponent | Rola |
|---|---|
| **Pole Widzenia** | stożek widzenia, wykrywanie, reakcja |
| **Obrót Wahadłowy** | obraca kamerę tam i z powrotem |
| **Ruch Po Trasie** | prowadzi strażnika między punktami |

## Jak użyć – krok po kroku

**Kamera:** przeciągnij `KameraOchrony` na scenę i zawieś na ścianie 4–5 m nad podłogą. Niebieska strzałka = kierunek patrzenia. Dziecko `Glowica` (Obrót Wahadłowy) ustawia zakres i tempo obrotu, dziecko `Oko` (Pole Widzenia) – zasięg, kąt i czas wykrycia.

**Strażnik:** przeciągnij `Straznik`, rozwiń go i ustaw dzieci `Punkt A` i `Punkt B`. Tempo i postoje: **Ruch Po Trasie** na korzeniu; wzrok: **Pole Widzenia** na dziecku `Oko`.

Fioletowe linie w oknie Scene pokazują stożek widzenia, turkusowe – trasę.

---

## Jak przerobić pod swoją grę

### Zmień znaczenie

| W grze o… | „Oczy” to… | Wykrycie to… |
|---|---|---|
| włamaniu | kamery, ochroniarze | alarm |
| horrorze | potwór, reflektor | pościg, śmierć |
| szkole | nauczyciel na korytarzu | powrót do klasy |
| więzieniu | wieża strażnicza | cela |
| przyrodzie | drapieżnik | ucieczka stada |

### Przepisy

| Chcę, żeby… | Zrób to |
|---|---|
| …wykrycie **nie odsyłało** do punktu kontrolnego | **Pole Widzenia** → `Reakcja` = Tylko Sygnał. Dodaj do `Cele` to, co ma się stać: drzwi, które się zamkną, most, który się schowa. |
| …alarm trwał **dłużej / krócej** | `Czas Alarmu`. |
| …napis „Zostałeś wykryty!” był **inny albo go nie było** | „Komunikaty i dźwięki” → `Gdy Wykryto` → zmień lub wyczyść `Tekst`; tam też dodasz dźwięk alarmu. |
| …gracz miał **więcej / mniej czasu** na ukrycie się | `Czas Wykrycia` (domyślnie 1,2 s). 0,3 s = natychmiastowa kara, 3 s = bardzo łagodnie. |
| …strażnik widział **dalej / szerzej** | `Zasieg` i `Kat`. Światło dopasuje się samo. |
| …strażnik **stał w miejscu** | Ustaw `Punkt A` i `Punkt B` w tym samym miejscu albo wyłącz **Ruch Po Trasie**. |
| …strażnik chodził **po trasie z zakrętami** | Zduplikuj punkt, ustaw go i dodaj do listy `Punkty` w **Ruch Po Trasie**. Tryb `W Pętli` da obchód w kółko. |
| …strażnik szedł **szybciej / wolniej**, stał dłużej na końcach | **Ruch Po Trasie** → `Predkosc`, `Czas Postoju`. |
| …kamera obracała się **szerzej / szybciej** | **Obrót Wahadłowy** → `Kat`, `Czas Obrotu`, `Czas Postoju`. |
| …kamera była **nieruchoma** | Wyłącz **Obrót Wahadłowy**. |
| …dwie kamery **nie ruszały się równo** | `Przesuniecie Cyklu` w jednej z nich. |
| …**przełącznik wyłączał** kamerę | Patrz niżej: „Wyłączanie kamery przełącznikiem”. |
| …światło miało **inny kolor** | Kolor spokojny jest w skrypcie; kolor zagrożenia to `Kolor Niebezpieczny` w UstawieniaLD. Jasność: komponent **Light** na `Oko`. |
| …napis „WIDZĄ CIĘ!” i pasek wyglądały **inaczej** | Prefab HUD → obiekt `Wykrywanie`. |
| …strażnik wyglądał jak **potwór / robot** | Otwórz prefab, podmień `Cialo/Model`. Obiekt `Oko` zostaw na wysokości oczu nowego modelu. |
| …strażnik **nie widział** małej postaci za niską osłoną | Działa od razu: pole widzenia sprawdza tułów i głowę postaci w jej aktualnym rozmiarze. |

### Wyłączanie kamery przełącznikiem (zdarzenia)

Pole Widzenia jest aktywatorem, więc nie można go wpisać na listę `Cele`. Użyj zdarzeń:

1. Zaznacz przełącznik. W sekcji `Gdy Wlaczony` kliknij `+`.
2. Przeciągnij do pola obiekt `Oko` kamery.
3. Z listy funkcji wybierz **PoleWidzenia → UstawDzialanie** i zostaw pole wyboru **odznaczone** (= wyłącz).
4. To samo zrób w `Gdy Wylaczony`, ale pole wyboru **zaznacz** (= włącz ponownie).

Gotowy przykład: obiekt „Przelacznik kamery” w scenie.

---

## Najważniejsze ustawienia

### Pole Widzenia

| Pole | Znaczenie |
|---|---|
| `Zasieg` | jak daleko widzi (kamera 14 m, strażnik 9 m) |
| `Kat` | szerokość stożka (kamera 50°, strażnik 70°) |
| `Czas Wykrycia` | ile sekund postać może być widoczna, zanim zostanie wykryta |
| `Czas Zapominania` | jak szybko pasek opada, gdy postać się schowa |
| `Reakcja` | **Odródź Gracza** albo **Tylko Sygnał** |
| `Cele` + `Czas Alarmu` | co reaguje na wykrycie i na jak długo |
| `Gdy Wykryto` | komunikat i dźwięk |

Światło: jasnoniebieskie = spokój, żółte = coś zauważył, czerwone = zaraz wykryje.

## Liczby do projektowania

- **Wysokość osłony: minimum 2,2–2,4 m.** Postać ma 2 m, a pole widzenia sprawdza tułów i głowę. Kamera patrzy z góry, więc niski blok nie zasłoni.
- **Martwe pole pod kamerą:** kamera 4,3 m nad podłogą, pochylona o 35°, nie widzi pasa ok. 2,5 m tuż pod sobą.
- **Strażnik widzi przed sobą i nic za plecami.** Idzie 2 m/s, gracz 4 m/s – można iść za nim.
- **Czas wykrycia 1,2 s** = ok. 7 m biegu przez światło bez konsekwencji.
- **Zawracający strażnik „zamiata” wzrokiem** boki.

## Wskazówki projektowe

- **Gracz musi widzieć zagrożenie wcześniej niż ono jego.** Wejście do pomieszczenia poza zasięgiem światła, z widokiem na całą sytuację.
- **Osłony wyznaczają trasę.** Odległość między osłonami to długość jednego „skoku” przez światło.
- **Rytm ma być czytelny:** postój kamery w skrajnych pozycjach i strażnika na końcach trasy to wyraźny moment startu.
- **Punkt kontrolny tylko w miejscu niewidocznym** – inaczej gracz wpada w pętlę wykryć.
- **Uwaga na drzwi.** Strażnik widzi przez otwarte drzwi automatyczne. Zakończ trasę tak, żeby jego wzrok nie sięgał do następnego pomieszczenia.
- **Kara nie musi być śmiercią.** `Tylko Sygnał` + drzwi w `Cele` = stracony czas zamiast straconego postępu.
- **Dwa rozwiązania są lepsze niż jedno:** przemknąć w rytm kamery albo po cichu ją wyłączyć.

## Pomysły na połączenie z inną mechaniką

- Jedno ciało odwraca uwagę, drugie przechodzi (zamiana ciał – wykryte może zostać także porzucone ciało).
- Pomniejszona postać mieści się za osłoną 0,8 m.
- Klucz leżący w świetle kamery; przycisk naciskowy na trasie patrolu.

## Najczęstsze problemy

| Objaw | Rozwiązanie |
|---|---|
| Kamera nie widzi postaci stojącej w świetle | Postać jest przy samej krawędzi plamy – widać tylko część sylwetki. To normalne. |
| Strażnik widzi przez ścianę | Ściana nie ma collidera. |
| Strażnik stoi w miejscu | `Punkt A` i `Punkt B` są w tym samym miejscu albo odznaczono `Aktywny Bez Sygnalu` w Ruch Po Trasie. |
| Światła kamery prawie nie widać | Scena jest za jasna. W scenie przykładowej słońce ma intensywność 0,4. |
| Postać ginie zaraz po odrodzeniu | Punkt kontrolny jest w polu widzenia. Przesuń go za osłonę. |
