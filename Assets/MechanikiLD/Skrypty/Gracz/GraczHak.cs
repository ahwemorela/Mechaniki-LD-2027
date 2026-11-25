using UnityEngine;
using UnityEngine.InputSystem;

namespace MechanikiLD
{
    /// <summary>
    /// HAK Z LINĄ.
    /// Przytrzymaj lewy przycisk myszy, celując w żółty "Punkt Haka" - hak się zaczepia, a gracz wisi na linie
    /// i może się na niej bujać jak na wahadle. Puszczenie przycisku odczepia hak, a gracz leci dalej
    /// z prędkością, którą miał w chwili puszczenia.
    ///
    ///  - prawy przycisk myszy (trzymany)  -> wciąganie się w stronę punktu zaczepienia,
    ///  - spacja                           -> puszczenie liny z dodatkowym wybiciem w górę,
    ///  - WSAD w powietrzu                 -> rozbujanie się / korekta lotu.
    ///
    /// Jak to działa "pod maską": dopóki lina jest zaczepiona, ten komponent przejmuje ruch postaci
    /// od FirstPersonController. Co klatkę liczy prędkość (grawitacja + sterowanie), a potem pilnuje,
    /// żeby gracz nie oddalił się od punktu zaczepienia bardziej, niż pozwala długość liny.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz Hak")]
    public class GraczHak : MonoBehaviour
    {
        [Header("Zasięg i celowanie")]
        [Tooltip("Maksymalna odległość (m), na jaką strzela hak. NAJWAŻNIEJSZA liczba przy projektowaniu poziomu z hakiem!")]
        public float zasieg = 22f;

        [Tooltip("Minimalna odległość - hak nie zaczepi się o coś tuż przed nosem.")]
        public float minimalnaOdleglosc = 2f;

        [Tooltip("Zaznaczone: hak łapie TYLKO obiekty z komponentem \"Punkt Haka\" (projektant decyduje, gdzie wolno).\nOdznaczone: hak łapie każdą powierzchnię.")]
        public bool tylkoPunktyHaka = true;

        [Tooltip("Pomoc w celowaniu: promień (m), w jakim hak sam \"znajdzie\" punkt zaczepienia obok celownika.")]
        public float pomocWCelowaniu = 0.6f;

        [Header("Ruch na linie")]
        [Tooltip("Prędkość wciągania (m/s) przy trzymaniu prawego przycisku myszy.")]
        public float predkoscWciagania = 13f;

        [Tooltip("Najkrótsza możliwa lina - bliżej punktu zaczepienia gracz się nie wciągnie.")]
        public float minimalnaDlugoscLiny = 1.5f;

        [Tooltip("Jak mocno WSAD wpływa na ruch w powietrzu (przyspieszenie m/s²). Większa wartość = łatwiej się rozbujać.")]
        public float sterowanieWPowietrzu = 7f;

        [Tooltip("Dodatkowa prędkość w górę przy puszczeniu liny spacją.")]
        public float wybiciePrzyPuszczeniu = 5f;

        [Tooltip("Ograniczenie prędkości na linie (m/s).")]
        public float maksymalnaPredkosc = 28f;

        [Header("Sterowanie")]
        public InputAction klawiszHaka = Wejscie.Przycisk("Hak", "<Mouse>/leftButton", "<Gamepad>/rightTrigger");
        public InputAction klawiszWciagania = Wejscie.Przycisk("Wciaganie", "<Mouse>/rightButton", "<Gamepad>/rightShoulder");

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        public Powiadomienie gdyZaczepiony = new Powiadomienie();
        public Powiadomienie gdyPuszczony = new Powiadomienie();

        [Header("Wygląd liny")]
        [Tooltip("Materiał liny (ustawiony w prefabie gracza).")]
        public Material materialLiny;
        public float gruboscLiny = 0.05f;

        /// <summary>Czy hak jest teraz zaczepiony.</summary>
        public bool Zaczepiony { get; private set; }

        /// <summary>Czy gracz celuje w coś, o co da się zaczepić (HUD zmienia wtedy celownik).</summary>
        public bool MaCel { get; private set; }

        /// <summary>Pozwala innemu skryptowi "trzymać" hak bez wciśniętego klawisza (np. w testach albo przerywniku).</summary>
        public bool WymusTrzymanie { get; set; }

        /// <summary>Pozwala innemu skryptowi wymusić wciąganie liny.</summary>
        public bool WymusWciaganie { get; set; }

        Gracz _gracz;
        LineRenderer _lina;
        Transform _obiekt;          // obiekt, o który zaczepiono hak (może się ruszać)
        Vector3 _punktLokalny;      // punkt zaczepienia w układzie tego obiektu
        float _dlugoscLiny;
        Vector3 _predkosc;

