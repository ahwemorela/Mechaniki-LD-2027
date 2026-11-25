using System.Collections.Generic;
using UnityEngine;

namespace MechanikiLD
{
    public enum TrybTrasy
    {
        [InspectorName("Tam I Z Powrotem")]
        [Tooltip("A → B → C → B → A → ... (wahadłowo)")]
        TamIZPowrotem,
        [InspectorName("W Pętli")]
        [Tooltip("A → B → C → A → B → ... (w kółko)")]
        WPetli,
        [InspectorName("Na Sygnał Do Końca")]
        [Tooltip("Jest sygnał: jedzie do ostatniego punktu i tam czeka. Brak sygnału: wraca do pierwszego. (winda, wysuwany most)")]
        NaSygnalDoKonca
    }

    /// <summary>
    /// RUCH PO TRASIE - ruchoma platforma, winda, wysuwany most, a także patrolujący strażnik.
    /// Przesuwa wskazany obiekt ("Obiekt Ruchomy") między punktami trasy.
    ///
    /// Budowa prefabu:  [korzeń z tym skryptem]
    ///                     ├─ Platforma   <- to się rusza
    ///                     ├─ Punkt A     <- puste obiekty wyznaczające trasę;
    ///                     └─ Punkt B        przesuwasz je w oknie Scene i od razu widzisz tor ruchu
    ///
    /// Sygnały (przycisk, przełącznik): domyślnie platforma jeździ cały czas ("Aktywny Bez Sygnalu").
    /// Odznacz to pole, a ruszy dopiero po otrzymaniu sygnału. Tryb "Na Sygnał Do Końca" zamienia ją w windę/most.
    ///
    /// Gracz stojący na platformie jedzie razem z nią (dba o to FirstPersonController).
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Ruch Po Trasie (platforma, strażnik)")]
    public class RuchPoTrasie : ObiektAktywowalny
    {
        [Header("Co i którędy")]
        [Tooltip("Obiekt, który ma się poruszać (platforma, strażnik). Na starcie zostanie ustawiony w pierwszym punkcie trasy.")]
        public Transform obiektRuchomy;
        [Tooltip("Punkty trasy w kolejności odwiedzania. Potrzebne są co najmniej dwa.")]
        public List<Transform> punkty = new List<Transform>();
        public TrybTrasy tryb = TrybTrasy.TamIZPowrotem;

        [Header("Tempo")]
        [Tooltip("Prędkość ruchu (m/s). Dla porównania: gracz idzie 4 m/s, biegnie 6 m/s.")]
        [Min(0.01f)] public float predkosc = 3f;
        [Tooltip("Postój na końcach trasy (sekundy). Daje graczowi czas na wejście i zejście.")]
        public float czasPostoju = 1.5f;
        [Tooltip("Łagodne ruszanie i hamowanie przy punktach trasy.")]
        public bool plynnyRuch = true;

        [Header("Obracanie (dla strażników)")]
        [Tooltip("Zaznaczone: obiekt obraca się przodem do kierunku jazdy (strażnik patrzy tam, dokąd idzie).")]
        public bool obracajWKierunkuRuchu = false;
        [Tooltip("Prędkość obracania (stopnie na sekundę).")]
        public float predkoscObrotu = 180f;

        Rigidbody _cialo;
        readonly List<Vector3> _pozycje = new List<Vector3>();
        int _od, _do = 1;
        float _postoj;

        void Awake()
        {
            if (obiektRuchomy != null) _cialo = obiektRuchomy.GetComponent<Rigidbody>();

            // zapamiętujemy pozycje punktów z początku gry
            foreach (Transform p in punkty)
                if (p != null) _pozycje.Add(p.position);

            if (obiektRuchomy != null && _pozycje.Count > 0)
            {
                obiektRuchomy.position = _pozycje[0];
                if (_cialo != null) _cialo.position = _pozycje[0];
            }
        }

        protected override void GdyZmianaStanu(bool aktywny, bool natychmiast) { }

