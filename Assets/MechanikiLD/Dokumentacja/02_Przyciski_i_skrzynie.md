# 02 – Przyciski naciskowe i skrzynie

**Scena przykładowa:** `Sceny/02_Przyciski_i_skrzynie`

## Co to jest

**Przycisk naciskowy** działa tak długo, jak coś na nim stoi – postać albo przedmiot fizyczny. **Skrzynię** można podnieść, przenieść, odłożyć i rzucić. Razem dają klasyczną zagadkę: „żeby przejść, coś musi zostać na przycisku”.

Przycisk może uruchomić dowolny cel: otworzyć drzwi, puścić w ruch platformę, wysunąć most.

## Z czego się składa

| Prefab | Rola |
|---|---|
| `Prefaby/Przyciski/PrzyciskNaciskowy` | płyta w podłodze wysyłająca sygnał |
| `Prefaby/Przedmioty/Skrzynia` | przedmiot do noszenia (biały sześcian 0,9 m, 8 kg) |
| `Prefaby/Drzwi/Drzwi` | cel: drzwi otwierane sygnałem |
| `Prefaby/Platformy/RuchomaPlatforma` | cel: platforma uruchamiana sygnałem |

Noszenie obsługuje komponent **Gracz Podnoszenie** na postaci: E – podnieś / odłóż, LPM – rzuć.

## Jak użyć – krok po kroku

**Przycisk otwierający drzwi**
1. Postaw na scenie `PrzyciskNaciskowy` i `Drzwi`.
2. Zaznacz przycisk, w liście `Cele` kliknij `+` i przeciągnij tam drzwi. W oknie Scene połączy je linia.
3. Postaw w pobliżu `Skrzynia`.

**Drzwi wymagające dwóch przycisków naraz** – dodaj drzwi do `Cele` obu przycisków i ustaw w drzwiach `Wymagane Sygnaly` = 2.

**Przycisk uruchamiający platformę** – w platformie (komponent **Ruch Po Trasie**) odznacz `Aktywny Bez Sygnalu` i dodaj ją do `Cele` przycisku.

---

## Jak przerobić pod swoją grę

### Zmień znaczenie

| W grze o… | Przycisk to… | Skrzynia to… |
|---|---|---|
| świątyni | kamienna płyta | posąg, waza |
| fabryce | waga, czujnik nacisku | beczka, paleta |
| statku kosmicznym | panel podłogowy | zasobnik, bateria |
| ogrodzie | pień, kamień w strumieniu | dynia, donica |

### Przepisy

| Chcę, żeby… | Zrób to |
|---|---|
| …przycisk wciskała **tylko skrzynia**, nie gracz | Odznacz `Reaguje Na Gracza`. |
| …przycisk wciskał **tylko gracz** | Odznacz `Reaguje Na Przedmioty`. |
| …potrzebny był **ciężki** przedmiot | Ustaw `Minimalna Masa` (skrzynia waży 8 kg) i zrób cięższą skrzynię: Rigidbody → `Mass`. |
| …przycisk działał **tylko raz** i zostawał wciśnięty | Zaznacz `Zostaje Wcisniety`. |
| …gracz miał **kilka sekund** po zejściu z przycisku | Zwiększ `Opoznienie Zwolnienia` – to prosta zagadka „na czas” bez skrzyni. |
| …przycisk wciskała **tylko powiększona postać** | `Minimalny Rozmiar Gracza` = 2 (mechanika zmiany skali). |
| …przycisk **klikał** | „Komunikaty i dźwięki” → `Gdy Wcisniety` / `Gdy Zwolniony` → pole `Dzwiek`. Możesz też wpisać tekst. |
| …przycisk był **większy / mniejszy** | Zmień skalę całego obiektu – strefa wykrywania skaluje się razem z nim. |
| …przycisk **nie zmieniał koloru** | Wyczyść listę `Kolorowane`. |
| …zamiast skrzyni była **kula / bateria / posąg** | Dowolny obiekt z **Rigidbody** i colliderem + komponent **Przedmiot Do Podnoszenia**. Albo otwórz prefab `Skrzynia` i podmień model. |
| …podpowiedź brzmiała „Weź baterię” | W przedmiocie zmień pole `Podpowiedz`. |
| …przedmiot **nie wracał** po upadku w przepaść | Odznacz `Wraca Na Start` (uwaga: zagadkę da się wtedy zepsuć). |
| …gracz niósł przedmiot **bliżej / dalej**, rzucał **mocniej** | Gracz → PlayerCapsule → **Gracz Podnoszenie**: `Odleglosc Trzymania`, `Sila Rzutu`, `Maksymalna Masa`. |
| …gracz mógł **przepychać** skrzynie ciałem | Gracz → PlayerCapsule → **Basic Rigid Body Push** → zaznacz `Can Push`. |
| …przycisk **wysuwał most** zamiast otwierać drzwi | Platforma w trybie `Na Sygnał Do Końca`: z sygnałem jedzie do ostatniego punktu, bez – wraca. |
| …przycisk uruchamiał **cokolwiek** (światło, animację, Timeline) | Zdarzenia `Gdy Wlaczony` / `Gdy Wylaczony` na dole komponentu. |

