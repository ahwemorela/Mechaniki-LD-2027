# 04 – Zamiana ciał

**Scena przykładowa:** `Sceny/04_Zamiana_cial`

## Co to jest

Na poziomie są dwie postacie (ciała). Gracz steruje jedną naraz, a klawiszem **Q** przechodzi do drugiej. Porzucone ciało zostaje tam, gdzie stało – i **nadal działa na świat**: wciska przyciski naciskowe, jeździ na platformach, może zostać zauważone przez strażnika.

Kamera nie przeskakuje cięciem, tylko **wylatuje z jednego ciała i wlatuje w drugie** – jakby gracz je opętywał. Lot trwa domyślnie 0,7 s. W tym czasie nikt nie steruje żadną postacią, a świat gry toczy się dalej.

Z tego biorą się zagadki „na dwie osoby dla jednego gracza”: jedno ciało staje na przycisku i otwiera drzwi drugiemu, potem role się odwracają.

## Z czego się składa

| Element | Rola |
|---|---|
| `Prefaby/Gracz/Gracz` | pierwsza postać – z kamerą i interfejsem. **Na scenie tylko jeden.** |
| `Prefaby/Gracz/DrugieCialo` | kolejna postać – sama kapsuła, bez kamery |
| komponent **Gracz Zamiana Cial** (na każdym ciele) | klawisz, warunki, przelot kamery i komunikaty zamiany |
| komponent **Gracz** (na każdym ciele) | `Nazwa Postaci`, `Kolor Ciala`, `Sterowana Na Starcie` |

Każde ciało ma **własne**: ekwipunek (klucze), punkt kontrolny, rozmiar i zestaw umiejętności. Nazwę ciała, którym gracz steruje, widać w prawym górnym rogu ekranu.

## Jak użyć – krok po kroku

1. Na scenie jest już prefab `Gracz`. Przeciągnij obok niego `DrugieCialo` i ustaw tam, gdzie druga postać ma zaczynać.
2. Gotowe – Q przełącza między ciałami.
3. W **Mecie Poziomu** zaznacz `Wymaga Wszystkich Postaci`, jeśli do mety mają dotrzeć oba ciała.

Ciał może być więcej niż dwa – każde kolejne `DrugieCialo` dołącza do kolejki, a Q przechodzi po nich po kolei.

---

## Jak przerobić pod swoją grę

### Zmień znaczenie

| W grze o… | Ciała to… | Zamiana to… |
|---|---|---|
| duchach | duch i opętane postacie | opętanie |
| robotach | operator i drony | przełączenie sygnału |
| rodzeństwie | brat i siostra | „teraz ty” |
| magii | mag i jego chowaniec | przeniesienie świadomości |
| czasie | ty teraz i ty z przeszłości | skok w czasie |

### Przepisy