        void Awake()
        {
            _gracz = GetComponent<Gracz>();

            // lina to zwykły LineRenderer z dwoma punktami: ręka gracza i punkt zaczepienia
            var obiektLiny = new GameObject("Lina haka");
            obiektLiny.transform.SetParent(transform, false);
            _lina = obiektLiny.AddComponent<LineRenderer>();
            _lina.positionCount = 2;
            _lina.useWorldSpace = true;
            _lina.startWidth = gruboscLiny * 0.4f; // przy ręce cieńsza, żeby nie zasłaniała widoku
            _lina.endWidth = gruboscLiny;
            _lina.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (materialLiny != null) _lina.sharedMaterial = materialLiny;
            _lina.enabled = false;
        }

        void OnEnable() { klawiszHaka.Enable(); klawiszWciagania.Enable(); }

        void OnDisable()
        {
            klawiszHaka.Disable();
            klawiszWciagania.Disable();
            Odczep(false);
            MaCel = false;
        }

        void Update()
        {
            if (Zaczepiony)
            {
                // obiekt z punktem zaczepienia zniknął albo gracz puścił przycisk
                if (_obiekt == null || !_obiekt.gameObject.activeInHierarchy || !(klawiszHaka.IsPressed() || WymusTrzymanie))
                {
                    Odczep(false);
                }
                else if (_gracz.WejscieRuchu != null && _gracz.WejscieRuchu.jump)
                {
                    _gracz.WejscieRuchu.jump = false;
                    Odczep(true);
                }
                else
                {
                    SymulujRuchNaLinie();
                }
                MaCel = false;
                return;
            }

            // postać, którą gracz akurat nie steruje, nie strzela hakiem
            if (!_gracz.CzySterowana) { MaCel = false; return; }

            // z przedmiotem w rękach nie strzelamy hakiem (LPM to wtedy rzut)
            bool zajeteRece = _gracz.Podnoszenie != null &&
                              (_gracz.Podnoszenie.Trzymany != null || Time.time - _gracz.Podnoszenie.CzasOstatniegoPuszczenia < 0.25f);
            if (zajeteRece) { MaCel = false; return; }

            MaCel = SzukajCelu(out Vector3 punkt, out Transform obiekt);
            if (MaCel && klawiszHaka.WasPressedThisFrame()) Zaczep(punkt, obiekt);
        }

        void LateUpdate()
        {
            if (!Zaczepiony) return;
            Transform oczy = _gracz.oczy;
            Vector3 reka = oczy.position + oczy.right * 0.3f - oczy.up * 0.35f + oczy.forward * 0.3f;
            _lina.SetPosition(0, reka);
            _lina.SetPosition(1, PunktZaczepienia());
        }

        /// <summary>Szuka przed graczem miejsca, o które można zaczepić hak.</summary>
        bool SzukajCelu(out Vector3 punkt, out Transform obiekt)
        {
            punkt = Vector3.zero;
            obiekt = null;
            if (_gracz.oczy == null) return false;

            Vector3 start = _gracz.oczy.position;
            Vector3 kierunek = _gracz.oczy.forward;
            int maska = _gracz.MaskaSwiata;

            // 1) celownik dokładnie na obiekcie
            if (Physics.Raycast(start, kierunek, out RaycastHit trafienie, zasieg, maska, QueryTriggerInteraction.Ignore))
            {
                if (CzyMoznaZaczepic(trafienie.collider) && trafienie.distance >= minimalnaOdleglosc)
                {
                    PunktHaka ph = trafienie.collider.GetComponentInParent<PunktHaka>();
                    punkt = (ph != null && ph.przyciagajDoSrodka) ? ph.transform.position : trafienie.point;
                    obiekt = trafienie.collider.transform;
                    return true;
                }
            }

            // 2) pomoc w celowaniu: "gruby" promień szukający punktów haka obok celownika
            if (pomocWCelowaniu <= 0f) return false;
            RaycastHit[] trafienia = Physics.SphereCastAll(start, pomocWCelowaniu, kierunek, zasieg, maska, QueryTriggerInteraction.Ignore);
            float najlepszy = float.MaxValue;
            bool znaleziono = false;
            foreach (RaycastHit t in trafienia)
            {
                if (t.distance <= 0f || t.distance < minimalnaOdleglosc) continue;
                PunktHaka ph = t.collider.GetComponentInParent<PunktHaka>();
                if (ph == null) continue;

                Vector3 p = ph.przyciagajDoSrodka ? ph.transform.position : t.point;
                // punkt nie może być zasłonięty przez inny obiekt
                if (Physics.Linecast(start, p, out RaycastHit zaslona, maska, QueryTriggerInteraction.Ignore) &&
                    zaslona.collider.GetComponentInParent<PunktHaka>() != ph) continue;

                if (t.distance < najlepszy)
                {
                    najlepszy = t.distance;
                    punkt = p;
                    obiekt = t.collider.transform;
                    znaleziono = true;
                }
            }
            return znaleziono;
        }

        bool CzyMoznaZaczepic(Collider collider)
        {
            if (!tylkoPunktyHaka) return true;
            return collider.GetComponentInParent<PunktHaka>() != null;
        }

