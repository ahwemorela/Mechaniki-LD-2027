using System.Collections.Generic;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MechanikiLD
{
    /// <summary>
    /// "Wizytówka" postaci. Zbiera w jednym miejscu odwołania do wszystkich jej komponentów,
    /// żeby mechaniki (drzwi, przyciski, strefy...) mogły łatwo zapytać np. o ekwipunek.
    ///
    /// Na scenie może być KILKA postaci (mechanika zamiany ciał): gracz steruje jedną naraz,
    /// pozostałe stoją tam, gdzie je zostawił - nadal wciskają przyciski, jeżdżą na platformach itd.
    /// Postać sterowana to Gracz.Instancja, wszystkie są na liście Gracz.Wszyscy.
    ///
    /// Komponent siedzi na obiekcie PlayerCapsule w prefabie "Gracz" oraz na prefabie "DrugieCialo".
    /// </summary>
    [DefaultExecutionOrder(-20)]
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz")]
    public class Gracz : MonoBehaviour
    {
        /// <summary>Postać, którą w tej chwili steruje gracz.</summary>
        public static Gracz Instancja { get; private set; }

        /// <summary>Wszystkie postacie na scenie (sterowana i pozostałe).</summary>
        public static readonly List<Gracz> Wszyscy = new List<Gracz>();

        [Header("Postać")]
        [Tooltip("Nazwa postaci pokazywana graczowi (np. przy zamianie ciał): \"Ciało A\", \"Robot\", \"Duch\"...")]
        public string nazwaPostaci = "Gracz";

        [Tooltip("Czy gracz zaczyna poziom w tej postaci. Gdy na scenie jest kilka postaci, zaznacz to tylko u jednej.")]
        public bool sterowanaNaStarcie = true;

        [Tooltip("Punkt, z którego postać \"patrzy\" (PlayerCameraRoot). Stąd wychodzą promienie interakcji i haka.")]
        public Transform oczy;

        [Tooltip("Interfejs gracza (obiekt HUD z komponentem Gracz HUD). Puste = zostanie znaleziony automatycznie na scenie.")]
        public GraczHUD hud;

        [Header("Wygląd ciała (widoczny, gdy patrzy się na postać z zewnątrz)")]
        [Tooltip("Model ciała, który ma przyjąć kolor postaci. Puste = kolor się nie zmienia.")]
        public Renderer modelCiala;
        public Color kolorCiala = new Color(0.45f, 0.6f, 0.85f);

        public FirstPersonController Kontroler { get; private set; }
        public CharacterController Kapsula { get; private set; }
        public GraczEkwipunek Ekwipunek { get; private set; }
        public GraczInterakcja Interakcja { get; private set; }
        public GraczPodnoszenie Podnoszenie { get; private set; }
        public GraczHak Hak { get; private set; }
        public GraczOdrodzenie Odrodzenie { get; private set; }
        public GraczSkala Skala { get; private set; }
        public GraczZamianaCial ZamianaCial { get; private set; }
        public GraczHUD HUD { get; private set; }

        /// <summary>Wejście (WSAD, mysz, skok), z którego korzysta ta postać. Postać niesterowana dostaje "puste" wejście.</summary>
        public StarterAssetsInputs WejscieRuchu { get; private set; }

        /// <summary>
        /// Trwa przelot kamery z ciała do ciała (efekt "opętania" w zamianie ciał).
        /// W tym czasie żadna postać nie reaguje na klawisze.
        /// </summary>
        public static bool TrwaPrzelot { get; private set; }

        /// <summary>Czy gracz steruje właśnie tą postacią (w trakcie przelotu kamery - żadną).</summary>
        public bool CzySterowana => Instancja == this && !TrwaPrzelot;

        /// <summary>Aktualny rozmiar postaci: 1 = normalny, 0.3 = pomniejszona, 2.5 = powiększona.</summary>
        public float Rozmiar => transform.lossyScale.y;

        /// <summary>Środek ciała (mniej więcej klatka piersiowa).</summary>
        public Vector3 Srodek => transform.position + Vector3.up * (1.0f * Rozmiar);

        /// <summary>Głowa - używana np. przez strażników do sprawdzania, czy widzą postać.</summary>
        public Vector3 Glowa => transform.position + Vector3.up * (1.7f * Rozmiar);

        /// <summary>Maska warstw "cały świat oprócz postaci gracza" - do promieni wychodzących z oczu.</summary>
        public int MaskaSwiata => ~((1 << gameObject.layer) | (1 << 2));

        StarterAssetsInputs _wejscieWlasne;   // wejście na tym obiekcie (zasilane przez PlayerInput, jeśli jest włączony)
        StarterAssetsInputs _wejsciePuste;    // zawsze zerowe - dla postaci, którą nikt nie steruje
        static StarterAssetsInputs _wejscieGlowne;
        static int _klatkaUstalenia = -1;

        void Awake()
        {
            Wszyscy.Add(this);
            if (Instancja == null) Instancja = this;

            Kontroler = GetComponent<FirstPersonController>();
            Kapsula = GetComponent<CharacterController>();
            Ekwipunek = GetComponent<GraczEkwipunek>();
            Interakcja = GetComponent<GraczInterakcja>();
            Podnoszenie = GetComponent<GraczPodnoszenie>();
            Hak = GetComponent<GraczHak>();
            Odrodzenie = GetComponent<GraczOdrodzenie>();
            Skala = GetComponent<GraczSkala>();
            ZamianaCial = GetComponent<GraczZamianaCial>();
            HUD = hud != null ? hud : FindFirstObjectByType<GraczHUD>(FindObjectsInactive.Include);

            if (oczy == null && Kontroler != null && Kontroler.CinemachineCameraTarget != null)
                oczy = Kontroler.CinemachineCameraTarget.transform;

            _wejscieWlasne = GetComponent<StarterAssetsInputs>();
            WejscieRuchu = _wejscieWlasne;

            // "puste" wejście: osobny obiekt-dziecko, do którego nie trafiają klawisze
            var puste = new GameObject("Puste wejscie");
            puste.transform.SetParent(transform, false);
            _wejsciePuste = puste.AddComponent<StarterAssetsInputs>();

            OdswiezKolor();
        }

        void OnValidate() { OdswiezKolor(); }

        void OdswiezKolor() { if (modelCiala != null) Paleta.Pomaluj(modelCiala, kolorCiala); }

        void Start()
        {
            // raz na start sceny ustalamy, którą postacią steruje gracz
            if (_klatkaUstalenia != Time.frameCount)
            {
                _klatkaUstalenia = Time.frameCount;
                UstalSterowanie();
            }
        }

        void OnDestroy()
        {
            Wszyscy.Remove(this);
            if (Instancja == this) Instancja = Wszyscy.Count > 0 ? Wszyscy[0] : null;
            if (Wszyscy.Count == 0) TrwaPrzelot = false;
        }

        // ---------- KTÓRĄ POSTACIĄ STERUJE GRACZ ----------

        static void UstalSterowanie()
        {
            if (Wszyscy.Count == 0) return;

            // Klawisze i mysz odbiera tylko ta postać, która ma włączony komponent PlayerInput (prefab "Gracz").
            // Jej wejście "pożyczamy" tej postaci, którą aktualnie steruje gracz.
            _wejscieGlowne = null;
            foreach (Gracz g in Wszyscy)
            {
                PlayerInput pi = g.GetComponent<PlayerInput>();
                if (pi != null && pi.enabled && g._wejscieWlasne != null) { _wejscieGlowne = g._wejscieWlasne; break; }
            }

            Gracz startowa = Wszyscy.Find(g => g.sterowanaNaStarcie);
            if (startowa == null) startowa = Wszyscy[0];
            if (_wejscieGlowne == null) _wejscieGlowne = startowa._wejscieWlasne;

            foreach (Gracz g in Wszyscy) g.UstawSterowanie(g == startowa, true);
        }

        /// <summary>Przekazuje sterowanie innej postaci OD RAZU, cięciem kamery (bez przelotu).</summary>
        public static void PrzejmijSterowanie(Gracz nowa)
        {
            if (nowa == null) return;
            if (TrwaPrzelot) { TrwaPrzelot = false; Instancja = null; } // przerwany przelot - kończymy go tutaj
            if (nowa == Instancja) return;
            Gracz stara = Instancja;
            if (stara != null) stara.UstawSterowanie(false, false);
            nowa.UstawSterowanie(true, false);
        }

        /// <summary>
        /// Początek przelotu kamery do innej postaci: dotychczasowa traci sterowanie,
        /// a nowa dostanie je dopiero w ZakonczPrzelot(). Kamerą w tym czasie porusza GraczZamianaCial.
        /// </summary>
        public static void RozpocznijPrzelot(Gracz nowa)
        {
            if (nowa == null || nowa == Instancja || TrwaPrzelot) return;
            Gracz stara = Instancja;
            if (stara != null) stara.UstawSterowanie(false, false);
            Instancja = nowa;   // interfejs od razu pokazuje nową postać
            TrwaPrzelot = true; // ...ale klawisze zadziałają dopiero po przelocie
        }

        /// <summary>Koniec przelotu kamery: postać, do której lecieliśmy, dostaje sterowanie i kamerę.</summary>
        public static void ZakonczPrzelot()
        {
            if (!TrwaPrzelot) return;
            TrwaPrzelot = false;
            if (Instancja != null) Instancja.UstawSterowanie(true, false);
        }

        /// <summary>Wirtualna kamera gracza (Cinemachine) - ta, która "siedzi w oczach" sterowanej postaci.</summary>
        public static CinemachineCamera KameraGracza
        {
            get
            {
                if (_kamera == null) _kamera = FindFirstObjectByType<CinemachineCamera>();
                return _kamera;
            }
        }
        static CinemachineCamera _kamera;

        void UstawSterowanie(bool sterowana, bool naStarcie)
        {
            if (sterowana)
            {
                Instancja = this;
                WejscieRuchu = _wejscieGlowne != null ? _wejscieGlowne : _wejscieWlasne;

                // kamera ma teraz patrzeć oczami tej postaci
                CinemachineCamera kamera = KameraGracza;
                if (kamera != null && oczy != null && kamera.Follow != oczy)
                {
                    kamera.Follow = oczy;
                    kamera.PreviousStateIsValid = false; // natychmiastowe cięcie (płynny przelot robi GraczZamianaCial)
                }
            }
            else
            {
                WejscieRuchu = _wejsciePuste;
                if (!naStarcie)
                {
                    // porzucana postać puszcza to, co trzymała
                    if (Hak != null) Hak.Odczep(false);
                    if (Podnoszenie != null) Podnoszenie.Upusc();
                }
            }

            if (WejscieRuchu != null)
            {
                // zerujemy wciśnięte klawisze, żeby nowa postać nie "odziedziczyła" ruchu poprzedniej
                WejscieRuchu.move = Vector2.zero;
                WejscieRuchu.look = Vector2.zero;
                WejscieRuchu.jump = false;
                WejscieRuchu.sprint = false;
            }
            if (Kontroler != null) Kontroler.SetInput(WejscieRuchu);
        }

        // ---------- STREFY ----------
        // Co klatkę sprawdzamy, w jakich strefach (colliderach z zaznaczonym "Is Trigger") stoi postać,
        // i powiadamiamy je o wejściu, przebywaniu i wyjściu. Robimy to własnym zapytaniem do fizyki,
        // bo jest to pewniejsze niż zdarzenia OnTrigger... w połączeniu z CharacterControllerem i teleportacją.

        readonly Collider[] _bufor = new Collider[32];
        List<IStrefaGracza> _strefyTeraz = new List<IStrefaGracza>();
        List<IStrefaGracza> _strefyPoprzednio = new List<IStrefaGracza>();

        void Update()
        {
            SprawdzStrefy();
        }

        void SprawdzStrefy()
        {
            // zamieniamy listy miejscami: "teraz" z poprzedniej klatki staje się "poprzednio"
            List<IStrefaGracza> tmp = _strefyPoprzednio;
            _strefyPoprzednio = _strefyTeraz;
            _strefyTeraz = tmp;
            _strefyTeraz.Clear();

            if (Kapsula != null && Kapsula.enabled)
            {
                // wymiary kapsuły przeliczone przez aktualny rozmiar postaci
                float skala = Rozmiar;
                float promien = Kapsula.radius * skala;
                Vector3 srodek = transform.TransformPoint(Kapsula.center);
                float pol = Mathf.Max(Kapsula.height * skala * 0.5f - promien, 0f);
                int ile = Physics.OverlapCapsuleNonAlloc(srodek + Vector3.up * pol, srodek - Vector3.up * pol, promien,
                    _bufor, ~0, QueryTriggerInteraction.Collide);
                for (int i = 0; i < ile; i++)
                {
                    if (!_bufor[i].isTrigger) continue;
                    IStrefaGracza strefa = _bufor[i].GetComponentInParent<IStrefaGracza>();
                    if (strefa != null && !_strefyTeraz.Contains(strefa)) _strefyTeraz.Add(strefa);
                }
            }

            foreach (IStrefaGracza s in _strefyTeraz)
            {
                if (!_strefyPoprzednio.Contains(s)) s.GraczWszedl(this);
                s.GraczWStrefie(this);
            }
            foreach (IStrefaGracza s in _strefyPoprzednio)
            {
                // (Object)s == null -> obiekt strefy został w międzyczasie zniszczony
                if (!_strefyTeraz.Contains(s) && (s as Object) != null) s.GraczWyszedl(this);
            }
        }

        /// <summary>
        /// Zwraca postać, jeśli podany collider do niej należy (inaczej null).
        /// Używane w strefach: void OnTriggerEnter(Collider c) { var gracz = Gracz.Z(c); ... }
        /// </summary>
        public static Gracz Z(Collider collider)
        {
            return collider != null ? collider.GetComponentInParent<Gracz>() : null;
        }

        /// <summary>Skrót do pokazania krótkiego komunikatu na ekranie.</summary>
        public static void Komunikat(string tekst, float czas = 2.5f)
        {
            if (string.IsNullOrEmpty(tekst)) return;
            if (Instancja != null && Instancja.HUD != null) Instancja.HUD.Komunikat(tekst, czas);
        }
    }
}