---

## Najważniejsze ustawienia

### Przycisk Naciskowy

| Pole | Znaczenie |
|---|---|
| `Cele` | co przycisk uruchamia |
| `Reaguje Na Gracza` / `Reaguje Na Przedmioty` | kto może go wcisnąć |
| `Minimalna Masa` | najlżejszy przedmiot, który go wciśnie |
| `Minimalny Rozmiar Gracza` | 0 = każda postać, 2 = tylko powiększona |
| `Zostaje Wcisniety` | przycisk jednorazowy |
| `Opoznienie Zwolnienia` | ile sekund po zejściu przycisk jeszcze działa |
| `Srodek Strefy` / `Rozmiar Strefy` | pudełko nad płytą, w którym wykrywany jest ciężar (zielona ramka po zaznaczeniu) |

### Przedmiot Do Podnoszenia

| Pole | Znaczenie |
|---|---|
| `Podpowiedz` | tekst pod celownikiem |
| `Po Podniesieniu` / `Po Odlozeniu` / `Po Rzucie` | komunikaty i dźwięki |
| `Wraca Na Start` | powrót na miejsce po wypadnięciu z poziomu |

## Liczby do projektowania

- Postać podnosi przedmiot z odległości do **3 m** i niesie go **ok. 1,9 m** przed sobą.
- Rzut: **9 m/s** – przedmiot doleci na kilka metrów.
- Drzwi zamykają się w **0,8 s**. Jeśli przycisk stoi bliżej niż ok. 4 m od drzwi, gracz może zdążyć przebiec bez skrzyni. W scenie przykładowej jest to 6 m – i nie da się.
- Skrzynia ma **0,9 m** – można na nią wskoczyć i użyć jej jako stopnia.
- Udźwig: do **50 kg**. Pomniejszona postać nie podniesie zwykłej skrzyni.

## Wskazówki projektowe

- **Daj graczowi samemu odkryć zasadę.** Staje na przycisku – drzwi się otwierają; schodzi – zamykają się, zanim dobiegnie. Nikt nie musi mu mówić, że potrzebuje skrzyni.
- **Skrzynia ma być widoczna z miejsca, w którym powstaje problem.**
- **Stopniuj:** jeden przycisk → dwa przyciski → skrzynia w trudniej dostępnym miejscu → przycisk steruje czymś innym niż drzwi.
- **Zabezpiecz się przed zepsuciem zagadki.** Pod przepaścią musi być Strefa Śmierci (skrzynia wróci na start). Sprawdź, czy gracz nie może zamknąć się po złej stronie drzwi.
- **Linie w oknie Scene to schemat zagadki.** Jeśli sam się w nich gubisz, gracz zgubi się na pewno.

## Pomysły na połączenie z inną mechaniką

- Przycisk trzyma drugie ciało zamiast skrzyni (scena `04_Zamiana_cial`).
- Ciężki przycisk tylko dla powiększonej postaci (scena `05_Zmiana_skali`).
- Skrzynia jako stopień na półkę z kluczem; przycisk na trasie patrolu strażnika.

## Najczęstsze problemy

| Objaw | Rozwiązanie |
|---|---|
| Przycisk nic nie uruchamia | Pusta lista `Cele`. |
| Przycisk nie reaguje na skrzynię | Skrzynia leży tylko rogiem albo `Minimalna Masa` jest większa niż jej masa. |
| Przycisk jest stale wciśnięty | W strefie wykrywania leży inny obiekt z Rigidbody. |
| Nie da się podnieść przedmiotu | Brak komponentu **Przedmiot Do Podnoszenia**, masa ponad udźwig albo wyłączony **Gracz Podnoszenie**. |
| Niesiona skrzynia wypada z rąk | Zaklinowała się o ścianę i została daleko od punktu trzymania dłużej niż pół sekundy. To celowe. |
