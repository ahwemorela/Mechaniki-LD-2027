using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MechanikiLD
{
    /// <summary>
    /// INTERFEJS NA EKRANIE (HUD).
    /// Cały interfejs to zwykły prefab z Canvasem ("Prefaby/UI/HUD"). Każdy element - celownik,
    /// teksty, paski, panele - jest osobnym obiektem, który można w edytorze przesunąć, przeskalować,
    /// przemalować, podmienić mu grafikę lub czcionkę, a także wyłączyć albo usunąć.
    ///
    /// Ten skrypt tylko "ożywia" elementy: wpisuje teksty, pokazuje i chowa panele, przesuwa paski.
    /// O tym, którym elementem steruje, decydują pola poniżej. KAŻDE pole może zostać puste -
    /// wtedy dana część interfejsu po prostu nie istnieje (np. gra bez celownika).
    ///
    /// Jak zrobić własny interfejs:
    ///  1) najprościej: otwórz prefab HUD (dwuklik) i zmieniaj to, co w nim jest,
    ///  2) bezpieczniej: utwórz wariant prefabu (PPM na HUD > Create > Prefab Variant), zmień wariant
    ///     i podmień HUD w prefabie Gracz - oryginał zostaje nietknięty.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz HUD")]
    public class GraczHUD : MonoBehaviour
    {
        [Header("WYGLĄD OGÓLNY")]
        [Tooltip("Czcionka całego interfejsu. Jeśli ją ustawisz, zastąpi czcionkę WSZYSTKICH tekstów w HUD.\nPuste = każdy tekst zachowuje własną czcionkę.\n\nWłasna czcionka: PPM na pliku .ttf/.otf > Create > TextMeshPro > Font Asset > SDF.")]
        public TMP_FontAsset czcionka;

        [Header("CELOWNIK")]
        [Tooltip("Grafika celownika na środku ekranu. Podmień jej Source Image na własną.")]
        public Image celownik;
        public Color celownikZwykly = Color.white;
        [Tooltip("Kolor celownika, gdy gracz patrzy na coś, czego może użyć (E).")]
        public Color celownikInterakcja = Color.white;
        [Tooltip("Kolor celownika, gdy można zaczepić hak.")]
        public Color celownikHak = new Color(1f, 0.85f, 0.1f);
        [Tooltip("Dodatkowa grafika pokazywana, gdy gracz może czegoś użyć (np. ramka wokół celownika).")]
        public GameObject ramkaInterakcji;
        [Tooltip("Dodatkowa grafika pokazywana, gdy można zaczepić hak.")]
        public GameObject ramkaHaka;
        [Tooltip("Tekst pod celownikiem: podpowiedź interakcji albo haka.")]
        public TMP_Text tekstPodCelownikiem;
        [Tooltip("Wzór podpowiedzi interakcji. {0} = opis czynności, np. \"Podnieś skrzynię\".")]
        public string formatInterakcji = "[E]  {0}";
        public string tekstHaka = "[LPM]  Zaczep hak";

        [Header("KLUCZE")]
        [Tooltip("Panel z zebranymi kluczami - widoczny tylko, gdy gracz ma choć jeden klucz.")]
        public GameObject panelKluczy;
        [Tooltip("Wzór ikony klucza. Dla każdego zebranego klucza powstaje jego kopia w kolorze klucza.\nJeśli rodzaj klucza ma własną ikonę, zastąpi ona grafikę wzoru.")]
        public Image wzorIkonyKlucza;

        [Header("NAZWA POSTACI  (zamiana ciał)")]
        [Tooltip("Tekst z nazwą postaci, którą gracz steruje. Widoczny tylko, gdy na scenie jest więcej niż jedna postać.")]
        public TMP_Text tekstPostaci;
        [Tooltip("Wzór tekstu. {0} = nazwa postaci.")]
        public string formatPostaci = "{0}";

        [Header("KOMUNIKAT  (krótkie informacje: \"Zebrano klucz\", \"Punkt kontrolny\")")]
        [Tooltip("Panel komunikatu. Komponent Canvas Group pozwala go płynnie wygaszać.")]
        public CanvasGroup panelKomunikatu;
        public TMP_Text tekstKomunikatu;
        [Tooltip("Ile sekund trwa wygaszanie komunikatu na końcu.")]
        public float czasWygaszania = 0.5f;

        [Header("PODPOWIEDŹ  (panel ze strefy \"Podpowiedź\")")]
        public GameObject panelPodpowiedzi;
        public TMP_Text tytulPodpowiedzi;
        public TMP_Text trescPodpowiedzi;

        [Header("WYKRYWANIE  (skradanie)")]
        [Tooltip("Panel widoczny, gdy strażnik lub kamera widzi gracza.")]
        public GameObject panelWykrywania;
        [Tooltip("Pasek rosnący od 0 do pełna. Jeśli Image Type = Filled, zmieniane jest Fill Amount; inaczej szerokość.")]
        public Image pasekWykrywania;
        public Color wykrywaniePoczatek = new Color(1f, 0.9f, 0.2f);
        public Color wykrywanieKoniec = new Color(1f, 0.1f, 0.1f);

        [Header("ODLICZANIE  (przełącznik czasowy)")]
        public GameObject panelOdliczania;
        public Image pasekOdliczania;
        public Color odliczaniePelne = new Color(0.25f, 0.9f, 0.35f);
        public Color odliczaniePuste = new Color(1f, 0.1f, 0.1f);
        public TMP_Text tekstOdliczania;
        [Tooltip("Wzór tekstu odliczania. {0:0.0} = pozostałe sekundy z jedną cyfrą po przecinku.")]
        public string formatOdliczania = "Czas: {0:0.0} s";

        [Header("ŚCIĄGA ZE STEROWANIEM")]
        public TMP_Text tekstSterowania;
        [Tooltip("Zaznaczone: ściąga układa się sama z linijek poniżej - tylko dla umiejętności, które gracz ma włączone.\nOdznaczone: zostaje dokładnie taki tekst, jaki wpiszesz w obiekcie tekstu.")]
        public bool sterowanieAutomatyczne = true;
        public string sterowanieRuch = "WSAD – ruch    Spacja – skok    Shift – bieg";
        public string sterowanieInterakcja = "E – użyj / podnieś / odłóż";
        public string sterowaniePodnoszenie = "LPM – rzut (gdy coś niesiesz)";
        public string sterowanieHak = "LPM (trzymaj) – hak    PPM – wciąganie    Spacja – puść linę z wybiciem";
        [Tooltip("Linijka pokazywana tylko, gdy na scenie jest więcej niż jedna postać.")]
        public string sterowanieZamiana = "Q – przejdź do drugiego ciała";
        public string sterowanieOdrodzenie = "R – wróć do punktu kontrolnego";

        [Header("ZACIEMNIENIE  (przy odradzaniu)")]
        [Tooltip("Grafika na cały ekran, której przezroczystość rośnie przy odradzaniu. Jej kolor = kolor zaciemnienia.")]
        public Image zaciemnienie;

        [Header("EKRAN KOŃCA POZIOMU")]
        public GameObject ekranKonca;
        public TMP_Text tekstWyniku;
        [Tooltip("Wzór tekstu wyniku. {0} = czas przejścia, {1} = liczba odrodzeń.")]
        public string formatWyniku = "Czas: {0}      Odrodzenia: {1}";
        [Tooltip("Elementy chowane po ukończeniu poziomu (celownik, ściąga...).")]
        public GameObject[] ukryjPoUkonczeniu;

        Gracz _gracz;

        // komunikat
        float _komunikatDo = -1f;

        // podpowiedzi (każda strefa "Podpowiedź", w której stoi gracz)
        readonly List<Podpowiedz> _podpowiedzi = new List<Podpowiedz>();
        Podpowiedz _pokazywana;

        // wartości zgłaszane co klatkę przez inne mechaniki
        float _wykrywanie; int _wykrywanieKlatka = -10;
        float _odliczanie, _odliczanieCalosc; int _odliczanieKlatka = -10;

        // ikony kluczy
        readonly List<Image> _ikonyKluczy = new List<Image>();
        Sprite _domyslnaIkonaKlucza;
        Gracz _postacNaEtykiecie;

        string _ostatnieSterowanie;
        bool _koniec;

        // ---------- start ----------

        void Awake()
        {
            UstawCzcionke();

            // na starcie chowamy wszystko, co pojawia się tylko w określonych sytuacjach
            Pokaz(ramkaInterakcji, false);
            Pokaz(ramkaHaka, false);
            Pokaz(panelKluczy, false);
            Pokaz(panelPodpowiedzi, false);
            Pokaz(panelWykrywania, false);
            Pokaz(panelOdliczania, false);
            Pokaz(ekranKonca, false);
            if (wzorIkonyKlucza != null)
            {
                _domyslnaIkonaKlucza = wzorIkonyKlucza.sprite;
                wzorIkonyKlucza.gameObject.SetActive(false);
            }
            if (tekstPostaci != null) tekstPostaci.gameObject.SetActive(false);
            if (panelKomunikatu != null) panelKomunikatu.alpha = 0f;
            if (tekstPodCelownikiem != null) tekstPodCelownikiem.text = "";
            UstawAlfe(zaciemnienie, 0f);
        }

        void OnValidate() { UstawCzcionke(); }

        void UstawCzcionke()
        {
            if (czcionka == null) return;
            foreach (TMP_Text t in GetComponentsInChildren<TMP_Text>(true))
                if (t.font != czcionka) t.font = czcionka;
        }

        // ---------- funkcje dla innych skryptów ----------

        /// <summary>Pokazuje krótki komunikat.</summary>
        public void Komunikat(string tekst, float czas = 2.5f)
        {
            if (tekstKomunikatu != null) tekstKomunikatu.text = tekst;
            _komunikatDo = Time.time + czas;
        }

        public void DodajPodpowiedz(Podpowiedz p) { if (!_podpowiedzi.Contains(p)) _podpowiedzi.Add(p); }
        public void UsunPodpowiedz(Podpowiedz p) { _podpowiedzi.Remove(p); }

        /// <summary>Strażnik/kamera zgłasza tu co klatkę, jak bardzo "widzi" gracza (0-1).</summary>
        public void ZglosWykrywanie(float poziom)
        {
            if (_wykrywanieKlatka != Time.frameCount) { _wykrywanieKlatka = Time.frameCount; _wykrywanie = 0f; }
            _wykrywanie = Mathf.Max(_wykrywanie, poziom);
        }

        /// <summary>Przełącznik czasowy zgłasza tu co klatkę, ile czasu zostało.</summary>
        public void ZglosOdliczanie(float pozostalo, float calosc)
        {
            if (_odliczanieKlatka != Time.frameCount || pozostalo < _odliczanie)
            {
                _odliczanie = pozostalo;
                _odliczanieCalosc = calosc;
            }
            _odliczanieKlatka = Time.frameCount;
        }

        /// <summary>Pokazuje ekran końca poziomu z czasem przejścia i liczbą odrodzeń.</summary>
        public void PokazKoniecPoziomu(float czasSekundy, int odrodzenia)
        {
            _koniec = true;
            int minuty = Mathf.FloorToInt(czasSekundy / 60f);
            float sekundy = czasSekundy - minuty * 60f;
            if (tekstWyniku != null)
                tekstWyniku.text = string.Format(formatWyniku, minuty + ":" + sekundy.ToString("00.0"), odrodzenia);

            Pokaz(ekranKonca, true);
            if (ukryjPoUkonczeniu != null)
                foreach (GameObject o in ukryjPoUkonczeniu) Pokaz(o, false);
        }

        // ---------- odświeżanie co klatkę ----------

        void LateUpdate()
        {
            // interfejs zawsze pokazuje stan postaci, którą gracz aktualnie steruje
            _gracz = Gracz.Instancja;
            if (_gracz == null) return;

            OdswiezPostac();
            OdswiezCelownik();
            OdswiezKlucze();
            OdswiezKomunikat();
            OdswiezPodpowiedz();
            OdswiezPaski();
            OdswiezSterowanie();

            if (_gracz.Odrodzenie != null) UstawAlfe(zaciemnienie, _gracz.Odrodzenie.Zaciemnienie);
        }

        void OdswiezCelownik()
        {
            if (_koniec) return;
            bool interakcja = _gracz.Interakcja != null && _gracz.Interakcja.enabled && !string.IsNullOrEmpty(_gracz.Interakcja.Podpowiedz);
            bool hak = _gracz.Hak != null && _gracz.Hak.enabled && _gracz.Hak.MaCel;

            if (celownik != null) celownik.color = hak ? celownikHak : interakcja ? celownikInterakcja : celownikZwykly;
            Pokaz(ramkaHaka, hak);
            Pokaz(ramkaInterakcji, interakcja && !hak);

            if (tekstPodCelownikiem != null)
            {
                if (interakcja)
                {
                    tekstPodCelownikiem.text = string.Format(formatInterakcji, _gracz.Interakcja.Podpowiedz);
                    tekstPodCelownikiem.color = celownikInterakcja;
                }
                else if (hak)
                {
                    tekstPodCelownikiem.text = tekstHaka;
                    tekstPodCelownikiem.color = celownikHak;
                }
                else if (tekstPodCelownikiem.text.Length > 0)
                {
                    tekstPodCelownikiem.text = "";
                }
            }
        }

        void OdswiezPostac()
        {
            if (tekstPostaci == null) return;
            bool kilka = Gracz.Wszyscy.Count > 1 && !_koniec;
            if (tekstPostaci.gameObject.activeSelf != kilka) tekstPostaci.gameObject.SetActive(kilka);
            if (!kilka || _postacNaEtykiecie == _gracz) return;

            _postacNaEtykiecie = _gracz;
            tekstPostaci.text = Powiadomienie.Wstaw(formatPostaci, _gracz.nazwaPostaci);
            tekstPostaci.color = _gracz.kolorCiala;
        }

        void OdswiezKlucze()
        {
            if (_gracz.Ekwipunek == null) return;
            List<RodzajKlucza> klucze = _gracz.Ekwipunek.klucze;
            Pokaz(panelKluczy, klucze.Count > 0);
            if (wzorIkonyKlucza == null) return;

            // tyle ikon, ile kluczy: brakujące dorabiamy z wzoru, nadmiarowe chowamy
            while (_ikonyKluczy.Count < klucze.Count)
                _ikonyKluczy.Add(Instantiate(wzorIkonyKlucza, wzorIkonyKlucza.transform.parent));

            for (int i = 0; i < _ikonyKluczy.Count; i++)
            {
                bool widoczna = i < klucze.Count;
                if (_ikonyKluczy[i].gameObject.activeSelf != widoczna) _ikonyKluczy[i].gameObject.SetActive(widoczna);
                if (!widoczna || klucze[i] == null) continue;

                // własna ikona rodzaju klucza albo domyślna z prefabu; kolor z rodzaju klucza
                RodzajKlucza rodzaj = klucze[i];
                _ikonyKluczy[i].sprite = rodzaj.ikona != null ? rodzaj.ikona : _domyslnaIkonaKlucza;
                _ikonyKluczy[i].color = rodzaj.barwIkone ? rodzaj.kolor : Color.white;
            }
        }

        void OdswiezKomunikat()
        {
            if (panelKomunikatu == null) return;
            float zostalo = _komunikatDo - Time.time;
            panelKomunikatu.alpha = zostalo <= 0f ? 0f : Mathf.Clamp01(zostalo / Mathf.Max(0.01f, czasWygaszania));
        }

        void OdswiezPodpowiedz()
        {
            // pokazujemy podpowiedź ze strefy, do której gracz wszedł najpóźniej
            Podpowiedz p = null;
            for (int i = _podpowiedzi.Count - 1; i >= 0; i--)
            {
                if (_podpowiedzi[i] == null) { _podpowiedzi.RemoveAt(i); continue; }
                if (p == null) p = _podpowiedzi[i];
            }
            if (_koniec) p = null;
            if (p == _pokazywana) return;
            _pokazywana = p;

            Pokaz(panelPodpowiedzi, p != null);
            if (p == null) return;

            bool maTytul = !string.IsNullOrEmpty(p.tytul);
            if (tytulPodpowiedzi != null)
            {
                tytulPodpowiedzi.gameObject.SetActive(maTytul);
                tytulPodpowiedzi.text = p.tytul;
            }
            if (trescPodpowiedzi != null) trescPodpowiedzi.text = p.tresc;
        }

        void OdswiezPaski()
        {
            // wykrywanie
            bool widza = !_koniec && _wykrywanieKlatka >= Time.frameCount - 1 && _wykrywanie > 0.01f;
            Pokaz(panelWykrywania, widza);
            if (widza) UstawPasek(pasekWykrywania, _wykrywanie, Color.Lerp(wykrywaniePoczatek, wykrywanieKoniec, _wykrywanie));

            // odliczanie
            bool odlicza = !_koniec && _odliczanieKlatka >= Time.frameCount - 1 && _odliczanieCalosc > 0f;
            Pokaz(panelOdliczania, odlicza);
            if (odlicza)
            {
                float u = Mathf.Clamp01(_odliczanie / _odliczanieCalosc);
                UstawPasek(pasekOdliczania, u, Color.Lerp(odliczaniePuste, odliczaniePelne, u));
                if (tekstOdliczania != null) tekstOdliczania.text = string.Format(formatOdliczania, _odliczanie);
            }
        }

        void OdswiezSterowanie()
        {
            if (tekstSterowania == null || !sterowanieAutomatyczne) return;

            string s = sterowanieRuch;
            if (_gracz.Interakcja != null && _gracz.Interakcja.enabled) s += "\n" + sterowanieInterakcja;
            if (_gracz.Podnoszenie != null && _gracz.Podnoszenie.enabled) s += "    " + sterowaniePodnoszenie;
            if (_gracz.Hak != null && _gracz.Hak.enabled) s += "\n" + sterowanieHak;
            if (Gracz.Wszyscy.Count > 1 && _gracz.ZamianaCial != null && _gracz.ZamianaCial.enabled) s += "\n" + sterowanieZamiana;
            if (_gracz.Odrodzenie != null && _gracz.Odrodzenie.enabled) s += "\n" + sterowanieOdrodzenie;

            if (s == _ostatnieSterowanie) return;
            _ostatnieSterowanie = s;
            tekstSterowania.text = s;
        }

        // ---------- pomocnicze ----------

        static void Pokaz(GameObject obiekt, bool widoczny)
        {
            if (obiekt != null && obiekt.activeSelf != widoczny) obiekt.SetActive(widoczny);
        }

        static void UstawAlfe(Graphic grafika, float alfa)
        {
            if (grafika == null) return;
            Color c = grafika.color;
            if (Mathf.Approximately(c.a, alfa)) return;
            c.a = alfa;
            grafika.color = c;
        }

        /// <summary>Ustawia wypełnienie paska (0-1) i jego kolor.</summary>
        static void UstawPasek(Image pasek, float wartosc, Color kolor)
        {
            if (pasek == null) return;
            pasek.color = kolor;
            if (pasek.type == Image.Type.Filled)
            {
                pasek.fillAmount = wartosc;
            }
            else
            {
                // zwykła grafika: skracamy ją, przesuwając prawą kotwicę
                RectTransform rt = pasek.rectTransform;
                rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
                rt.anchorMax = new Vector2(Mathf.Clamp01(wartosc), rt.anchorMax.y);
            }
        }
    }
}
