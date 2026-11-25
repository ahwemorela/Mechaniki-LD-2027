using UnityEngine;

namespace MechanikiLD
{
    public enum SposobOtwierania
    {
        [InspectorName("Tylko Sygnał")]
        [Tooltip("Drzwi otwiera wyłącznie sygnał z aktywatora (przycisk, przełącznik...).")]
        TylkoSygnal,
        [Tooltip("Gracz otwiera i zamyka drzwi klawiszem E.")]
        Interakcja,
        [Tooltip("Drzwi otwierają się same, gdy gracz podejdzie.")]
        Automatycznie
    }

    public enum RuchDrzwi
    {
        Przesuwanie,
        [InspectorName("Obrót")] Obrot
    }

    /// <summary>
    /// DRZWI / BRAMA.
    /// Skrzydło drzwi przesuwa się (albo obraca) między pozycją zamkniętą a otwartą.
    ///
    /// Drzwi można otworzyć na trzy sposoby (pole "Sposob Otwierania"):
    ///  - Tylko Sygnał   - otwiera je przycisk, przełącznik itp. (drzwi dodaje się do listy "Cele" aktywatora),
    ///  - Interakcja     - gracz otwiera je klawiszem E,
    ///  - Automatycznie  - otwierają się, gdy gracz podejdzie.
    /// Do dwóch ostatnich można dodać ZAMEK NA KLUCZ (pole "Wymagany Klucz"): drzwi zadziałają dopiero,
    /// gdy postać ma klucz tego rodzaju. Sygnał z aktywatora otwiera drzwi zawsze, niezależnie od sposobu.
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ: "drzwi" to każda przeszkoda, która znika lub się odsuwa - brama, krata,
    /// most zwodzony (Przesuniecie w bok), klapa w podłodze, pole siłowe. Zmień model skrzydła, kierunek
    /// i czas ruchu oraz teksty w sekcji "Komunikaty i dźwięki".
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Drzwi")]
    public class Drzwi : ObiektAktywowalny, IInteraktywny
    {
        [Header("Ruch skrzydła")]
        [Tooltip("Ruchoma część drzwi (obiekt-dziecko z modelem i colliderem).")]
        public Transform skrzydlo;
        public RuchDrzwi rodzajRuchu = RuchDrzwi.Przesuwanie;
        [Tooltip("O ile przesuwa się skrzydło po otwarciu (w metrach, w osiach obiektu Drzwi). Np. (0, 3, 0) = wjeżdża 3 m w górę.")]
        public Vector3 przesuniecie = new Vector3(0f, 3f, 0f);
        [Tooltip("O ile obraca się skrzydło po otwarciu (w stopniach). Używane, gdy Rodzaj Ruchu = Obrót.")]
        public Vector3 obrot = new Vector3(0f, 100f, 0f);
        [Tooltip("Ile sekund trwa otwarcie / zamknięcie.")]
        [Min(0.05f)] public float czasRuchu = 0.8f;

        [Header("Sposób otwierania")]
        public SposobOtwierania sposobOtwierania = SposobOtwierania.TylkoSygnal;
        [Tooltip("Z jakiej odległości drzwi automatyczne wykrywają gracza (metry).")]
        public float zasiegAutomatu = 3.5f;
        [Tooltip("Zaznaczone: raz otwarte drzwi już się nie zamkną.\nPrzydatne, gdy nie chcesz, żeby gracz mógł się zatrzasnąć albo musiał wracać do przycisku.")]
        public bool zostajaOtwarte = false;

        [Header("Zamek na klucz (dla sposobów Interakcja i Automatycznie)")]
        [Tooltip("Rodzaj klucza potrzebny do otwarcia. PUSTE = drzwi nie mają zamka.\nWłasny rodzaj: PPM w oknie Project > Create > Mechaniki LD > Rodzaj klucza.")]
        public RodzajKlucza wymaganyKlucz;
        [Tooltip("Czy otwarcie zabiera klucz z ekwipunku (jeden klucz = jedne drzwi).")]
        public bool zuzywaKlucz = true;

        [Header("Komunikaty i dźwięki  ({0} = nazwa klucza; puste = wyłączone)")]
        [Tooltip("Gdy postać podejdzie bez właściwego klucza.")]
        public Powiadomienie gdyBrakKlucza = new Powiadomienie("Zamknięte. Potrzebny klucz: {0}");
        [Tooltip("Gdy zamek zostaje otwarty kluczem.")]
        public Powiadomienie gdyOtwartoKluczem = new Powiadomienie("Otwarto kluczem: {0}");
        [Tooltip("Gdy drzwi zaczynają się otwierać (zwykle sam dźwięk).")]
        public Powiadomienie gdyOtwieranie = new Powiadomienie();
        [Tooltip("Gdy drzwi zaczynają się zamykać (zwykle sam dźwięk).")]
        public Powiadomienie gdyZamykanie = new Powiadomienie();

