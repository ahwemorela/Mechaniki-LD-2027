using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// PUNKT KONTROLNY (checkpoint).
    /// Gdy postać przez niego przejdzie, od tej pory odradza się właśnie tutaj.
    /// (Przy kilku postaciach każda pamięta własny punkt kontrolny.)
    ///
    /// Zasada: stawiaj punkt kontrolny PRZED każdym trudnym fragmentem i PO każdym zaliczonym wyzwaniu.
    /// Powtarzanie tego, co gracz już umie, jest karą - a nie wyzwaniem.
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ: podmień model (ognisko, terminal, ołtarz, łóżko), zmień lub wyłącz
    /// komunikat. Samą strefę (Box Collider) możesz rozciągnąć na całą szerokość korytarza,
    /// żeby gracz nie mógł jej ominąć.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Punkt Kontrolny")]
    public class PunktKontrolny : MonoBehaviour, IStrefaGracza
    {
        [Tooltip("Miejsce i kierunek, w którym pojawi się postać. Niebieska strzałka tego obiektu = kierunek patrzenia. Puste = środek punktu kontrolnego.")]
        public Transform miejsceOdrodzenia;

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        public Powiadomienie poAktywacji = new Powiadomienie("Punkt kontrolny");

        [Header("Wygląd")]
        [Tooltip("Elementy zmieniające kolor: pomarańczowy = nieaktywny, zielony = tutaj odrodzi się gracz.\nPuste = punkt nie zmienia koloru.")]
        public Renderer[] kolorowane;

        static PunktKontrolny _aktualny;

        void Awake() { OdswiezWyglad(); }
        void OnValidate() { OdswiezWyglad(); }
        void OnDestroy() { if (_aktualny == this) _aktualny = null; }

        public void GraczWszedl(Gracz gracz)
        {
            if (gracz.Odrodzenie == null) return;

            Transform m = miejsceOdrodzenia != null ? miejsceOdrodzenia : transform;
            gracz.Odrodzenie.UstawPunktKontrolny(m.position, m.eulerAngles.y);

            // wygląd i komunikat zmieniamy tylko dla postaci, którą gracz steruje
            if (!gracz.CzySterowana || _aktualny == this) return;

            PunktKontrolny poprzedni = _aktualny;
            _aktualny = this;
            if (poprzedni != null) poprzedni.OdswiezWyglad();
            OdswiezWyglad();
            poAktywacji.Wyslij(transform.position);
        }

        public void GraczWStrefie(Gracz gracz) { }
        public void GraczWyszedl(Gracz gracz) { }

        void OdswiezWyglad()
        {
            bool aktywny = Application.isPlaying && _aktualny == this;
            Paleta.Pomaluj(kolorowane, aktywny ? Paleta.Aktywny : Paleta.Nieaktywny, aktywny ? 1.2f : 0.3f);
        }

        void OnDrawGizmos()
        {
            Transform m = miejsceOdrodzenia != null ? miejsceOdrodzenia : transform;
            Gizmos.color = Paleta.Aktywny;
            Gizmos.DrawWireSphere(m.position + Vector3.up * 0.2f, 0.25f);
            Gizmos.DrawRay(m.position + Vector3.up * 0.2f, m.forward * 1.2f);
        }
    }
}
