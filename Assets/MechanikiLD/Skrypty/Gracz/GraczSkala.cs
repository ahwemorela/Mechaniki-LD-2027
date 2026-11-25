using System.Collections;
using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// ZMIANA SKALI POSTACI.
    /// Postać może się zmniejszyć albo urosnąć (zwykle po zebraniu przedmiotu "Zmiana Skali").
    /// Razem z rozmiarem zmieniają się jej możliwości:
    ///  - MAŁA: mieści się w wąskich szybach i pod niskimi przeszkodami, ale skacze nisko,
    ///          porusza się wolniej i nie udźwignie skrzyni,
    ///  - DUŻA: skacze wysoko i daleko, przekracza przeszkody, dźwiga ciężkie rzeczy i wciska
    ///          "ciężkie" przyciski, ale nie zmieści się w zwykłych drzwiach.
    /// Ten sam poziom czyta się więc zupełnie inaczej w każdym rozmiarze.
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ: suwaki poniżej decydują, JAK BARDZO rozmiar wpływa na ruch.
    /// "Wplyw Na Predkosc" = 0 da małą postać równie szybką jak normalna (mysz), 1 - bardzo powolną (ślimak).
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz Skala")]
    public class GraczSkala : MonoBehaviour
    {
        [Header("Przebieg zmiany")]
        [Tooltip("Ile sekund trwa płynne przejście do nowego rozmiaru.")]
        public float czasZmiany = 0.4f;

        [Header("Wpływ rozmiaru na możliwości postaci")]
        [Tooltip("Jak mocno rozmiar zmienia prędkość ruchu.\n0 = prędkość zawsze taka sama, 1 = wprost proporcjonalna do rozmiaru (2x większy = 2x szybszy).")]
        [Range(0f, 1f)] public float wplywNaPredkosc = 0.5f;
        [Tooltip("Jak mocno rozmiar zmienia wysokość skoku.\n0 = skok zawsze taki sam, 1 = wprost proporcjonalny do rozmiaru.")]
        [Range(0f, 1f)] public float wplywNaSkok = 1f;
        [Tooltip("Czy rozmiar zmienia zasięg rąk (interakcja, noszenie) i udźwig.")]
        public bool skalujZasiegIUdzwig = true;

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        [Tooltip("Gdy postać nie może urosnąć, bo coś jest nad nią lub obok niej.")]
        public Powiadomienie gdyBrakMiejsca = new Powiadomienie("Za mało miejsca, żeby urosnąć");
        [Tooltip("Gdy czasowa zmiana rozmiaru się kończy.")]
        public Powiadomienie gdyPowrotDoNormy = new Powiadomienie();

        /// <summary>Rozmiar, do którego postać dąży (1 = normalny).</summary>
        public float Docelowa { get; private set; } = 1f;

        Gracz _gracz;
        Vector3 _skalaBazowa;
        float _aktualna = 1f;
        Coroutine _przejscie;
        float _koniecCzasowej = -1f;   // kiedy kończy się zmiana czasowa (-1 = bez limitu)
        float _czasCzasowej;

        // wartości bazowe (dla rozmiaru 1), od których liczymy przeskalowane
        float _chod, _bieg, _skok, _promienZiemi, _odstepZiemi;
        float _zasieg, _trzymanie, _udzwig;

        void Awake()
        {
            _gracz = GetComponent<Gracz>();
            _skalaBazowa = transform.localScale;
        }

        void Start()
        {
            var k = _gracz.Kontroler;
            _chod = k.MoveSpeed; _bieg = k.SprintSpeed; _skok = k.JumpHeight;
            _promienZiemi = k.GroundedRadius; _odstepZiemi = k.GroundedOffset;
            if (_gracz.Interakcja != null) _zasieg = _gracz.Interakcja.zasieg;
            if (_gracz.Podnoszenie != null) { _trzymanie = _gracz.Podnoszenie.odlegloscTrzymania; _udzwig = _gracz.Podnoszenie.maksymalnaMasa; }
        }

        void Update()
        {
            if (_koniecCzasowej < 0f) return;

            // zmiana czasowa: pokazujemy odliczanie, a po czasie wracamy do normalnego rozmiaru
            float zostalo = _koniecCzasowej - Time.time;
            if (zostalo > 0f)
            {
                if (_gracz.CzySterowana && _gracz.HUD != null) _gracz.HUD.ZglosOdliczanie(zostalo, _czasCzasowej);
                return;
            }
            // jeśli nie ma miejsca, żeby urosnąć (np. w szybie), próbujemy ponownie w kolejnych klatkach
            if (UstawSkale(1f, 0f, false))
            {
                gdyPowrotDoNormy.Wyslij(transform.position);
            }
        }

        /// <summary>
        /// Zmienia rozmiar postaci. "czasTrwania" > 0 oznacza zmianę czasową (potem powrót do rozmiaru 1).
        /// Zwraca false, jeśli postać nie może urosnąć z braku miejsca.
        /// </summary>
        public bool UstawSkale(float skala, float czasTrwania = 0f, bool komunikatOBrakuMiejsca = true)
        {
            skala = Mathf.Clamp(skala, 0.1f, 10f);

            // rośniemy tylko wtedy, gdy większa postać zmieści się w tym miejscu
            if (skala > _aktualna && !CzyZmiesciSie(skala))
            {
                if (komunikatOBrakuMiejsca && _gracz.CzySterowana) gdyBrakMiejsca.Wyslij(transform.position);
                return false;
            }

            Docelowa = skala;
            _czasCzasowej = czasTrwania;
            _koniecCzasowej = czasTrwania > 0f ? Time.time + czasTrwania : -1f;

            if (_przejscie != null) StopCoroutine(_przejscie);
            _przejscie = StartCoroutine(Przejscie(skala));
            return true;
        }

        IEnumerator Przejscie(float cel)
        {
            float start = _aktualna;
            for (float t = 0f; t < czasZmiany; t += Time.deltaTime)
            {
                Zastosuj(Mathf.Lerp(start, cel, Mathf.SmoothStep(0f, 1f, t / czasZmiany)));
                yield return null;
            }
            Zastosuj(cel);
            _przejscie = null;
        }

        /// <summary>Ustawia rozmiar i przelicza wszystko, co od niego zależy.</summary>
        void Zastosuj(float s)
        {
            _aktualna = s;
            transform.localScale = _skalaBazowa * s;

            // ruch
            var k = _gracz.Kontroler;
            float predkosc = Mathf.Pow(s, wplywNaPredkosc);
            k.MoveSpeed = _chod * predkosc;
            k.SprintSpeed = _bieg * predkosc;
            k.JumpHeight = _skok * Mathf.Pow(s, wplywNaSkok);
            // "czujnik ziemi" kontrolera ma wymiary w metrach, więc też musi się skalować
            k.GroundedRadius = _promienZiemi * s;
            k.GroundedOffset = _odstepZiemi * s;

            // ręce: zasięg rośnie wolniej niż postać, udźwig - szybciej
            if (skalujZasiegIUdzwig)
            {
                float rece = (1f + s) * 0.5f;
                if (_gracz.Interakcja != null) _gracz.Interakcja.zasieg = _zasieg * rece;
                if (_gracz.Podnoszenie != null)
                {
                    _gracz.Podnoszenie.odlegloscTrzymania = _trzymanie * rece;
                    _gracz.Podnoszenie.maksymalnaMasa = _udzwig * s * s;
                }
            }
        }

        /// <summary>Czy postać o podanym rozmiarze zmieściłaby się w miejscu, w którym teraz stoi.</summary>
        bool CzyZmiesciSie(float skala)
        {
            CharacterController kapsula = _gracz.Kapsula;
            float promien = kapsula.radius * skala * 0.95f;
            float wysokosc = kapsula.height * skala;
            Vector3 dol = transform.position + Vector3.up * (promien + 0.08f);
            Vector3 gora = transform.position + Vector3.up * Mathf.Max(wysokosc - promien, promien + 0.08f);
            return !Physics.CheckCapsule(dol, gora, promien, _gracz.MaskaSwiata, QueryTriggerInteraction.Ignore);
        }
    }
}