        [Header("Podpowiedzi pod celownikiem (sposób Interakcja)")]
        public string podpowiedzOtworz = "Otwórz";
        public string podpowiedzZamknij = "Zamknij";
        [Tooltip("Gdy postać ma klucz. {0} = nazwa klucza.")]
        public string podpowiedzOtworzKluczem = "Otwórz kluczem ({0})";
        [Tooltip("Gdy postać nie ma klucza. {0} = nazwa klucza.")]
        public string podpowiedzBrakKlucza = "Zamknięte – potrzebny klucz: {0}";

        [Header("Wygląd")]
        [Tooltip("Lampki / oznaczenia pokazujące stan drzwi: kolor klucza = zamek na klucz, pomarańczowy = zamknięte (czekają na sygnał), zielony = otwarte.\nPuste = drzwi nie zmieniają koloru.")]
        public Renderer[] znacznikiStanu;

        /// <summary>Czy drzwi są (lub właśnie stają się) otwarte.</summary>
        public bool Otwarte { get; private set; }

        Rigidbody _cialo;
        Vector3 _zamknietaPozycja;
        Quaternion _zamknietyObrot;
        float _postep;            // 0 = zamknięte, 1 = otwarte
        bool _odblokowane;        // zamek otwarty kluczem
        bool _otwarteRecznie;     // gracz otworzył klawiszem E
        bool _zatrzasnieteOtwarte;
        float _czasKomunikatu = -10f;

        bool WymagaKlucza => wymaganyKlucz != null;
        string NazwaKlucza => wymaganyKlucz != null ? wymaganyKlucz.nazwa : "";

        void Awake()
        {
            if (skrzydlo != null)
            {
                _zamknietaPozycja = skrzydlo.localPosition;
                _zamknietyObrot = skrzydlo.localRotation;
                _cialo = skrzydlo.GetComponent<Rigidbody>();
            }
        }

        void OnValidate() { OdswiezWyglad(); }

        protected override void GdyZmianaStanu(bool aktywny, bool natychmiast)
        {
            // sam ruch liczymy w FixedUpdate; tu tylko ustawienie stanu na starcie sceny
            if (natychmiast && aktywny)
            {
                _postep = 1f;
                UstawSkrzydlo(1f, true);
            }
        }

        void FixedUpdate()
        {
            bool bylyOtwarte = Otwarte;
            Otwarte = CzyPowinnyBycOtwarte();
            if (Otwarte && zostajaOtwarte) _zatrzasnieteOtwarte = true;
            if (Otwarte != bylyOtwarte && Time.timeSinceLevelLoad > 0.2f)
                (Otwarte ? gdyOtwieranie : gdyZamykanie).Wyslij(transform.position, NazwaKlucza);

            float cel = Otwarte ? 1f : 0f;
            if (!Mathf.Approximately(_postep, cel))
            {
                _postep = Mathf.MoveTowards(_postep, cel, Time.fixedDeltaTime / czasRuchu);
                UstawSkrzydlo(_postep, false);
            }
            OdswiezWyglad();
        }

