# Mechaniki LD – zestaw mechanik do zajęć z level designu

Projekt Unity z gotowymi, prostymi mechanikami FPP, wokół których można zbudować własny poziom w estetyce blockoutu. Nie trzeba programować: mechaniki to prefaby, które przeciąga się na scenę, łączy ze sobą i **przerabia pod własną grę** w Inspektorze.

- **Unity:** 6000.3 (Unity 6.3 LTS), URP, nowy Input System
- **Postać:** First Person Controller ze Starter Assets (rozszerzony o hak, platformy, punkty kontrolne, zamianę ciał i zmianę skali)
- **Wszystko, co dotyczy zajęć, leży w folderze** `Assets/MechanikiLD`

## Szybki start

1. Otwórz projekt w Unity 6000.3.
2. Otwórz jedną ze scen z `Assets/MechanikiLD/Sceny` i wciśnij **Play**.
3. Przejdź scenę jako gracz, a potem wyjdź z trybu Play i obejrzyj ją w oknie **Scene**. Przy ważnych miejscach są notatki:
   - **żółte** – dlaczego coś stoi tam, gdzie stoi (decyzje projektowe),
   - **niebieskie** – jak daną rzecz przerobić pod własną grę.
4. Własny poziom zacznij od menu **Mechaniki LD → Nowy poziom z szablonu…**

Pełna instrukcja: [`00_Start.md`](Assets/MechanikiLD/Dokumentacja/00_Start.md)

## Sterowanie

| Klawisz | Działanie |
|---|---|
| W S A D | ruch |
| Spacja | skok (na linie: puszczenie liny z wybiciem) |
| Shift | bieg |
| E | użyj / podnieś / odłóż |
| LPM | hak (przytrzymaj) albo rzut niesionym przedmiotem |
| PPM | wciąganie się na linie |
| Q | przejście do drugiego ciała |
| R | powrót do ostatniego punktu kontrolnego |

## Sceny i mechaniki

| Scena | Mechanika | Dokumentacja |
|---|---|---|
| `01_Klucze_i_drzwi` | Zbieranie kluczy otwierających drzwi tego samego rodzaju. Rodzaje kluczy można dodawać samemu. | [01](Assets/MechanikiLD/Dokumentacja/01_Klucze_i_drzwi.md) |
| `02_Przyciski_i_skrzynie` | Przycisk wciskany ciałem lub przeniesioną skrzynią; otwiera drzwi albo uruchamia platformę. | [02](Assets/MechanikiLD/Dokumentacja/02_Przyciski_i_skrzynie.md) |
| `03_Hak` | Hak z liną: bujanie się i wciąganie do punktów zaczepienia. | [03](Assets/MechanikiLD/Dokumentacja/03_Hak.md) |
| `04_Zamiana_cial` | Dwa ciała, jeden gracz: przechodzenie z ciała do ciała i współpraca z samym sobą. | [04](Assets/MechanikiLD/Dokumentacja/04_Zamiana_cial.md) |
| `05_Zmiana_skali` | Przedmioty, po których zebraniu postać staje się bardzo mała albo bardzo duża. | [05](Assets/MechanikiLD/Dokumentacja/05_Zmiana_skali.md) |
| `06_Skradanie` | Kamery i strażnicy z polem widzenia, osłony, alarm. | [06](Assets/MechanikiLD/Dokumentacja/06_Skradanie.md) |
| `00_Szablon` | Pusta scena startowa do budowy własnego poziomu. | – |

Elementy wspólne (punkt kontrolny, strefa śmierci, meta, podpowiedzi, przełącznik, ruchoma platforma, miarka gracza): [`07_Elementy_wspolne.md`](Assets/MechanikiLD/Dokumentacja/07_Elementy_wspolne.md)

## Przerabianie pod własną grę

Zestaw jest punktem wyjścia, nie gotową grą. Każda mechanika ma w dokumentacji rozdział **„Jak przerobić pod swoją grę”**, a ogólne zasady opisuje [`08_Dostosowywanie.md`](Assets/MechanikiLD/Dokumentacja/08_Dostosowywanie.md). W skrócie:

