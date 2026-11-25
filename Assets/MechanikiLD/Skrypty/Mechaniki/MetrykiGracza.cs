using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// METRYKI GRACZA - "miarka" projektanta poziomów, widoczna tylko w oknie Scene.
    /// Rysuje sylwetkę gracza oraz to, jak wysoko i jak daleko potrafi skoczyć (idąc i biegnąc).
    /// Postaw ją przy przepaści albo półce i od razu widać, czy skok jest możliwy.
    ///
    /// Wartości domyślne odpowiadają ustawieniom FirstPersonController w prefabie Gracz.
    /// Jeśli zmienisz prędkość, wysokość skoku albo grawitację gracza - zmień je także tutaj.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Metryki Gracza (miarka)")]
    public class MetrykiGracza : MonoBehaviour
    {
        [Header("Wymiary postaci")]
        public float wysokosc = 2f;
        public float promien = 0.5f;
        [Tooltip("Najwyższy stopień, na który postać wejdzie bez skoku (Step Offset w Character Controller).")]
        public float wysokoscStopnia = 0.25f;

        [Header("Ruch (jak w FirstPersonController)")]
        public float predkoscChodu = 4f;
        public float predkoscBiegu = 6f;
        public float wysokoscSkoku = 1.2f;
        public float grawitacja = 15f;

        [Header("Hak (jak w Gracz Hak)")]
        public bool pokazZasiegHaka = false;
        public float zasiegHaka = 22f;

        /// <summary>Czas lotu przy skoku na tę samą wysokość.</summary>
        public float CzasSkoku => 2f * Mathf.Sqrt(2f * wysokoscSkoku / grawitacja);
        public float DlugoscSkokuChod => predkoscChodu * CzasSkoku;
        public float DlugoscSkokuBieg => predkoscBiegu * CzasSkoku;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Vector3 p = transform.position;
            Vector3 przod = transform.forward;

            // sylwetka
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(p + Vector3.up * promien, promien);
            Gizmos.DrawWireSphere(p + Vector3.up * (wysokosc - promien), promien);
            Gizmos.DrawLine(p + transform.right * promien, p + transform.right * promien + Vector3.up * wysokosc);
            Gizmos.DrawLine(p - transform.right * promien, p - transform.right * promien + Vector3.up * wysokosc);

            // łuki skoku: chód (zielony) i bieg (żółty)
            RysujLuk(p, przod, predkoscChodu, Paleta.Aktywny);
            RysujLuk(p, przod, predkoscBiegu, Paleta.Hak);

            // wysokość skoku i stopnia
            Gizmos.color = Paleta.Trasa;
            Gizmos.DrawLine(p - transform.right * 0.8f + Vector3.up * wysokoscSkoku, p + transform.right * 0.8f + Vector3.up * wysokoscSkoku);

            var styl = UnityEditor.EditorStyles.whiteMiniLabel;
            UnityEditor.Handles.Label(p + Vector3.up * (wysokosc + 0.3f), "gracz: " + wysokosc.ToString("0.0") + " m wys., " + (promien * 2f).ToString("0.0") + " m szer.", styl);
            UnityEditor.Handles.Label(p + transform.right * 0.9f + Vector3.up * wysokoscSkoku, "skok w górę: " + wysokoscSkoku.ToString("0.0") + " m  (stopień bez skoku: " + wysokoscStopnia.ToString("0.00") + " m)", styl);
            UnityEditor.Handles.Label(p + przod * DlugoscSkokuChod + Vector3.up * 0.2f, "skok z chodu: " + DlugoscSkokuChod.ToString("0.0") + " m", styl);
            UnityEditor.Handles.Label(p + przod * DlugoscSkokuBieg + Vector3.up * 0.2f, "skok z biegu: " + DlugoscSkokuBieg.ToString("0.0") + " m", styl);

            if (pokazZasiegHaka)
            {
                Gizmos.color = new Color(Paleta.Hak.r, Paleta.Hak.g, Paleta.Hak.b, 0.4f);
                Gizmos.DrawWireSphere(p + Vector3.up * 1.4f, zasiegHaka);
            }
        }

        void RysujLuk(Vector3 start, Vector3 przod, float predkosc, Color kolor)
        {
            Gizmos.color = kolor;
            float v0 = Mathf.Sqrt(2f * grawitacja * wysokoscSkoku);
            float czas = CzasSkoku;
            Vector3 poprzedni = start;
            for (int i = 1; i <= 20; i++)
            {
                float t = czas * i / 20f;
                Vector3 punkt = start + przod * (predkosc * t) + Vector3.up * (v0 * t - 0.5f * grawitacja * t * t);
                Gizmos.DrawLine(poprzedni, punkt);
                poprzedni = punkt;
            }
            Gizmos.DrawWireSphere(poprzedni, 0.12f);
        }
#endif
    }
}
