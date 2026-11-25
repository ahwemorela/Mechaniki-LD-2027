# 05 – Zmiana skali

**Scena przykładowa:** `Sceny/05_Zmiana_skali`

## Co to jest

Po zebraniu przedmiotu postać staje się **bardzo mała** albo **bardzo duża**. Razem z rozmiarem zmieniają się jej możliwości – a więc i to, czym jest dla niej poziom:

| | Mała (0,3) | Normalna (1) | Duża (2,5) |
|---|---|---|---|
| Wzrost / szerokość | 0,6 m / 0,3 m | 2 m / 1 m | 5 m / 2,5 m |
| Prędkość chodu | 2,2 m/s | 4 m/s | 6,3 m/s |
| Skok w górę | ok. 0,4 m | ok. 1,4 m | ok. 3 m |
| Udźwig | 4,5 kg (nie podniesie skrzyni) | 50 kg | ponad 300 kg |
| Mieści się w | szybach, pod niskimi przeszkodami | drzwiach 2×3 m | tylko na otwartej przestrzeni |
| Ciężki przycisk | nie | nie | **tak** |

Zmiana skali nie dodaje nowych miejsc – **zmienia znaczenie tych, które już są**. Szczelina pod ścianą staje się drzwiami, a wysoki mur – schodkiem.

## Z czego się składa

| Element | Rola |
|---|---|
| `Prefaby/Skala/Pomniejszacz` | mała niebieska kula – skala 0,3 |
| `Prefaby/Skala/Powiekszacz` | duża różowa kula – skala 2,5 |
| `Prefaby/Skala/PrzywracaczRozmiaru` | biała kula – skala 1 |
| komponent **Gracz Skala** (na postaci) | jak rozmiar wpływa na ruch i siłę |

Wszystkie trzy prefaby to ten sam komponent **Zmiana Skali** z inną wartością pola `Skala`. Zebrana kula znika i wraca po 4 sekundach.

## Jak użyć – krok po kroku

1. Przeciągnij na scenę `Pomniejszacz` (albo inny przedmiot skali).
2. Zbuduj coś, co ma sens tylko w tym rozmiarze: szyb o wysokości 0,9 m dla małej postaci, półkę 2,5 m dla dużej.
3. Postaw w zasięgu `PrzywracaczRozmiaru`, jeśli dalej potrzebny jest normalny rozmiar.

Postać nie urośnie tam, gdzie się nie zmieści (np. w szybie) – zobaczy komunikat i zostanie w starym rozmiarze.

---

## Jak przerobić pod swoją grę

### Zmień znaczenie

| W grze o… | Przedmiot to… |
|---|---|
| krainie czarów | ciastko „zjedz mnie”, butelka „wypij mnie” |
| laboratorium | promień zmniejszający, serum |
| lesie | grzyby dwóch kolorów |
| magii | runy, zaklęte bramy |
| zabawkach | strefa „pokój dziecka”, w której wszystko jest ogromne |

### Przepisy

| Chcę, żeby… | Zrób to |
|---|---|
| …różnica rozmiarów była **subtelniejsza / większa** | Pole `Skala` w przedmiocie: 0.5 i 1.5 albo 0.15 i 4. Pamiętaj, żeby dopasować do tego szyby i półki. |
| …efekt **mijał po czasie** | `Czas Trwania` = np. 8. Na ekranie pojawi się odliczanie, a gracz musi zdążyć. Postać wróci do normy dopiero tam, gdzie się zmieści. |
| …przedmiot był **jednorazowy** | `Czas Odnowienia` = 0. Uwaga: gracz może utknąć w złym rozmiarze – daj mu inną drogę. |
| …przedmiot wracał **szybciej / wolniej** | `Czas Odnowienia`. |
| …zamiast przedmiotu była **bramka**, przez którą się przechodzi | Odznacz `Znika Po Zebraniu`, usuń model kuli i rozciągnij strefę (Sphere Collider → zamień na Box Collider z `Is Trigger`). |
| …napis „Stajesz się mały” był **inny albo go nie było** | `Po Zebraniu` → zmień lub wyczyść `Tekst`. Tam też dodasz dźwięk. |
| …kula miała **inny kolor** | Pole `Kolor`. |
| …przedmiot wyglądał jak **grzyb / eliksir** | Otwórz prefab, podmień dzieci obiektu `Model`. Wyczyść `Kolorowane`, jeśli model ma zachować własne kolory. |
| …mała postać była **równie szybka** jak normalna | Gracz → PlayerCapsule → **Gracz Skala** → `Wplyw Na Predkosc` = 0. (1 = bardzo powolna.) |
| …mała postać skakała **normalnie wysoko** | `Wplyw Na Skok` = 0. |
| …rozmiar **nie zmieniał** zasięgu rąk i udźwigu | Odznacz `Skaluj Zasieg I Udzwig`. |
| …zmiana trwała **dłużej / krócej** (animacja) | `Czas Zmiany`. |
| …komunikat o braku miejsca był inny | **Gracz Skala** → `Gdy Brak Miejsca`. |
| …przycisk wciskała **tylko duża** postać | Przycisk Naciskowy → `Minimalny Rozmiar Gracza` = 2. Powiększ też sam przycisk (skala obiektu), żeby wyglądał na ciężki. |
| …przycisk wciskała każda postać **oprócz małej** | `Minimalny Rozmiar Gracza` = 0.9. |
| …postać **zaczynała** poziom mała | Postaw `Pomniejszacz` dokładnie w miejscu startu (możesz usunąć mu model i wyczyścić komunikat). |
| …rozmiar zmieniało **tylko jedno z ciał** (zamiana ciał) | Wyłącz **Gracz Skala** w drugim ciele. |

