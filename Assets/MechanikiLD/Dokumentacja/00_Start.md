# Start – jak korzystać z projektu

## 1. Obejrzyj sceny przykładowe

Sceny leżą w `Assets/MechanikiLD/Sceny`. Każdą warto obejrzeć dwa razy:

1. **Jako gracz** – wciśnij Play i przejdź ją do zielonej mety.
2. **Jako projektant** – wyjdź z trybu Play i przeleć po scenie w oknie Scene. Ramki z tekstem to **notatki**:
   - **żółte** wyjaśniają decyzje projektowe: co gracz ma w danym miejscu zrozumieć i jak to osiągnięto,
   - **niebieskie** („JAK TO PRZEROBIĆ”) podpowiadają, co w danym obiekcie można zmienić pod własną grę.

   Kolorowe linie pokazują połączenia między obiektami i trasy platform.

> Jeśli nie widzisz notatek ani linii, włącz **Gizmos** (przycisk w prawym górnym rogu okna Scene).

## 2. Zacznij własny poziom

1. Menu **Mechaniki LD → Nowy poziom z szablonu…** – wybierz nazwę, a dostaniesz kopię sceny `00_Szablon`.
2. W scenie jest już wszystko, co niezbędne: podłoga, światło, **Gracz**, **Meta Poziomu** i **Metryki Gracza** (miarka).
3. Buduj z bloków:
   - przeciągaj prefaby z `Prefaby/Blockout` i zmieniaj im skalę,
   - albo twórz zwykłe kostki (**GameObject → 3D Object → Cube**) i nadawaj im materiały `Blok_*`.
   Siatka na materiałach `Blok_*` ma zawsze oczka co 1 m – niezależnie od skali bloku.
4. Dodawaj mechaniki, przeciągając prefaby z `Prefaby/...` na scenę.
5. **Przerabiaj je pod swoją grę** – patrz punkt 6.

Trzymaj porządek w hierarchii: sceny przykładowe dzielą obiekty na `--- OTOCZENIE ---`, `--- MECHANIKI ---`, `--- GRACZ ---` i `--- PODPOWIEDZI I NOTATKI ---`.

## 3. Wymiary, które trzeba znać (metryki)

Poziom projektuje się pod możliwości postaci:

| Co | Wartość |
|---|---|
| Wysokość / szerokość postaci | 2 m / 1 m |
| Wysokość oczu | ok. 1,4 m |
| Prędkość chodu / biegu | 4 m/s / 6 m/s |
| Stopień pokonywany bez skoku | do 0,25 m |
| Skok w górę | ok. 1,4 m (bezpiecznie projektuj na **1,2 m**) |
| Skok w dal z chodu | ok. 3,5 m (bezpiecznie: **3 m**) |
| Skok w dal z biegu | ok. 5 m (bezpiecznie: **4,5 m**) |
| Zasięg interakcji (E) | 3 m |
| Zasięg haka | 22 m |
| Otwór drzwi z prefabu | 2 m szerokości, ok. 3 m wysokości |
| Skrzynia | sześcian 0,9 m |
| Postać pomniejszona / powiększona | 0,6 m / 5 m wzrostu (tabela w [05_Zmiana_skali.md](05_Zmiana_skali.md)) |

Prefab **MetrykiGracza** (`Prefaby/Poziom`) rysuje w oknie Scene sylwetkę postaci i łuki skoków. Przesuwaj go po poziomie jak miarkę.

Wartości „bezpieczne” zostawiają graczowi margines. Skok wymagający idealnego wybicia z ostatniego centymetra krawędzi nie jest wyzwaniem – jest frustracją.

## 4. Jak łączyć mechaniki: sygnały

Większość mechanik rozmawia ze sobą w prosty sposób: **aktywator wysyła sygnał do celu**.

- **Aktywatory** (coś uruchamiają): Przycisk Naciskowy, Przełącznik, Klucz (w chwili zebrania), Pole Widzenia (w chwili wykrycia).
- **Cele** (reagują): Drzwi, Ruch Po Trasie (platforma, winda, most).

**Jak połączyć:** zaznacz aktywator, w Inspektorze znajdź listę `Cele`, kliknij `+` i przeciągnij tam obiekt-cel. W oknie Scene połączy je linia.

Każdy cel ma dwa wspólne ustawienia (sekcja „Aktywacja”):

| Pole | Co robi |
|---|---|
| `Wymagane Sygnaly` | Ile aktywatorów musi być włączonych **jednocześnie**. Ustaw 2, a drzwi otworzą się dopiero po wciśnięciu dwóch przycisków naraz. |
| `Aktywny Bez Sygnalu` | Odwraca logikę: obiekt działa od początku, a sygnał go **wyłącza** (drzwi otwarte, dopóki nie włączy się alarm; platforma, którą przycisk zatrzymuje). |

Jeden aktywator może mieć wiele celów, a jeden cel – wiele aktywatorów.

Każdy aktywator i cel ma też **zdarzenia** (`Gdy Wlaczony`, `Gdy Aktywowany`…), pod które można podpiąć dowolną funkcję dowolnego obiektu: włączyć światło, uruchomić animację lub Timeline, wyłączyć kamerę (przykład w scenie `06_Skradanie`).