        void FixedUpdate()
        {
            if (obiektRuchomy == null || _pozycje.Count < 2) return;

            float dt = Time.fixedDeltaTime;
            int ostatni = _pozycje.Count - 1;

            if (tryb == TrybTrasy.NaSygnalDoKonca)
            {
                // sygnał decyduje, w którą stronę trasy jedziemy
                int kierunek = CzyAktywny ? 1 : -1;
                if ((_do - _od) * kierunek < 0) { int t = _od; _od = _do; _do = t; _postoj = 0f; }
                if (_od == _do)
                {
                    int nastepny = Mathf.Clamp(_do + kierunek, 0, ostatni);
                    if (nastepny == _do) return; // stoimy na końcu trasy
                    _do = nastepny;
                }
            }
            else if (!CzyAktywny)
            {
                return; // brak sygnału = postój w miejscu
            }

            if (_postoj > 0f) { _postoj -= dt; return; }

            Vector3 pozycja = _cialo != null ? _cialo.position : obiektRuchomy.position;
            Vector3 cel = _pozycje[_do];

            // łagodne ruszanie/hamowanie: zwalniamy w pobliżu punktu, z którego ruszamy i do którego dojeżdżamy
            float v = predkosc;
            if (plynnyRuch)
            {
                float doCelu = Vector3.Distance(pozycja, cel);
                float odStartu = Vector3.Distance(pozycja, _pozycje[_od]);
                float najblizej = Mathf.Min(CzyPostojW(_do) ? doCelu : float.MaxValue, CzyPostojW(_od) ? odStartu : float.MaxValue);
                v *= Mathf.Clamp(najblizej / 1.2f + 0.2f, 0.2f, 1f);
            }

            Vector3 nowa = Vector3.MoveTowards(pozycja, cel, v * dt);
            Przesun(nowa, cel - pozycja, dt);

            if ((nowa - cel).sqrMagnitude < 0.000001f) DotarlDoPunktu(ostatni);
        }

        // czy w danym punkcie obiekt się zatrzymuje (od tego zależy hamowanie)
        bool CzyPostojW(int indeks)
        {
            if (tryb == TrybTrasy.WPetli) return czasPostoju > 0f;
            return indeks == 0 || indeks == _pozycje.Count - 1;
        }

        void DotarlDoPunktu(int ostatni)
        {
            int kierunek = _do >= _od ? 1 : -1;
            _od = _do;

            switch (tryb)
            {
                case TrybTrasy.TamIZPowrotem:
                    if (_do == ostatni) { kierunek = -1; _postoj = czasPostoju; }
                    else if (_do == 0) { kierunek = 1; _postoj = czasPostoju; }
                    _do = Mathf.Clamp(_do + kierunek, 0, ostatni);
                    break;

                case TrybTrasy.WPetli:
                    _do = (_do + 1) % _pozycje.Count;
                    _postoj = czasPostoju;
                    break;

                case TrybTrasy.NaSygnalDoKonca:
                    // następny cel wybierze FixedUpdate na podstawie sygnału
                    break;
            }
        }

        void Przesun(Vector3 pozycja, Vector3 kierunekRuchu, float dt)
        {
            Quaternion obrot = _cialo != null ? _cialo.rotation : obiektRuchomy.rotation;
            if (obracajWKierunkuRuchu)
            {
                kierunekRuchu.y = 0f;
                if (kierunekRuchu.sqrMagnitude > 0.0001f)
                    obrot = Quaternion.RotateTowards(obrot, Quaternion.LookRotation(kierunekRuchu), predkoscObrotu * dt);
            }

            if (_cialo != null)
            {
                // ruch przez Rigidbody: skrzynie leżące na platformie jadą razem z nią
                _cialo.MovePosition(pozycja);
                if (obracajWKierunkuRuchu) _cialo.MoveRotation(obrot);
            }
            else
            {
                obiektRuchomy.SetPositionAndRotation(pozycja, obrot);
            }
        }

        // --- podgląd trasy w oknie Scene ---

        void OnDrawGizmos()
        {
            if (punkty == null || punkty.Count == 0) return;

            Gizmos.color = Paleta.Trasa;
            Vector3 rozmiar = Vector3.one * 0.3f;
            Vector3 przesuniecieBryly = Vector3.zero;
            if (obiektRuchomy != null)
            {
                Renderer r = obiektRuchomy.GetComponentInChildren<Renderer>();
                if (r != null) { rozmiar = r.bounds.size; przesuniecieBryly = r.bounds.center - obiektRuchomy.position; }
            }

            Vector3? poprzedni = null;
            Vector3? pierwszy = null;
            foreach (Transform p in punkty)
            {
                if (p == null) continue;
                Vector3 poz = Application.isPlaying && _pozycje.Count == punkty.Count ? _pozycje[punkty.IndexOf(p)] : p.position;
                Gizmos.DrawWireCube(poz + przesuniecieBryly, rozmiar);
                Gizmos.DrawSphere(poz, 0.15f);
                if (poprzedni.HasValue) Gizmos.DrawLine(poprzedni.Value, poz);
                if (!pierwszy.HasValue) pierwszy = poz;
                poprzedni = poz;
            }
            if (tryb == TrybTrasy.WPetli && poprzedni.HasValue && pierwszy.HasValue)
                Gizmos.DrawLine(poprzedni.Value, pierwszy.Value);
        }
    }
}
