# 01 – Klucze i drzwi

**Scena przykładowa:** `Sceny/01_Klucze_i_drzwi`

## Co to jest

Gracz zbiera przedmiot (klucz), który pozwala przejść dalej (otwiera drzwi wymagające klucza tego samego rodzaju). To najprostszy sposób, żeby **sterować kolejnością zwiedzania poziomu**: gracz widzi zamknięte przejście, szuka klucza, wraca.

## Z czego się składa

| Element | Rola |
|---|---|
| `Prefaby/Klucze/Klucz` | przedmiot do zebrania |
| `Prefaby/Drzwi/Drzwi_NaKlucz` | drzwi z zamkiem |
| `Prefaby/Drzwi/Drzwi` | drzwi otwierane sygnałem (przyciskiem, przełącznikiem) |
| `Prefaby/Drzwi/Drzwi_Automatyczne` | drzwi otwierające się, gdy postać podejdzie |
| `Ustawienia/Klucze/Klucz_*` | **rodzaje kluczy** – pliki z nazwą, kolorem i ikoną |

Klucz i drzwi pasują do siebie, gdy wskazują **ten sam plik rodzaju klucza**. Zebrane klucze widać w lewym górnym rogu ekranu.

## Jak użyć – krok po kroku

1. Przeciągnij na scenę `Drzwi_NaKlucz` i wstaw w otwór w ścianie (2,8 m szerokości z ramą, 3,4 m wysokości).
2. W komponencie **Drzwi** przeciągnij rodzaj klucza na pole `Wymagany Klucz`.
3. Przeciągnij na scenę `Klucz` i ustaw mu ten sam plik w polu `Rodzaj`.

Pas na drzwiach, lampki i klucz przyjmą kolor rodzaju.

---

## Jak przerobić pod swoją grę

### Zmień znaczenie – mechanika zostaje ta sama

„Klucz” to dowolna rzecz, którą trzeba mieć, żeby przejść dalej:

| W grze o… | Klucz to… | Drzwi to… |
|---|---|---|
| stacji kosmicznej | karta dostępu | śluza |
| nawiedzonym domu | stary klucz, świeca | zamknięty pokój, ciemny korytarz |
| laboratorium | bateria, bezpiecznik | brama pod napięciem |
| świątyni | kryształ, pieczęć | kamienna płyta |
| mieście | bilet, przepustka | bramka, szlaban |

### Przepisy

| Chcę, żeby… | Zrób to |
|---|---|
| …był **własny rodzaj klucza** (inny kolor, inna nazwa) | PPM w oknie Project → **Create → Mechaniki LD → Rodzaj klucza**. Ustaw `Nazwa` i `Kolor`, przeciągnij plik na `Rodzaj` klucza i `Wymagany Klucz` drzwi. |
| …po otwarciu **nie było napisu** „Otwarto kluczem: czerwony” | Zaznacz drzwi → sekcja „Komunikaty i dźwięki” → `Gdy Otwarto Kluczem` → wyczyść pole `Tekst`. |
| …napis był **inny** | W tym samym polu wpisz własny tekst, np. „Śluza odblokowana”. `{0}` wstawia nazwę klucza – możesz je pominąć. |
| …zmienić tekst we **wszystkich** drzwiach naraz | Otwórz prefab `Drzwi_NaKlucz` i zmień tekst tam. |
| …drzwi **wydawały dźwięk** | Przeciągnij plik dźwiękowy na `Dzwiek` w `Gdy Otwieranie` / `Gdy Zamykanie`. |
| …klucz zbierało się **klawiszem E**, a nie dotykiem | W kluczu odznacz `Zbieraj Dotykiem`. Tekst podpowiedzi: `Podpowiedz Interakcji`. |
| …klucz wyglądał jak **karta / bateria / kryształ** | Otwórz prefab `Klucz`, usuń dzieci obiektu `Model` i wstaw własny model. Wyczyść listę `Kolorowane`, jeśli model ma zachować własne kolory. |
| …klucz **nie znikał** z ekwipunku po użyciu | W drzwiach odznacz `Zuzywa Klucz` – jeden klucz otworzy wszystkie drzwi tego rodzaju (klucz uniwersalny). |
| …drzwi **zamykały się** za graczem | Odznacz `Zostaja Otwarte`. |
| …drzwi otwierało się **ręcznie** (E) | `Sposob Otwierania` = Interakcja. Teksty podpowiedzi: `Podpowiedz Otworz`, `Podpowiedz Brak Klucza` itd. |
| …drzwi przesuwały się **w bok** albo **obracały** na zawiasach | `Przesuniecie` = (2.1, 0, 0) dla ruchu w bok. Albo `Rodzaj Ruchu` = Obrót i `Obrot` = (0, 100, 0). Zielona ramka w oknie Scene pokazuje pozycję po otwarciu. |
| …drzwi otwierały się **wolno i groźnie** | Zwiększ `Czas Ruchu`. |
| …zebranie klucza **coś uruchamiało** (alarm, zamknięcie bramy) | Klucz jest aktywatorem: dodaj obiekt do jego listy `Cele`. |
| …w interfejsie była **własna ikona** klucza | W pliku rodzaju klucza ustaw `Ikona`. |
| …postać **zaczynała** z kluczem | Gracz → PlayerCapsule → **Gracz Ekwipunek** → dodaj rodzaj do listy `Klucze`. |