---

## Najważniejsze ustawienia

### Zmiana Skali (przedmiot)

| Pole | Znaczenie |
|---|---|
| `Skala` | rozmiar po zebraniu (1 = normalny) |
| `Czas Trwania` | 0 = do zebrania innego przedmiotu |
| `Znika Po Zebraniu` | przedmiot albo stała bramka |
| `Czas Odnowienia` | po ilu sekundach wraca; 0 = nie wraca |
| `Po Zebraniu` | komunikat i dźwięk |
| `Kolor`, `Model`, `Predkosc Obrotu` | wygląd |

### Gracz Skala (postać)

| Pole | Znaczenie |
|---|---|
| `Czas Zmiany` | ile trwa płynne przejście |
| `Wplyw Na Predkosc` | 0 = prędkość stała; 1 = wprost proporcjonalna do rozmiaru |
| `Wplyw Na Skok` | 0 = skok stały; 1 = wprost proporcjonalny do rozmiaru |
| `Skaluj Zasieg I Udzwig` | czy rozmiar zmienia zasięg interakcji i udźwig |
| `Gdy Brak Miejsca` | komunikat, gdy postać nie może urosnąć |

## Liczby do projektowania

- **Szyb dla małej postaci:** 0,9 m wysokości, 1,2 m szerokości. Mała (0,6 m) przechodzi swobodnie, normalna nawet nie próbuje.
- **Półka dla dużej postaci:** 2,6 m. Normalna skacze na ok. 1,4 m, duża na ok. 3 m – różnica ma być oczywista, nie „na styk”.
- **Drzwi 2×3 m** zatrzymują dużą postać (2,5 m szerokości, 5 m wzrostu).
- **Ściany** w poziomie z dużą postacią: minimum 9 m, inaczej gracz je przeskoczy.
- **Miejsce na urośnięcie:** duża postać potrzebuje wolnego koła o średnicy 2,5 m i 5 m wysokości. Nie stawiaj Powiększacza przy ścianie ani pod sufitem.
- **Mała postać jest wolna** (2,2 m/s): 10 m szybu to prawie 5 sekund. Nie rób długich tuneli.

## Wskazówki projektowe

- **Jedna przestrzeń, kilka odczytań.** Najlepsze miejsca to te, które gracz widział już w innym rozmiarze i minął – a teraz nagle są drogą.
- **Konflikt rozmiarów buduje zagadkę.** W scenie duża postać wciska ciężki przycisk, ale nie przejdzie przez drzwi, które otworzyła. Trzeba użyć wielkości i z niej zrezygnować.
- **Bez ślepych zaułków.** Kule się odnawiają, więc błędną decyzję zawsze da się cofnąć. Jeśli robisz przedmiot jednorazowy, sprawdź każdą kolejność.
- **Wielkość kuli podpowiada efekt:** mała kula pomniejsza, duża powiększa. Kształt mówi więcej niż napis.
- **Pokaż cel przed zmianą.** Gracz powinien zobaczyć szyb jako normalny („za mały”), zanim znajdzie Pomniejszacz.
- **Skala to też emocja.** Mała postać w zwykłym pokoju czuje się zagrożona, duża – potężna. Wykorzystaj to.

## Pomysły na połączenie z inną mechaniką

- Klucz we wnęce z małym wejściem (jest w scenie).
- Duża postać podnosi skrzynię 100 kg, której normalna nie ruszy (Rigidbody → `Mass`).
- Mała postać przechodzi pod polem widzenia strażnika za niską osłoną.
- Dwa ciała w różnych rozmiarach: małe wchodzi do szybu i wciska przycisk dla dużego.

## Najczęstsze problemy

| Objaw | Rozwiązanie |
|---|---|
| Postać nie rośnie, widać komunikat o braku miejsca | Za blisko ściany albo za niski sufit. Odsuń przedmiot albo poszerz miejsce. |
| Kula nie reaguje | Postać ma już ten rozmiar albo kula jeszcze się nie odnowiła. |
| Duża postać przeskakuje ściany | Podnieś ściany do min. 9 m. |
| Po zmianie rozmiaru łuki w MetrykachGracza się nie zgadzają | Miarka pokazuje postać normalną. Wpisz w niej wartości dla danego rozmiaru (tabela na górze). |