        /// <summary>Zaczepia hak w podanym punkcie obiektu.</summary>
        public void Zaczep(Vector3 punkt, Transform obiekt)
        {
            if (obiekt == null) return;
            _obiekt = obiekt;
            _punktLokalny = obiekt.InverseTransformPoint(punkt);
            _dlugoscLiny = Mathf.Max(minimalnaDlugoscLiny, Vector3.Distance(PunktNaGraczu(), punkt));

            // przejmujemy sterowanie postacią, zaczynając od prędkości, którą miała
            _predkosc = _gracz.Kontroler.Velocity;
            _gracz.Kontroler.ExternalControl = true;

            Zaczepiony = true;
            _lina.enabled = true;
            gdyZaczepiony.Wyslij(transform.position);
        }

        /// <summary>Odczepia hak. Gracz zachowuje aktualną prędkość (to dzięki temu można "przelecieć" dalej).</summary>
        public void Odczep(bool zWybiciem)
        {
            if (!Zaczepiony) return;
            Zaczepiony = false;
            if (_lina != null) _lina.enabled = false;
            gdyPuszczony.Wyslij(transform.position);

            if (zWybiciem) _predkosc.y = Mathf.Max(_predkosc.y, 0f) + wybiciePrzyPuszczeniu;

            if (_gracz != null && _gracz.Kontroler != null)
            {
                _gracz.Kontroler.ExternalControl = false;
                _gracz.Kontroler.SetVelocity(_predkosc);
            }
        }

        Vector3 PunktZaczepienia() { return _obiekt.TransformPoint(_punktLokalny); }

        // miejsce na ciele gracza, do którego "przywiązana" jest lina
        Vector3 PunktNaGraczu() { return transform.position + Vector3.up * (1.2f * _gracz.Rozmiar); }

        void SymulujRuchNaLinie()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            var kontroler = _gracz.Kontroler;
            Vector3 kotwica = PunktZaczepienia();

            // kierunek z klawiszy WSAD względem tego, gdzie patrzy gracz
            Vector2 ruch = _gracz.WejscieRuchu != null ? _gracz.WejscieRuchu.move : Vector2.zero;
            Vector3 kierunek = Vector3.ClampMagnitude(transform.right * ruch.x + transform.forward * ruch.y, 1f);

            if (kontroler.Grounded && _predkosc.y <= 0f)
            {
                // stoimy na ziemi z zaczepioną liną: zwykłe chodzenie
                Vector3 poziom = new Vector3(_predkosc.x, 0f, _predkosc.z);
                poziom = Vector3.MoveTowards(poziom, kierunek * kontroler.MoveSpeed, 40f * dt);
                _predkosc = new Vector3(poziom.x, -2f, poziom.z);
            }
            else
            {
                // wisimy: grawitacja + lekkie sterowanie w powietrzu
                _predkosc.y += kontroler.Gravity * dt;
                _predkosc += kierunek * (sterowanieWPowietrzu * dt);
            }

            // wciąganie = skracanie liny
            Vector3 teraz = PunktNaGraczu();
            if (klawiszWciagania.IsPressed() || WymusWciaganie)
            {
                _dlugoscLiny = Mathf.Max(minimalnaDlugoscLiny, _dlugoscLiny - predkoscWciagania * dt);

                // Im krótsza lina, tym szybciej kręci się wahadło (jak łyżwiarka przyciągająca ręce).
                // Żeby gracz nie "przekręcił się" nad punkt zaczepienia, na ostatnich metrach
                // wciągania stopniowo wytracamy prędkość.
                float blisko = Mathf.InverseLerp(minimalnaDlugoscLiny + 4f, minimalnaDlugoscLiny, _dlugoscLiny);
                _predkosc *= Mathf.Exp(-6f * blisko * dt);

                // lina wciągnięta do końca: zerujemy prędkość w stronę punktu zaczepienia,
                // żeby gracz zawisł pod nim, a nie przeleciał z rozpędu na drugą stronę
                Vector3 doKotwicy = kotwica - teraz;
                if (doKotwicy.magnitude <= minimalnaDlugoscLiny + 0.5f)
                {
                    Vector3 kier = doKotwicy.normalized;
                    float zblizanie = Vector3.Dot(_predkosc, kier);
                    if (zblizanie > 0f) _predkosc -= kier * zblizanie;
                }
            }

            // OGRANICZENIE LINY: jeśli po tym kroku gracz byłby dalej od kotwicy niż długość liny,
            // "ściągamy" go z powrotem na okrąg o promieniu liny. Z tego bierze się ruch wahadłowy.
            Vector3 potem = teraz + _predkosc * dt;
            Vector3 odKotwicy = potem - kotwica;
            if (odKotwicy.magnitude > _dlugoscLiny)
            {
                potem = kotwica + odKotwicy.normalized * _dlugoscLiny;
                _predkosc = (potem - teraz) / dt;
            }

            _predkosc = Vector3.ClampMagnitude(_predkosc, maksymalnaPredkosc);
            _gracz.Kapsula.Move(_predkosc * dt);

            // jeśli wpadliśmy na ścianę/podłogę, bierzemy prędkość faktycznie uzyskaną po kolizji
            Vector3 faktyczna = _gracz.Kapsula.velocity;
            if (faktyczna.sqrMagnitude < _predkosc.sqrMagnitude) _predkosc = faktyczna;
        }
    }
}
