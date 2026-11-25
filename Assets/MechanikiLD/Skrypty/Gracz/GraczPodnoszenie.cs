using UnityEngine;
using UnityEngine.InputSystem;

namespace MechanikiLD
{
    /// <summary>
    /// NOSZENIE PRZEDMIOTÓW FIZYCZNYCH.
    /// Gracz podnosi obiekt z komponentem "Przedmiot Do Podnoszenia" klawiszem E,
    /// niesie go przed sobą, odkłada (E) albo rzuca (lewy przycisk myszy).
    ///
    /// Przedmiot nie jest "przyklejony" do kamery - jest ciągnięty fizyką w stronę punktu przed graczem,
    /// dzięki temu nie przenika przez ściany i może np. wcisnąć przycisk naciskowy.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz Podnoszenie")]
    public class GraczPodnoszenie : MonoBehaviour
    {
        [Tooltip("Jak daleko przed oczami gracza wisi niesiony przedmiot (metry).")]
        public float odlegloscTrzymania = 1.9f;

        [Tooltip("Z jaką prędkością (m/s) przedmiot wylatuje z rąk przy rzucie.")]
        public float silaRzutu = 9f;

        [Tooltip("Cięższych przedmiotów gracz nie podniesie (kg).")]
        public float maksymalnaMasa = 50f;

        [Tooltip("Podpowiedź pod celownikiem, gdy postać coś niesie.")]
        public string podpowiedzOdlozenia = "Odłóż   (LPM – rzuć)";

        [Tooltip("Klawisz rzutu (działa tylko, gdy gracz coś niesie).")]
        public InputAction klawiszRzutu = Wejscie.Przycisk("Rzut", "<Mouse>/leftButton", "<Gamepad>/rightTrigger");

        /// <summary>Aktualnie niesiony przedmiot (null, gdy ręce są puste).</summary>
        public PrzedmiotDoPodnoszenia Trzymany { get; private set; }

        /// <summary>Kiedy ostatnio gracz odłożył/rzucił przedmiot (żeby to samo kliknięcie nie wystrzeliło haka).</summary>
        public float CzasOstatniegoPuszczenia { get; private set; } = -10f;

        Gracz _gracz;
        Rigidbody _cialo;
        bool _mialGrawitacje;
        float _mialOporObrotu;
        float _katWzgledemGracza;
        float _czasDaleko;
        bool _rzut;

        void Awake() { _gracz = GetComponent<Gracz>(); }
        void OnEnable() { klawiszRzutu.Enable(); }
        void OnDisable() { klawiszRzutu.Disable(); Upusc(); }

        public bool MozePodniesc(PrzedmiotDoPodnoszenia przedmiot)
        {
            return enabled && Trzymany == null && przedmiot != null && przedmiot.Cialo != null && przedmiot.Cialo.mass <= maksymalnaMasa;
        }

        public void Podnies(PrzedmiotDoPodnoszenia przedmiot)
        {
            if (!MozePodniesc(przedmiot)) return;

            Trzymany = przedmiot;
            _cialo = przedmiot.Cialo;
            _czasDaleko = 0f;

            // zapamiętujemy ustawienia fizyki, żeby je przywrócić przy odłożeniu
            _mialGrawitacje = _cialo.useGravity;
            _mialOporObrotu = _cialo.angularDamping;
            _cialo.useGravity = false;
            _cialo.angularDamping = 10f;
            _katWzgledemGracza = Mathf.DeltaAngle(transform.eulerAngles.y, _cialo.rotation.eulerAngles.y);

            // niesiony przedmiot nie może zderzać się z graczem (inaczej dałoby się na nim "latać")
            UstawKolizjeZGraczem(przedmiot, false);
            przedmiot.GdyPodniesiony(_gracz);
        }

        public void Upusc()
        {
            if (Trzymany == null) return;

            _cialo.useGravity = _mialGrawitacje;
            _cialo.angularDamping = _mialOporObrotu;
            UstawKolizjeZGraczem(Trzymany, true);
            Trzymany.GdyUpuszczony(_rzut);

            Trzymany = null;
            _cialo = null;
            CzasOstatniegoPuszczenia = Time.time;
        }

        public void Rzuc()
        {
            if (Trzymany == null) return;
            Rigidbody cialo = _cialo;
            _rzut = true;
            Upusc();
            _rzut = false;
            cialo.linearVelocity = _gracz.oczy.forward * silaRzutu;
        }

        void Update()
        {
            if (Trzymany == null) return;

            // przedmiot mógł zostać wyłączony lub zniszczony (np. wpadł do strefy śmierci)
            if (!Trzymany.gameObject.activeInHierarchy) { Upusc(); return; }

            if (_gracz.CzySterowana && klawiszRzutu.WasPressedThisFrame()) Rzuc();
        }

        void FixedUpdate()
        {
            if (Trzymany == null || _cialo == null) return;

            // punkt przed oczami gracza, trochę poniżej linii wzroku, ale nigdy pod podłogą
            Transform oczy = _gracz.oczy;
            float rozmiar = _gracz.Rozmiar;
            Vector3 cel = oczy.position + oczy.forward * odlegloscTrzymania - oczy.up * (0.25f * rozmiar);
            cel.y = Mathf.Max(cel.y, transform.position.y + 0.5f);

            Vector3 roznica = cel - _cialo.worldCenterOfMass;

            // przedmiot zaklinował się za ścianą albo został daleko w tyle -> po chwili wypada z rąk
            // (liczymy czas, żeby szybki obrót gracza o 180° nie wyrywał przedmiotu z rąk)
            if (roznica.magnitude > 2.5f) _czasDaleko += Time.fixedDeltaTime; else _czasDaleko = 0f;
            if (_czasDaleko > 0.5f) { Upusc(); return; }

            // ciągniemy przedmiot do celu prędkością (a nie teleportem), więc dalej zderza się ze światem
            _cialo.linearVelocity = Vector3.ClampMagnitude(roznica * 14f, 18f) + _gracz.Kapsula.velocity;

            // łagodnie obracamy przedmiot tak, żeby stał prosto i obracał się razem z graczem
            Quaternion docelowy = Quaternion.Euler(0f, transform.eulerAngles.y + _katWzgledemGracza, 0f);
            Quaternion delta = docelowy * Quaternion.Inverse(_cialo.rotation);
            delta.ToAngleAxis(out float kat, out Vector3 os);
            if (kat > 180f) kat -= 360f;
            if (!float.IsNaN(os.x) && !float.IsInfinity(os.x))
                _cialo.angularVelocity = os * (kat * Mathf.Deg2Rad * 8f);
        }

        void UstawKolizjeZGraczem(PrzedmiotDoPodnoszenia przedmiot, bool wlaczone)
        {
            Collider[] kolideryGracza = GetComponentsInChildren<Collider>();
            foreach (Collider kp in przedmiot.GetComponentsInChildren<Collider>())
                foreach (Collider kg in kolideryGracza)
                    if (kp != null && kg != null) Physics.IgnoreCollision(kp, kg, !wlaczone);
        }
    }
}
