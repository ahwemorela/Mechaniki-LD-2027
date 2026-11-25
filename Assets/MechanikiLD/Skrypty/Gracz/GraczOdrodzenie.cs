using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MechanikiLD
{
    /// <summary>
    /// ODRADZANIE GRACZA.
    /// Zapamiętuje ostatni punkt kontrolny (na starcie jest nim miejsce, w którym gracz zaczyna poziom).
    /// Gdy gracz "zginie" (strefa śmierci, wykrycie przez strażnika, upadek poza mapę),
    /// ekran na moment ciemnieje i gracz wraca do punktu kontrolnego.
    ///
    /// Klawisz R pozwala wrócić do punktu kontrolnego w dowolnej chwili - to wyjście awaryjne,
    /// gdyby gracz gdzieś utknął. Dobry poziom nie powinien jednak tego wymagać!
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz Odrodzenie")]
    public class GraczOdrodzenie : MonoBehaviour
    {
        [Tooltip("Jeśli gracz spadnie poniżej tej wysokości (oś Y), automatycznie się odradza. Zabezpieczenie przed wypadnięciem poza mapę.")]
        public float minimalnaWysokosc = -30f;

        [Tooltip("Ile trwa ściemnienie i rozjaśnienie ekranu (sekundy).")]
        public float czasZaciemnienia = 0.3f;

        [Tooltip("Klawisz powrotu do ostatniego punktu kontrolnego.")]
        public InputAction klawiszPowrotu = Wejscie.Przycisk("Powrot", "<Keyboard>/r", "<Gamepad>/select");

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        [Tooltip("Komunikat po powrocie do punktu kontrolnego klawiszem R.")]
        public string komunikatPowrotu = "Powrót do punktu kontrolnego";
        [Tooltip("Komunikat po wypadnięciu poza mapę.")]
        public string komunikatPozaMapa = "Wypadłeś poza mapę";
        [Tooltip("Dźwięk przy każdym odrodzeniu.")]
        public AudioClip dzwiekOdrodzenia;

        /// <summary>Zdarzenie wywoływane po każdym odrodzeniu gracza (np. zapadnięte platformy wracają wtedy na miejsce).</summary>
        public static event Action GraczOdrodzony;

        /// <summary>0 = ekran normalny, 1 = całkiem czarny. Rysuje to HUD.</summary>
        public float Zaciemnienie { get; private set; }

        /// <summary>Ile razy gracz zginął na tym poziomie (przydatne przy testach z graczami).</summary>
        public int LiczbaOdrodzen { get; private set; }

        /// <summary>Ustawiane przez Metę Poziomu - po ukończeniu poziomu R restartuje scenę.</summary>
        public bool PoziomUkonczony { get; set; }

        Gracz _gracz;
        Vector3 _pozycja;
        float _obrot;
        bool _wTrakcie;

        void Awake() { _gracz = GetComponent<Gracz>(); }
        void OnEnable() { klawiszPowrotu.Enable(); }
        void OnDisable() { klawiszPowrotu.Disable(); }

        void Start()
        {
            // pierwszym punktem kontrolnym jest miejsce startu
            _pozycja = transform.position;
            _obrot = transform.eulerAngles.y;
        }

        void Update()
        {
            // klawisz R działa tylko dla postaci, którą gracz steruje
            if (_gracz.CzySterowana && klawiszPowrotu.WasPressedThisFrame())
            {
                if (PoziomUkonczony) ZaladujScenePonownie();
                else Zgin(komunikatPowrotu);
            }

            if (transform.position.y < minimalnaWysokosc) Zgin(komunikatPozaMapa);
        }

        /// <summary>Wczytuje aktualną scenę od nowa (cały poziom wraca do stanu początkowego).</summary>
        public static void ZaladujScenePonownie()
        {
            Scene scena = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            // w edytorze działa to także dla scen, których nie ma na liście Build Settings
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scena.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(scena.buildIndex);
#endif
        }

        /// <summary>Zapamiętuje nowe miejsce odradzania.</summary>
        public void UstawPunktKontrolny(Vector3 pozycja, float obrotY)
        {
            _pozycja = pozycja;
            _obrot = obrotY;
        }

        /// <summary>"Zabija" gracza: ściemnia ekran i przenosi go do ostatniego punktu kontrolnego.</summary>
        public void Zgin(string komunikat = null)
        {
            if (_wTrakcie || !isActiveAndEnabled) return;
            StartCoroutine(Odrodz(komunikat));
        }

        IEnumerator Odrodz(string komunikat)
        {
            _wTrakcie = true;
            LiczbaOdrodzen++;
            Dzwiek.Odtworz(dzwiekOdrodzenia, transform.position);

            // puszczamy wszystko, co gracz trzyma
            if (_gracz.Hak != null) _gracz.Hak.Odczep(false);
            if (_gracz.Podnoszenie != null) _gracz.Podnoszenie.Upusc();

            // ściemnienie
            for (float t = 0f; t < czasZaciemnienia; t += Time.deltaTime)
            {
                Zaciemnienie = t / czasZaciemnienia;
                yield return null;
            }
            Zaciemnienie = 1f;

            _gracz.Kontroler.Teleport(_pozycja, _obrot);
            GraczOdrodzony?.Invoke();
            if (_gracz.CzySterowana) Gracz.Komunikat(komunikat);
            yield return null;

            // rozjaśnienie
            for (float t = 0f; t < czasZaciemnienia; t += Time.deltaTime)
            {
                Zaciemnienie = 1f - t / czasZaciemnienia;
                yield return null;
            }
            Zaciemnienie = 0f;
            _wTrakcie = false;
        }
    }
}