        bool CzyPowinnyBycOtwarte()
        {
            if (_zatrzasnieteOtwarte) return true;
            if (CzyAktywny) return true;        // sygnał z aktywatora
            if (_otwarteRecznie) return true;   // gracz otworzył klawiszem E

            if (sposobOtwierania == SposobOtwierania.Automatycznie)
            {
                // drzwi reagują na każdą postać (także tę, którą gracz akurat nie steruje)
                foreach (Gracz gracz in Gracz.Wszyscy)
                {
                    bool blisko = Vector3.Distance(gracz.transform.position, transform.position) <= zasiegAutomatu;
                    if (!blisko) continue;
                    if (!WymagaKlucza || _odblokowane) return true;
                    if (SprobujOdblokowac(gracz)) return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>Sprawdza, czy gracz ma klucz. Jeśli tak - otwiera zamek (i ewentualnie zabiera klucz).</summary>
        bool SprobujOdblokowac(Gracz gracz)
        {
            if (!WymagaKlucza || _odblokowane) return true;

            if (gracz.Ekwipunek != null && gracz.Ekwipunek.MaKlucz(wymaganyKlucz))
            {
                if (zuzywaKlucz) gracz.Ekwipunek.ZuzyjKlucz(wymaganyKlucz);
                _odblokowane = true;
                gdyOtwartoKluczem.Wyslij(transform.position, NazwaKlucza);
                return true;
            }

            // informacja zwrotna dla gracza - ale nie częściej niż co 3 sekundy i tylko dla postaci, którą steruje
            if (gracz.CzySterowana && Time.time - _czasKomunikatu > 3f)
            {
                _czasKomunikatu = Time.time;
                gdyBrakKlucza.Wyslij(transform.position, NazwaKlucza);
            }
            return false;
        }

        void UstawSkrzydlo(float postep, bool natychmiast)
        {
            if (skrzydlo == null) return;
            float t = Mathf.SmoothStep(0f, 1f, postep); // łagodny start i koniec ruchu

            Vector3 pozycjaLokalna = _zamknietaPozycja;
            Quaternion obrotLokalny = _zamknietyObrot;
            if (rodzajRuchu == RuchDrzwi.Przesuwanie) pozycjaLokalna += przesuniecie * t;
            else obrotLokalny = _zamknietyObrot * Quaternion.Euler(obrot * t);

            Transform rodzic = skrzydlo.parent;
            Vector3 pozycja = rodzic != null ? rodzic.TransformPoint(pozycjaLokalna) : pozycjaLokalna;
            Quaternion rotacja = rodzic != null ? rodzic.rotation * obrotLokalny : obrotLokalny;

            if (_cialo != null && !natychmiast)
            {
                // ruch przez Rigidbody - dzięki temu drzwi poprawnie popychają skrzynie
                _cialo.MovePosition(pozycja);
                _cialo.MoveRotation(rotacja);
            }
            else
            {
                // ustawienie natychmiastowe (start sceny): przestawiamy i obiekt, i jego Rigidbody,
                // inaczej fizyka w następnym kroku cofnęłaby skrzydło na starą pozycję
                skrzydlo.SetPositionAndRotation(pozycja, rotacja);
                if (_cialo != null)
                {
                    _cialo.position = pozycja;
                    _cialo.rotation = rotacja;
                }
            }
        }

        void OdswiezWyglad()
        {
            if (znacznikiStanu == null) return;
            Color kolor;
            if (Otwarte) kolor = Paleta.Aktywny;
            else if (WymagaKlucza && !_odblokowane) kolor = wymaganyKlucz.kolor;
            else kolor = Paleta.Nieaktywny;
            Paleta.Pomaluj(znacznikiStanu, kolor, 1.2f);
        }

        // --- otwieranie klawiszem E ---

        public bool CzyMoznaUzyc(Gracz gracz)
        {
            return sposobOtwierania == SposobOtwierania.Interakcja && !_zatrzasnieteOtwarte;
        }

        public string TekstPodpowiedzi(Gracz gracz)
        {
            if (WymagaKlucza && !_odblokowane)
            {
                bool ma = gracz.Ekwipunek != null && gracz.Ekwipunek.MaKlucz(wymaganyKlucz);
                return Powiadomienie.Wstaw(ma ? podpowiedzOtworzKluczem : podpowiedzBrakKlucza, NazwaKlucza);
            }
            return _otwarteRecznie ? podpowiedzZamknij : podpowiedzOtworz;
        }

        public void Uzyj(Gracz gracz)
        {
            _czasKomunikatu = -10f; // przy użyciu E zawsze pokaż komunikat o braku klucza
            if (!SprobujOdblokowac(gracz)) return;
            _otwarteRecznie = !_otwarteRecznie;
        }

        // --- podgląd w oknie Scene: gdzie znajdzie się skrzydło po otwarciu ---

        void OnDrawGizmosSelected()
        {
            if (skrzydlo == null) return;
            MeshFilter siatka = skrzydlo.GetComponentInChildren<MeshFilter>();
            if (siatka == null || siatka.sharedMesh == null) return;

            Vector3 pozycjaLokalna = Application.isPlaying ? _zamknietaPozycja : skrzydlo.localPosition;
            Quaternion obrotLokalny = Application.isPlaying ? _zamknietyObrot : skrzydlo.localRotation;
            if (rodzajRuchu == RuchDrzwi.Przesuwanie) pozycjaLokalna += przesuniecie;
            else obrotLokalny *= Quaternion.Euler(obrot);

            Transform rodzic = skrzydlo.parent;
            Matrix4x4 m = (rodzic != null ? rodzic.localToWorldMatrix : Matrix4x4.identity)
                          * Matrix4x4.TRS(pozycjaLokalna, obrotLokalny, skrzydlo.localScale);
            if (siatka.transform != skrzydlo) m *= skrzydlo.worldToLocalMatrix * siatka.transform.localToWorldMatrix;

            Gizmos.color = Paleta.Aktywny;
            Gizmos.matrix = m;
            Gizmos.DrawWireCube(siatka.sharedMesh.bounds.center, siatka.sharedMesh.bounds.size);
            Gizmos.matrix = Matrix4x4.identity;

            if (sposobOtwierania == SposobOtwierania.Automatycznie)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, zasiegAutomatu);
            }
        }
    }
}
