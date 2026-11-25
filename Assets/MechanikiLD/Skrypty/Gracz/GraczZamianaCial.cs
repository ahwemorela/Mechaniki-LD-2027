using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MechanikiLD
{
    /// <summary>
    /// ZAMIANA CIAŁ.
    /// Na scenie są dwie (lub więcej) postacie. Gracz steruje jedną naraz, a klawiszem Q "przeskakuje"
    /// do następnej. Porzucone ciało zostaje tam, gdzie stało - i nadal działa na świat:
    /// wciska przyciski naciskowe, jeździ na platformach, zasłania widok.
    ///
    /// Kamera nie przeskakuje cięciem, tylko WYLATUJE z jednego ciała i WLATUJE w drugie - jakby gracz
    /// je opętywał. W czasie lotu nikt nie steruje żadną postacią, a świat gry toczy się dalej.
    ///
    /// Z tego biorą się zagadki "na dwie osoby dla jednego gracza": jedno ciało staje na przycisku
    /// i otwiera drzwi drugiemu, potem role się odwracają.
    ///
    /// Jak dodać drugą postać: przeciągnij na scenę prefab "Prefaby/Gracz/DrugieCialo".
    /// (Prefab "Gracz" ma kamerę i interfejs, więc powinien być na scenie tylko jeden.)
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ:
    ///  - "Wymaga Widocznosci" - przeskoczyć można tylko do ciała, które się WIDZI. Wtedy okna, kraty
    ///    i zakręty stają się elementem zagadki (opętanie, przesyłanie świadomości, teleportacja do drona),
    ///  - "Maksymalna Odleglosc" - przeskok działa tylko z bliska (przekazanie pałeczki),
    ///  - "Czas Przelotu" - 0 daje natychmiastowe cięcie (przełączanie kamer), 0.7 szybkie opętanie,
    ///    2-3 sekundy powolny lot ducha, w czasie którego gracz ogląda poziom z góry i jest bezbronny,
    ///  - różne umiejętności ciał: wyłącz hak albo podnoszenie w jednym z nich, zmień mu prędkość lub rozmiar,
    ///  - nazwy i kolory postaci ustawiasz w komponencie Gracz ("Nazwa Postaci", "Kolor Ciala").
    ///
    /// Ustawienia przelotu bierzemy z ciała, Z KTÓREGO gracz wylatuje - każde ciało może mieć inne.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz Zamiana Cial")]
    public class GraczZamianaCial : MonoBehaviour
    {
        [Header("Sterowanie")]
        public InputAction klawiszZamiany = Wejscie.Przycisk("Zamiana", "<Keyboard>/q", "<Gamepad>/buttonNorth");

        [Header("Warunki zamiany")]
        [Tooltip("Zaznaczone: przeskoczyć można tylko do ciała, które widać z oczu aktualnej postaci (nic go nie zasłania).")]
        public bool wymagaWidocznosci = false;
        [Tooltip("Największa odległość do drugiego ciała (metry). 0 = bez ograniczenia.")]
        public float maksymalnaOdleglosc = 0f;
        [Tooltip("Przerwa między kolejnymi zamianami (sekundy), liczona od końca przelotu.")]
        public float czasOdnowienia = 0.4f;

        [Header("Przelot kamery (efekt \"opętania\")")]
        [Tooltip("Ile sekund kamera leci z tego ciała do następnego.\n0 = bez przelotu, natychmiastowe cięcie.\nW czasie lotu gracz nie steruje żadną postacią, a świat się nie zatrzymuje.")]
        [Min(0f)] public float czasPrzelotu = 0.7f;
        [Tooltip("Dodatkowy czas za każdy metr odległości między ciałami (sekundy).\n0 = lot trwa zawsze tyle samo.\nNp. 0.05 = do ciała oddalonego o 20 m leci się o sekundę dłużej - dalekie przeskoki \"kosztują\" więcej czasu.")]
        [Min(0f)] public float czasNaMetr = 0f;
        [Tooltip("Tempo lotu. Oś pozioma = czas (od 0 do 1), oś pionowa = przebyta droga (od 0 do 1).\nDomyślnie: łagodny start i łagodne lądowanie. Linia prosta = stała prędkość.")]
        public AnimationCurve krzywaPrzelotu = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("O ile stopni poszerza się pole widzenia w połowie lotu - daje wrażenie pędu.\n0 = bez tego efektu.")]
        [Range(0f, 60f)] public float poszerzenieWidoku = 20f;
        [Tooltip("Zaznaczone: kamera najpierw obraca się w stronę ciała, do którego leci, a jego kierunek patrzenia przyjmuje dopiero pod koniec.\nOdznaczone: kamera obraca się równo przez cały lot.")]
        public bool patrzNaCelWLocie = true;
        [Tooltip("Dźwięk na początku lotu (np. świst). Puste = cisza.")]
        public AudioClip dzwiekPrzelotu;

        [Header("Komunikaty i dźwięki  ({0} = nazwa postaci; puste = wyłączone)")]
        [Tooltip("Po udanej zamianie, gdy kamera dotrze do nowego ciała. {0} = nazwa postaci, którą gracz teraz steruje.")]
        public Powiadomienie poZamianie = new Powiadomienie("Sterujesz: {0}");
        [Tooltip("Gdy zamiana jest niemożliwa (ciało niewidoczne albo za daleko).")]
        public Powiadomienie gdyNiemozliwa = new Powiadomienie("Nie możesz teraz przejść do drugiego ciała");

        Gracz _gracz;
        static float _czasOstatniejZamiany = -10f;

        // przelot kamery
        Coroutine _przelot;
        Transform _punktLotu;   // pomocniczy, niewidoczny obiekt, za którym kamera podąża w czasie lotu
        float _fovBazowy;

        void Awake() { _gracz = GetComponent<Gracz>(); }
        void OnEnable() { klawiszZamiany.Enable(); }

        void OnDisable()
        {
            klawiszZamiany.Disable();
            // wyłączenie komponentu w trakcie lotu: kończymy lot od razu, żeby kamera nie została "w powietrzu"
            if (_przelot != null)
            {
                StopCoroutine(_przelot);
                ZakonczLot();
            }
        }

        void Update()
        {
            // klawisz czyta tylko postać, którą gracz steruje
            if (!_gracz.CzySterowana) return;
            if (klawiszZamiany.WasPressedThisFrame()) Zamien();
        }

        /// <summary>Próbuje przejść do następnego ciała. Zwraca true, jeśli się udało (zaczął się przelot albo nastąpiło cięcie).</summary>
        public bool Zamien()
        {
            if (Gracz.Wszyscy.Count < 2) return false;
            if (Gracz.TrwaPrzelot) return false;
            if (Time.time - _czasOstatniejZamiany < czasOdnowienia) return false;

            // szukamy następnej postaci na liście (po ostatniej wracamy do pierwszej)
            int start = Gracz.Wszyscy.IndexOf(_gracz);
            for (int i = 1; i < Gracz.Wszyscy.Count; i++)
            {
                Gracz cel = Gracz.Wszyscy[(start + i) % Gracz.Wszyscy.Count];
                if (!CzyMoznaPrzejscDo(cel)) continue;

                _czasOstatniejZamiany = Time.time;

                bool zPrzelotem = czasPrzelotu > 0f && isActiveAndEnabled
                                  && Gracz.KameraGracza != null && _gracz.oczy != null && cel.oczy != null;
                if (zPrzelotem)
                {
                    _przelot = StartCoroutine(Przelot(cel));
                }
                else
                {
                    Gracz.PrzejmijSterowanie(cel);
                    poZamianie.Wyslij(cel.transform.position, cel.nazwaPostaci);
                }
                return true;
            }

            gdyNiemozliwa.Wyslij(transform.position);
            return false;
        }

        /// <summary>Sprawdza warunki zamiany (odległość i widoczność) dla wskazanego ciała.</summary>
        public bool CzyMoznaPrzejscDo(Gracz cel)
        {
            if (cel == null || cel == _gracz || !cel.gameObject.activeInHierarchy) return false;

            if (maksymalnaOdleglosc > 0f && Vector3.Distance(_gracz.Srodek, cel.Srodek) > maksymalnaOdleglosc) return false;

            if (wymagaWidocznosci && _gracz.oczy != null)
            {
                // wystarczy, że widać tułów albo głowę drugiego ciała
                Vector3 oko = _gracz.oczy.position;
                int maska = _gracz.MaskaSwiata;
                bool tulow = !Physics.Linecast(oko, cel.Srodek, maska, QueryTriggerInteraction.Ignore);
                bool glowa = !Physics.Linecast(oko, cel.Glowa, maska, QueryTriggerInteraction.Ignore);
                if (!tulow && !glowa) return false;
            }
            return true;
        }

        // ---------- PRZELOT KAMERY ----------
        // Kamera gracza (Cinemachine) zawsze podąża za jakimś obiektem - normalnie za "oczami" postaci.
        // Na czas lotu podstawiamy jej pomocniczy punkt i przesuwamy go od oczu starego ciała do oczu nowego.
        // Na końcu kamera wraca do podążania za oczami - już nowej postaci.

        IEnumerator Przelot(Gracz cel)
        {
            CinemachineCamera kamera = Gracz.KameraGracza;
            Vector3 startPozycja = _gracz.oczy.position;
            Quaternion startObrot = _gracz.oczy.rotation;
            float czas = czasPrzelotu + czasNaMetr * Vector3.Distance(startPozycja, cel.oczy.position);

            // stare ciało traci sterowanie; nowe dostanie je dopiero po wylądowaniu
            Gracz.RozpocznijPrzelot(cel);
            Dzwiek.Odtworz(dzwiekPrzelotu, startPozycja);

            _punktLotu = new GameObject("Przelot kamery").transform;
            _punktLotu.SetPositionAndRotation(startPozycja, startObrot);
            _fovBazowy = kamera.Lens.FieldOfView;
            kamera.Follow = _punktLotu;

            float t = 0f; // postęp lotu w czasie: 0 = start, 1 = koniec
            while (t < 1f && Gracz.TrwaPrzelot && cel != null && cel.oczy != null && kamera != null)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / czas);
                float droga = Mathf.Clamp01(krzywaPrzelotu.Evaluate(t));

                // cel czytamy co klatkę - drugie ciało może w tym czasie jechać na platformie albo spadać
                Vector3 koniecPozycja = cel.oczy.position;
                Quaternion koniecObrot = cel.oczy.rotation;

                _punktLotu.SetPositionAndRotation(
                    Vector3.Lerp(startPozycja, koniecPozycja, droga),
                    ObrotWLocie(startPozycja, startObrot, koniecPozycja, koniecObrot, t));

                // pole widzenia: najszersze w połowie lotu, na końcu wraca do normy
                UstawPoleWidzenia(kamera, _fovBazowy + poszerzenieWidoku * Mathf.Sin(t * Mathf.PI));

                yield return null;
            }

            ZakonczLot();
        }

        void ZakonczLot()
        {
            _przelot = null;
            bool doleciala = Gracz.TrwaPrzelot; // false = ktoś w międzyczasie przełączył postać cięciem

            CinemachineCamera kamera = Gracz.KameraGracza;
            if (kamera != null && _fovBazowy > 0f) UstawPoleWidzenia(kamera, _fovBazowy);

            Gracz.ZakonczPrzelot(); // nowa postać dostaje klawisze, a kamera wraca do podążania za jej oczami
            if (_punktLotu != null) Destroy(_punktLotu.gameObject);

            _czasOstatniejZamiany = Time.time;
            Gracz nowa = Gracz.Instancja;
            if (doleciala && nowa != null) poZamianie.Wyslij(nowa.transform.position, nowa.nazwaPostaci);
        }

        // Kierunek patrzenia kamery w danym momencie lotu (t od 0 do 1).
        // Liczymy osobno obrót w poziomie i pochylenie - dzięki temu kamera nigdy nie przechyla się na bok.
        Quaternion ObrotWLocie(Vector3 startPozycja, Quaternion startObrot, Vector3 koniecPozycja, Quaternion koniecObrot, float t)
        {
            Katy(startObrot * Vector3.forward, out float poziom0, out float pion0);
            Katy(koniecObrot * Vector3.forward, out float poziom1, out float pion1);

            float poziom, pion;
            Vector3 doCelu = koniecPozycja - startPozycja;
            if (patrzNaCelWLocie && doCelu.sqrMagnitude > 1f)
            {
                // 1) obrót w stronę celu, 2) lot z widokiem na cel, 3) przyjęcie spojrzenia nowego ciała
                Katy(doCelu, out float poziomCelu, out float pionCelu);
                float naCel = Mathf.SmoothStep(0f, 1f, t / 0.35f);
                float naKoniec = Mathf.SmoothStep(0f, 1f, (t - 0.55f) / 0.45f);
                poziom = Mathf.LerpAngle(Mathf.LerpAngle(poziom0, poziomCelu, naCel), poziom1, naKoniec);
                pion = Mathf.Lerp(Mathf.Lerp(pion0, pionCelu, naCel), pion1, naKoniec);
            }
            else
            {
                float w = Mathf.SmoothStep(0f, 1f, t);
                poziom = Mathf.LerpAngle(poziom0, poziom1, w);
                pion = Mathf.Lerp(pion0, pion1, w);
            }
            return Quaternion.Euler(pion, poziom, 0f);
        }

        // Zamienia kierunek na dwa kąty: obrót w poziomie (lewo-prawo) i pochylenie (góra-dół).
        static void Katy(Vector3 kierunek, out float poziom, out float pion)
        {
            kierunek.Normalize();
            poziom = Mathf.Atan2(kierunek.x, kierunek.z) * Mathf.Rad2Deg;
            pion = -Mathf.Asin(Mathf.Clamp(kierunek.y, -1f, 1f)) * Mathf.Rad2Deg;
        }

        static void UstawPoleWidzenia(CinemachineCamera kamera, float stopnie)
        {
            LensSettings obiektyw = kamera.Lens;
            obiektyw.FieldOfView = Mathf.Clamp(stopnie, 1f, 170f);
            kamera.Lens = obiektyw;
        }
    }
}
