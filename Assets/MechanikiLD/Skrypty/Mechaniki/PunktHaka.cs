using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// PUNKT HAKA.
    /// Znacznik: "o ten obiekt wolno zaczepić hak". Wystarczy dodać ten komponent do dowolnego obiektu
    /// z colliderem (kula pod sufitem, belka, panel na ścianie, a nawet ruchoma platforma).
    ///
    /// Dzięki temu to PROJEKTANT decyduje, gdzie gracz może się zaczepić - a więc jakimi drogami
    /// da się przejść poziom. W tym projekcie punkty haka są zawsze ŻÓŁTE.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Punkt Haka")]
    public class PunktHaka : MonoBehaviour
    {
        [Tooltip("Zaznaczone: lina zawsze zaczepia się w ŚRODKU obiektu (dobre dla małych kul).\nOdznaczone: lina zaczepia się dokładnie tam, gdzie celował gracz (dobre dla belek, sufitów, ścian).")]
        public bool przyciagajDoSrodka = true;

        [Header("Podgląd w oknie Scene")]
        [Tooltip("Promień sfery pokazującej, z jak daleka gracz dosięgnie tego punktu. Ustaw tyle, ile wynosi \"Zasieg\" w komponencie Gracz Hak (domyślnie 22 m).")]
        public float podgladZasiegu = 22f;

        void OnDrawGizmos()
        {
            Gizmos.color = Paleta.Hak;
            Gizmos.DrawWireSphere(transform.position, 0.35f);
        }

        // Po zaznaczeniu punktu widać sferę zasięgu haka - gracz musi znaleźć się WEWNĄTRZ niej, żeby się zaczepić.
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(Paleta.Hak.r, Paleta.Hak.g, Paleta.Hak.b, 0.35f);
            Gizmos.DrawWireSphere(transform.position, podgladZasiegu);
        }
    }
}