## 5. Gracz i jego umiejętności

Prefab `Prefaby/Gracz/Gracz` zawiera postać, kamerę i interfejs (dziecko `HUD`). Umiejętności to osobne komponenty na obiekcie **PlayerCapsule**:

| Komponent | Umiejętność |
|---|---|
| `First Person Controller` | chodzenie, bieg, skok |
| `Gracz Interakcja` | używanie obiektów klawiszem E |
| `Gracz Podnoszenie` | noszenie i rzucanie przedmiotów |
| `Gracz Hak` | hak z liną |
| `Gracz Zamiana Cial` | przechodzenie do drugiego ciała (Q) – działa, gdy na scenie jest prefab `DrugieCialo` |
| `Gracz Skala` | reakcja na przedmioty zmiany skali |
| `Gracz Odrodzenie` | punkty kontrolne i klawisz R |

**Wyłączenie umiejętności:** odznacz pole wyboru przy nazwie komponentu. Jeśli Twój poziom nie korzysta z haka, wyłącz `Gracz Hak` – zniknie też ze ściągi sterowania na ekranie. Gracz powinien mieć tylko te narzędzia, wokół których zbudowany jest poziom.

## 6. Przerabianie pod własną grę

To nie jest gotowa gra, tylko zestaw klocków. Trzy zasady:

1. **Wszystko, co robi obiekt, jest w jego Inspektorze.** Zaznacz drzwi, klucz albo przycisk: znajdziesz tam ustawienia zachowania oraz sekcję **„Komunikaty i dźwięki”**. Każdy tekst można zmienić; **puste pole = brak komunikatu**.
2. **Zmiana na scenie dotyczy jednego obiektu, zmiana w prefabie – wszystkich.** Chcesz innego napisu w każdych drzwiach? Otwórz prefab drzwi.
3. **Wygląd jest oddzielony od działania.** Klucz może wyglądać jak karta dostępu, drzwi jak krata, a skrzynia jak posąg – skrypt zostaje ten sam.

Każdy dokument mechaniki ma rozdział **„Jak przerobić pod swoją grę”** z tabelą gotowych przepisów („Chcę, żeby… → Zrób to”). Zasady ogólne, interfejs, kolory i własne rodzaje kluczy opisuje [08_Dostosowywanie.md](08_Dostosowywanie.md). Skróty są w menu **Mechaniki LD**.

## 7. Dobre praktyki, które widać w scenach przykładowych

- **Naucz → sprawdź → zaskocz.** Każda scena najpierw pokazuje mechanikę w bezpiecznej sytuacji, potem wymaga jej samodzielnego użycia, a na końcu łączy ją z czymś nowym.
- **Najpierw cel, potem środek.** Gracz powinien zobaczyć zamknięte drzwi, zanim znajdzie klucz; za mały szyb, zanim znajdzie kulę pomniejszającą.
- **Kolor to informacja.** Ustal własny język kolorów i trzymaj się go – gracz uczy się go w pierwszej minucie i potem na nim polega.
- **Punkt kontrolny po każdym wyzwaniu.** Powtarzanie tego, co już się umie, to kara, nie trudność.
- **Zagadki nie da się zepsuć.** Skrzynie wracają na miejsce, kule zmiany skali się odnawiają. Zawsze zapytaj: „co, jeśli gracz zrobi to w złej kolejności?”.
- **Testuj na ludziach.** Ekran końcowy pokazuje czas przejścia i liczbę odrodzeń. Daj poziom komuś, kto go nie zna, i patrz w milczeniu.

## 8. Gdy coś nie działa

| Objaw | Przyczyna i rozwiązanie |
|---|---|
| Przycisk nic nie otwiera | Lista `Cele` jest pusta. |
| Drzwi otwierają się „na odwrót” | Zaznaczone `Aktywny Bez Sygnalu`. |
| Klucz nie pasuje do drzwi | Klucz (`Rodzaj`) i drzwi (`Wymagany Klucz`) muszą wskazywać ten sam plik rodzaju klucza. |
| Nie pojawia się komunikat | Pole `Tekst` w sekcji „Komunikaty i dźwięki” jest puste. |
| Postać przelatuje przez podłogę | Blok nie ma collidera. |
| Postać nie reaguje na strefę (śmierci, metę, podpowiedź) | Collider strefy nie ma zaznaczonego `Is Trigger`. |
| Hak nie chce się zaczepić | Obiekt nie ma komponentu `Punkt Haka`, jest poza zasięgiem albo coś go zasłania. |
| Q nie zmienia ciała | Na scenie nie ma prefabu `DrugieCialo`. |
| Nie widać notatek i linii w oknie Scene | Wyłączone Gizmos. |
| Obiekty są różowe | Materiał z innego potoku renderowania. Użyj materiałów z `MechanikiLD/Materialy`. |
| W grze nie da się sterować | Kliknij w okno Game, żeby przejęło mysz i klawiaturę. |
| Nie ma interfejsu na ekranie | W prefabie Gracz brakuje dziecka `HUD` albo jest wyłączone. |
| Zamiast liter widać kwadraty | Własna czcionka nie ma polskich znaków. |