W scenie przykładowej: czerwone drzwi mają teksty domyślne, zielone – własne („Magazyn zamknięty…”), a niebieski klucz i niebieskie drzwi mają komunikaty wyłączone. Zaznacz je i porównaj.

---

## Najważniejsze ustawienia

### Klucz

| Pole | Znaczenie |
|---|---|
| `Rodzaj` | plik rodzaju klucza – musi być ten sam co w drzwiach |
| `Zbieraj Dotykiem` | zbieranie przez wejście w klucz albo klawiszem E |
| `Po Zebraniu` | komunikat i dźwięk po zebraniu |
| `Predkosc Obrotu`, `Wysokosc Unoszenia` | ruch modelu (0 = stoi nieruchomo) |
| `Cele` | obiekty uruchamiane w chwili zebrania |

### Drzwi

| Pole | Znaczenie |
|---|---|
| `Sposob Otwierania` | **Tylko Sygnał** – otwiera je aktywator; **Interakcja** – klawisz E; **Automatycznie** – gdy postać podejdzie |
| `Wymagany Klucz` | rodzaj klucza. **Puste = bez zamka.** Działa ze sposobami Interakcja i Automatycznie |
| `Zuzywa Klucz` | czy otwarcie zabiera klucz |
| `Zostaja Otwarte` | raz otwarte już się nie zamkną |
| `Przesuniecie` / `Obrot` / `Czas Ruchu` | ruch skrzydła |
| `Zasieg Automatu` | z jak daleka drzwi automatyczne wykrywają postać |
| `Znaczniki Stanu` | lampki i pas: kolor klucza = zamek, pomarańczowy = zamknięte, zielony = otwarte. Pusta lista = drzwi nie zmieniają koloru |

## Wskazówki projektowe

- **Zamek przed kluczem.** Ustaw start tak, żeby gracz najpierw zobaczył zamknięte drzwi, a dopiero potem klucz.
- **Pokaż klucz, zanim da się go wziąć** – przez kratę, za przepaścią, na wysokiej półce. Gracz wie, czego chce, i szuka drogi.
- **Łańcuch kluczy** (klucz A → drzwi A → klucz B → drzwi B) wymusza kolejność. Po trzecim ogniwie gracz zaczyna się gubić.
- **Powrót ma być krótszy niż droga po klucz.** Otwórz skrót (klucz jako aktywator drzwi) albo zapętl przestrzeń.
- **Kolor wystarczy.** Czerwony pas i czerwony klucz mówią to samo co napis – dlatego napis można wyłączyć.

## Pomysły na połączenie z inną mechaniką

- Klucz we wnęce dostępnej tylko dla pomniejszonej postaci (jest w scenie `05_Zmiana_skali`).
- Klucz ma jedno ciało, a drzwi stoją na torze drugiego (zamiana ciał – każde ciało ma własny ekwipunek).
- Klucz za drzwiami otwieranymi przyciskiem naciskowym; klucz pilnowany przez kamerę.

## Najczęstsze problemy

| Objaw | Rozwiązanie |
|---|---|
| Postać ma klucz, a drzwi się nie otwierają | Klucz i drzwi wskazują różne pliki rodzaju albo `Sposob Otwierania` = Tylko Sygnał. |
| Klucza nie da się zebrać | Pole `Rodzaj` jest puste (w konsoli pojawia się ostrzeżenie) albo klucz stoi w bryle. |
| Klucz jest biały | Puste pole `Rodzaj` albo pusta lista `Kolorowane`. |
| Drzwi zamykają się za graczem i klucz przepadł | Zaznacz `Zostaja Otwarte` albo odznacz `Zuzywa Klucz`. |