| Chcę, żeby… | Zrób to |
|---|---|
| …przejść można było tylko do ciała, które się **widzi** | W **Gracz Zamiana Cial** (na każdym ciele) zaznacz `Wymaga Widocznosci`. Ściany, okna i zakręty stają się wtedy częścią zagadki. |
| …zamiana działała tylko **z bliska** | `Maksymalna Odleglosc` = np. 5 (przekazanie pałeczki). |
| …zamiana miała **dłuższą przerwę** | `Czas Odnowienia` (liczony od końca przelotu). |
| …kamera **przeskakiwała od razu**, bez lotu | `Czas Przelotu` = 0. Pasuje do przełączania kamer albo dronów. |
| …lot był **powolny, jak lot ducha** | `Czas Przelotu` = 2–3. Gracz ogląda wtedy poziom z powietrza, ale przez ten czas jest bezbronny – strażnik może w tym czasie dojść do porzuconego ciała. |
| …**dalekie** przeskoki trwały dłużej niż bliskie | `Czas Na Metr` = np. 0,05 (do ciała oddalonego o 20 m leci się o sekundę dłużej). Odległość między ciałami zaczyna wtedy kosztować czas. |
| …lot miał **inne tempo** (np. gwałtowny start) | `Krzywa Przelotu` – kliknij krzywą i przeciągnij punkty. Linia prosta = stała prędkość. |
| …nie było efektu **„pędu”** (poszerzania obrazu) | `Poszerzenie Widoku` = 0. Większa wartość = mocniejszy efekt. |
| …kamera w locie **nie odwracała się** w stronę drugiego ciała | Odznacz `Patrz Na Cel W Locie`. |
| …przelotowi towarzyszył **dźwięk** | `Dzwiek Przelotu` (gra na starcie lotu). Dźwięk lądowania dodasz w `Po Zamianie`. |
| …każde ciało „wylatywało” **inaczej** | Ustawienia przelotu bierzemy z ciała, **z którego** gracz wylatuje – ustaw je osobno w każdym ciele (np. z robota szybko, z ducha powoli). |
| …ciała nazywały się **inaczej** | Komponent **Gracz** → `Nazwa Postaci` („Robot”, „Duch”, „Ania”). |
| …ciała miały **inne kolory** | Komponent **Gracz** → `Kolor Ciala`. W scenie kolor ciała = kolor toru, po którym chodzi. |
| …napis „Sterujesz: {0}” był **inny albo go nie było** | **Gracz Zamiana Cial** → `Po Zamianie` → zmień lub wyczyść `Tekst`. Tam też dodasz dźwięk zamiany. |
| …ciała miały **różne umiejętności** | Włącz/wyłącz komponenty umiejętności osobno w każdym ciele: jedno ma hak, drugie nosi skrzynie. |
| …ciała różniły się **szybkością lub skokiem** | **First Person Controller** każdego ciała: `Move Speed`, `Jump Height`. |
| …jedno ciało było **małe**, drugie duże | Postaw `Pomniejszacz` albo `Powiekszacz` (z `Czas Odnowienia` = 0) dokładnie w miejscu startu tego ciała. |
| …gracz **zaczynał** w drugim ciele | Zaznacz `Sterowana Na Starcie` w tym ciele i odznacz w pierwszym. |
| …do mety wystarczyło **jedno** ciało | W Mecie Poziomu odznacz `Wymaga Wszystkich Postaci`. |
| …komunikat przy mecie był inny | Meta Poziomu → `Gdy Brakuje Postaci`. |
| …zamiana była pod **innym klawiszem** | `Klawisz Zamiany`. Tekst w ściądze: prefab HUD → **Gracz HUD** → `Sterowanie Zamiana`. |
| …nazwy ciała **nie było** na ekranie | W prefabie HUD wyłącz obiekt `Nazwa postaci`. |
| …drugie ciało wyglądało **inaczej** | Otwórz prefab `DrugieCialo` i podmień dziecko `Capsule` na własny model. |

---

## Najważniejsze ustawienia

### Gracz Zamiana Cial

| Pole | Znaczenie |
|---|---|
| `Klawisz Zamiany` | domyślnie Q |
| `Wymaga Widocznosci` | zamiana tylko do ciała, które widać z oczu aktualnej postaci |
| `Maksymalna Odleglosc` | 0 = bez ograniczenia |
| `Czas Odnowienia` | przerwa między zamianami, liczona od końca przelotu |
| `Czas Przelotu` | ile sekund kamera leci do drugiego ciała; 0 = natychmiastowe cięcie |
| `Czas Na Metr` | dodatkowy czas lotu za każdy metr odległości; 0 = lot trwa zawsze tyle samo |
| `Krzywa Przelotu` | tempo lotu: oś pozioma to czas, pionowa to przebyta droga |
| `Poszerzenie Widoku` | o ile stopni poszerza się pole widzenia w połowie lotu |
| `Patrz Na Cel W Locie` | kamera najpierw obraca się w stronę drugiego ciała, a jego spojrzenie przyjmuje pod koniec |
| `Dzwiek Przelotu` | dźwięk na starcie lotu |
| `Po Zamianie` | komunikat i dźwięk po wylądowaniu; {0} = nazwa ciała |
| `Gdy Niemozliwa` | komunikat, gdy warunki nie są spełnione |

### Gracz

| Pole | Znaczenie |
|---|---|
| `Nazwa Postaci` | nazwa pokazywana graczowi |
| `Sterowana Na Starcie` | od którego ciała zaczyna gracz |
| `Kolor Ciala` | kolor modelu i nazwy w interfejsie |

## Jak zachowuje się porzucone ciało

