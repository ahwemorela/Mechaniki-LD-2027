using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MechanikiLD
{
    /// <summary>
    /// META POZIOMU.
    /// Strefa kończąca poziom: pokazuje ekran "Poziom ukończony" z czasem przejścia i liczbą odrodzeń.
    /// Klawisz R uruchamia wtedy poziom od nowa.
    ///
    /// Czas i liczba odrodzeń to gotowe dane do testów: daj poziom do przejścia kilku osobom i porównaj wyniki.
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ: podmień model (portal, winda, drzwi wyjściowe, statek), a wygląd
    /// ekranu końcowego zmień w prefabie HUD (obiekt "Ekran konca"). Zdarzenie "Gdy Ukonczono" pozwala
    /// uruchomić cokolwiek - animację, Timeline, dźwięk.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Meta Poziomu")]
    public class MetaPoziomu : MonoBehaviour, IStrefaGracza
    {
        [Tooltip("Zaznaczone: poziom kończy się dopiero, gdy w strefie mety stoją WSZYSTKIE postacie (mechanika zamiany ciał).\nOdznaczone: wystarczy dowolna postać.")]
        public bool wymagaWszystkichPostaci = false;

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        [Tooltip("Gdy do mety dotarła dopiero część postaci.")]
        public Powiadomienie gdyBrakujePostaci = new Powiadomienie("Do mety muszą dotrzeć wszystkie postacie");
        [Tooltip("W chwili ukończenia poziomu (zwykle sam dźwięk - tekst jest na ekranie końcowym).")]
        public Powiadomienie poUkonczeniu = new Powiadomienie();

        [Tooltip("Zdarzenie wywoływane w chwili ukończenia poziomu (opcjonalne).")]
        public UnityEvent gdyUkonczono;

        readonly List<Gracz> _wStrefie = new List<Gracz>();
        bool _ukonczono;

        public void GraczWszedl(Gracz gracz)
        {
            if (!_wStrefie.Contains(gracz)) _wStrefie.Add(gracz);
            if (_ukonczono) return;

            if (wymagaWszystkichPostaci && _wStrefie.Count < Gracz.Wszyscy.Count)
            {
                if (gracz.CzySterowana) gdyBrakujePostaci.Wyslij(transform.position);
                return;
            }
            Ukoncz();
        }

        public void GraczWStrefie(Gracz gracz) { }
        public void GraczWyszedl(Gracz gracz) { _wStrefie.Remove(gracz); }

        void Ukoncz()
        {
            _ukonczono = true;

            // liczymy odrodzenia wszystkich postaci
            int odrodzenia = 0;
            foreach (Gracz g in Gracz.Wszyscy)
                if (g.Odrodzenie != null) odrodzenia += g.Odrodzenie.LiczbaOdrodzen;

            // sam wygląd ekranu końcowego (teksty, układ) ustawia się w prefabie HUD
            Gracz sterowana = Gracz.Instancja;
            if (sterowana != null && sterowana.HUD != null) sterowana.HUD.PokazKoniecPoziomu(Time.timeSinceLevelLoad, odrodzenia);
            poUkonczeniu.Wyslij(transform.position);

            // zatrzymujemy postacie i pozwalamy zrestartować poziom klawiszem R
            foreach (Gracz g in Gracz.Wszyscy)
            {
                if (g.Hak != null) g.Hak.Odczep(false);
                if (g.Odrodzenie != null) g.Odrodzenie.PoziomUkonczony = true;
                if (g.Kontroler != null) g.Kontroler.enabled = false;
            }

            gdyUkonczono?.Invoke();
        }

        void OnDrawGizmos()
        {
            BoxCollider pudelko = GetComponent<BoxCollider>();
            if (pudelko == null) return;
            Gizmos.color = new Color(Paleta.Aktywny.r, Paleta.Aktywny.g, Paleta.Aktywny.b, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(pudelko.center, pudelko.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
