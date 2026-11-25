using UnityEngine;

namespace MechanikiLD
{
    public enum ReakcjaNaWykrycie
    {
        [InspectorName("Odródź Gracza")]
        [Tooltip("Gracz wraca do ostatniego punktu kontrolnego.")]
        OdrodzGracza,
        [InspectorName("Tylko Sygnał")]
        [Tooltip("Gracz nie ginie - zostaje tylko wysłany sygnał do celów (np. alarm zamyka bramę).")]
        TylkoSygnal
    }

    /// <summary>
    /// POLE WIDZENIA strażnika lub kamery (mechanika skradania).
    /// Stożek widzenia wychodzi z tego obiektu wzdłuż jego NIEBIESKIEJ strzałki (osi Z).
    /// Jeśli gracz jest w stożku i nic go nie zasłania, rośnie pasek wykrycia. Gdy się zapełni - gracz został wykryty.
    ///
    /// Gracz widzi stożek jako plamę światła na podłodze: tam, gdzie pada światło, tam patrzy strażnik.
    /// Cień za ścianą lub skrzynią to bezpieczna kryjówka - światło i pole widzenia zasłaniają te same obiekty.
    ///
    /// Sam ruch zapewniają inne komponenty: "Ruch Po Trasie" (patrol) albo "Obrót Wahadłowy" (kamera).
    /// Wykrycie może dodatkowo wysłać sygnał do obiektów z listy "Cele" (alarm).
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Pole Widzenia (strażnik, kamera)")]
    public class PoleWidzenia : Aktywator
    {
        [Header("Stożek widzenia")]
        [Tooltip("Jak daleko widzi (metry).")]
        public float zasieg = 12f;
        [Tooltip("Szerokość stożka (stopnie). 60 = wąska latarka, 120 = bardzo szerokie pole.")]
        [Range(10f, 170f)] public float kat = 60f;

        [Header("Wykrywanie")]
        [Tooltip("Ile sekund gracz może być widoczny, zanim zostanie wykryty. To \"margines błędu\" gracza.")]
        [Min(0.05f)] public float czasWykrycia = 1.2f;
        [Tooltip("Jak szybko pasek wykrycia opada, gdy gracz zniknie z pola widzenia (sekundy do zera).")]
        [Min(0.05f)] public float czasZapominania = 1.5f;
        public ReakcjaNaWykrycie reakcja = ReakcjaNaWykrycie.OdrodzGracza;
        [Tooltip("Ile sekund trwa sygnał alarmu wysyłany do celów po wykryciu.")]
        public float czasAlarmu = 4f;

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        [Tooltip("W chwili wykrycia postaci.")]
        public Powiadomienie gdyWykryto = new Powiadomienie("Zostałeś wykryty!");

        [Header("Wygląd")]
        [Tooltip("Reflektor pokazujący pole widzenia (jego kąt i zasięg ustawiają się automatycznie).")]
        public Light swiatlo;
        [Tooltip("Elementy zmieniające kolor razem ze światłem (np. \"oko\" strażnika).")]
        public Renderer[] kolorowane;
        [Tooltip("Własne ciało strażnika - żeby nie zasłaniał widoku sam sobie.")]
        public Transform wlasneCialo;

        static readonly Color KolorSpokojny = new Color(0.75f, 0.85f, 1f);
        static readonly Color KolorPodejrzliwy = new Color(1f, 0.85f, 0.2f);

        /// <summary>Poziom wykrycia od 0 (spokój) do 1 (wykryty).</summary>
        public float Poziom { get; private set; }

        float _koniecAlarmu;
        bool _dziala = true;

        void OnValidate() { UstawSwiatlo(KolorSpokojny); }
        void Awake() { UstawSwiatlo(KolorSpokojny); }

        /// <summary>Włącza lub wyłącza "oko" (np. przełącznik wyłączający kamerę - do podpięcia przez UnityEvent).</summary>
        public void UstawDzialanie(bool dziala)
        {
            _dziala = dziala;
            if (swiatlo != null) swiatlo.enabled = dziala;
            if (!dziala) Poziom = 0f;
        }

        void Update()
        {
            if (CzyWlaczony && Time.time >= _koniecAlarmu) UstawStan(false);

            if (!_dziala || Gracz.Wszyscy.Count == 0) return;

            // szukamy postaci, którą widać (gdy jest ich kilka - pierwszej widocznej)
            Gracz gracz = null;
            foreach (Gracz g in Gracz.Wszyscy)
                if (CzyWidzi(g)) { gracz = g; break; }
            bool widzi = gracz != null;
            if (gracz == null) gracz = Gracz.Instancja;
            if (gracz == null) return;
            Poziom = Mathf.Clamp01(Poziom + (widzi ? Time.deltaTime / czasWykrycia : -Time.deltaTime / czasZapominania));

            if (gracz.HUD != null && Poziom > 0f) gracz.HUD.ZglosWykrywanie(Poziom);

            Color kolor = Poziom <= 0f ? KolorSpokojny : Color.Lerp(KolorPodejrzliwy, Paleta.Niebezpieczny, Poziom);
            UstawSwiatlo(kolor);

            if (Poziom >= 1f)
            {
                Poziom = 0f;
                _koniecAlarmu = Time.time + czasAlarmu;
                UstawStan(true); // alarm dla celów

                Dzwiek.Odtworz(gdyWykryto.dzwiek, transform.position);
                if (reakcja == ReakcjaNaWykrycie.OdrodzGracza && gracz.Odrodzenie != null)
                    gracz.Odrodzenie.Zgin(gdyWykryto.tekst);   // komunikat pokaże się po odrodzeniu
                else
                    Gracz.Komunikat(gdyWykryto.tekst);
            }
        }

        /// <summary>Czy gracz jest w stożku widzenia i nic go nie zasłania.</summary>
        public bool CzyWidzi(Gracz gracz)
        {
            // sprawdzamy dwa punkty: tułów i głowę (wystająca zza skrzyni głowa też zdradza gracza)
            return CzyWidacPunkt(gracz.Srodek, gracz) || CzyWidacPunkt(gracz.Glowa, gracz);
        }

        bool CzyWidacPunkt(Vector3 punkt, Gracz gracz)
        {
            Vector3 doPunktu = punkt - transform.position;
            float odleglosc = doPunktu.magnitude;
            if (odleglosc > zasieg) return false;                                        // za daleko
            if (Vector3.Angle(transform.forward, doPunktu) > kat * 0.5f) return false;    // poza stożkiem

            // promień od oka do gracza: jeśli po drodze jest ściana lub skrzynia, gracz jest ukryty
            RaycastHit[] trafienia = Physics.RaycastAll(transform.position, doPunktu / odleglosc, odleglosc, gracz.MaskaSwiata, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit t in trafienia)
            {
                if (wlasneCialo != null && t.collider.transform.IsChildOf(wlasneCialo)) continue;
                return false;
            }
            return true;
        }

        void UstawSwiatlo(Color kolor)
        {
            if (swiatlo != null)
            {
                swiatlo.type = LightType.Spot;
                swiatlo.spotAngle = kat;
                swiatlo.innerSpotAngle = kat * 0.9f;
                swiatlo.range = zasieg * 1.05f;
                swiatlo.color = kolor;
            }
            Paleta.Pomaluj(kolorowane, kolor, 1.5f);
        }

        // --- podgląd stożka w oknie Scene ---
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = Paleta.Straznik;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, kat, zasieg, 0.05f, 1f);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