- Stoi w miejscu, ale podlega grawitacji i **jeździ na platformach**.
- **Wciska przyciski naciskowe** i otwiera drzwi automatyczne.
- Może **spaść** – wtedy odradza się we własnym punkcie kontrolnym.
- Może zostać **wykryte** przez strażnika lub kamerę.
- Przy zamianie puszcza niesiony przedmiot i linę haka.
- W czasie przelotu kamery **oba** ciała są porzucone: żadne nie reaguje na klawisze, ale oba nadal mogą spaść, pojechać platformą albo zostać wykryte.

## Liczby do projektowania

- Przepaść między torami ciał: **7 m** – więcej niż najdłuższy skok (ok. 5 m), a ciała dobrze się widzą.
- Przycisk i drzwi, które otwiera: w zasięgu wzroku. Gracz, stojąc na przycisku jednym ciałem, powinien **widzieć** otwierające się drzwi.
- Platforma przewożąca bezczynne ciało: 2,5 m/s – gracz zdąży zobaczyć, że to działa.
- Przelot kamery: **0,7 s** niezależnie od odległości. Przy częstych zamianach dłuższy lot szybko zaczyna nużyć; 2–3 s zostaw na poziomy, w których zamian jest mało, a sam lot ma coś pokazać.

## Wskazówki projektowe

- **Oba ciała w jednym kadrze.** Najlepiej, gdy z każdego miejsca widać drugie ciało. Gracz, który nie wie, gdzie zostawił drugą postać, traci wątek.
- **Kolor toru = kolor ciała.** Gracz od razu wie, która część poziomu „należy” do którego ciała.
- **Najpierw najprostsza wymiana:** przycisk jednego ciała otwiera drzwi drugiemu – i od razu odwrotnie. Dopiero potem wariacje (przewożenie ciała, przełącznik, różne umiejętności).
- **Nie każ biegać tam i z powrotem.** Zamiana ma oszczędzać chodzenie, a nie je mnożyć: po zamianie gracz powinien mieć coś do zrobienia w zasięgu kilku kroków.
- **Meta dla obu ciał** zamyka zagadkę: bez niej gracz porzuci jedno ciało, gdy przestanie być potrzebne.
- **Przelot pokazuje drogę.** Kamera leci po linii prostej od ciała do ciała, więc gracz widzi, co leży między nimi i gdzie względem siebie stoją. Jeśli lot przebija ściany, gracz przez chwilę zagląda do ich wnętrza – przy takich układach skróć lot albo włącz `Wymaga Widocznosci`.
- **Wymaga Widocznosci zmienia wszystko.** Wtedy projektujesz linie wzroku: okno, przez które widać drugie ciało, jest „drzwiami” dla świadomości.

## Pomysły na połączenie z inną mechaniką

- Każde ciało ma własny ekwipunek: klucz leży na torze jednego ciała, a drzwi stoją na torze drugiego – trzeba najpierw otworzyć drogę temu, które ma po klucz pójść.
- Jedno ciało pomniejszone (przechodzi szybami), drugie normalne (nosi skrzynie).
- Jedno ciało odwraca uwagę kamery, drugie przechodzi.
- Tylko jedno ciało ma hak.

## Najczęstsze problemy

| Objaw | Rozwiązanie |
|---|---|
| Q nic nie robi | Na scenie jest tylko jedno ciało albo w **Gracz Zamiana Cial** włączone są warunki, których ciała nie spełniają. |
| Kamera przeskakuje bez lotu | `Czas Przelotu` jest ustawiony na 0 w ciele, z którego gracz wychodzi. |
| Po naciśnięciu Q przez chwilę nie da się ruszyć | To przelot kamery – sterowanie wraca po wylądowaniu. Skróć `Czas Przelotu`, jeśli przeszkadza. |
| Dwie kamery, podwójny interfejs | Na scenie są dwa prefaby `Gracz`. Drugą postacią ma być `DrugieCialo`. |
| Drugie ciało rusza się razem z pierwszym | W prefabie `DrugieCialo` komponent **Player Input** musi być wyłączony (tak jest domyślnie). |
| Poziom kończy się jednym ciałem | W Mecie Poziomu zaznacz `Wymaga Wszystkich Postaci`. |
| Nie wiadomo, którym ciałem się steruje | Nadaj ciałom różne `Kolor Ciala` i `Nazwa Postaci`. |