- **Wszystko, co robi obiekt, jest w jego Inspektorze** – zachowanie, teksty komunikatów i dźwięki. Puste pole tekstu = brak komunikatu.
- **Rodzaje kluczy dodaje się samemu** (PPM → Create → Mechaniki LD → Rodzaj klucza): własna nazwa, kolor i ikona.
- **Interfejs (HUD)** to zwykły prefab z Canvasem – każdy element można przesunąć, przemalować, podmienić albo usunąć.
- **Kolory mechanik** zmienia się w jednym pliku ustawień.
- **Modele** mechanik są oddzielone od ich działania – można je podmienić na własne.

Skróty do tych miejsc są w menu **Mechaniki LD** na górnym pasku Unity.

## Struktura folderu `Assets/MechanikiLD`

```
MechanikiLD/
├─ Dokumentacja/   opisy mechanik i ich przerabiania (po polsku)
├─ Editor/         menu "Mechaniki LD" na górnym pasku Unity
├─ Materialy/      Blok_* – materiały blockoutu z siatką 1 m;  Mech_* – materiały mechanik
├─ Prefaby/        gotowe elementy do przeciągania na scenę
│   ├─ Blockout/   bloki, rampa, schody
│   ├─ Gracz/      Gracz (postać + kamera + interfejs), DrugieCialo (druga postać)
│   ├─ UI/         HUD – interfejs gracza do własnej przeróbki
│   ├─ Klucze/ Drzwi/ Przyciski/ Przedmioty/ Platformy/ Hak/ Skala/ Skradanie/
│   └─ Poziom/     PunktKontrolny, StrefaSmierci, MetaPoziomu, Podpowiedz, NotatkaProjektanta, MetrykiGracza
├─ Resources/      UstawieniaLD – kolory mechanik i głośność dźwięków
├─ Sceny/          sceny przykładowe i szablon
├─ Skrypty/        kod mechanik (komentarze po polsku)
├─ UI/             grafiki i czcionka interfejsu
└─ Ustawienia/     Klucze – rodzaje kluczy (tu dodajesz własne)
```

## Język kolorów

W blockoucie kolor jest informacją. W scenach przykładowych ten sam kolor zawsze znaczy to samo:

| Kolor | Znaczenie |
|---|---|
| szary z siatką | zwykła geometria poziomu (siatka ma oczka co 1 m) |
| pomarańczowy | element czeka na akcję gracza (przycisk, przełącznik, punkt kontrolny, zamknięte drzwi) |
| zielony | włączone / otwarte / zaliczone / meta |
| czerwony | niebezpieczeństwo – zabija |
| żółty | punkt, o który można zaczepić hak |
| fioletowy | strażnicy i kamery |
| biały | przedmiot, który można podnieść |
| kolor klucza | pas na drzwiach = potrzebny klucz tego rodzaju |
| niebieska / różowa / biała kula | zmiana skali: pomniejszenie / powiększenie / powrót do normy |

We własnym projekcie możesz ustalić inny język kolorów – ważne, żeby był jeden i konsekwentny.

## Uwagi techniczne

- Projekt powstał z szablonu 2D, dlatego dodano ustawienia renderowania 3D (`Assets/Settings/URP_3D.asset`) i przypisano je do wszystkich poziomów jakości. Stare pliki 2D w `Assets/Settings` nie są używane.
- Skrypt `FirstPersonController` ze Starter Assets został rozszerzony – wszystkie dopisane fragmenty są oznaczone komentarzem `[LD]`.
- Warstwa nr 8 nazywa się `Gracz` i jest zarezerwowana dla postaci gracza.
- Interfejs używa TextMeshPro; jego podstawowe zasoby leżą w `Assets/TextMesh Pro` (nie usuwaj tego folderu).
- Do szybkiego budowania brył warto doinstalować pakiet **ProBuilder** (Window → Package Manager); projekt go nie wymaga.
