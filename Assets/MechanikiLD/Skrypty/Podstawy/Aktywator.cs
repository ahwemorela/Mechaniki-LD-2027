using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MechanikiLD
{
    /// <summary>
    /// NADAJNIK SYGNAŁU.
    /// Klasa bazowa dla wszystkiego, co coś URUCHAMIA: przycisk naciskowy, przełącznik,
    /// zebrany klucz, strażnik który zauważył gracza...
    ///
    /// Na liście "Cele" wskazujesz obiekty, które mają zareagować (drzwi, platforma, winda).
    /// W oknie Scene od aktywatora do każdego celu rysowana jest linia - od razu widać, co jest z czym połączone.
    /// </summary>
    public abstract class Aktywator : MonoBehaviour
    {
        [Header("Co ten obiekt uruchamia")]
        [Tooltip("Przeciągnij tu obiekty, które mają zareagować: drzwi, ruchomą platformę, windę, most...\nMożna podać kilka.")]
        public List<ObiektAktywowalny> cele = new List<ObiektAktywowalny>();

        [Header("Zdarzenia (opcjonalne, dla chętnych)")]
        [Tooltip("Wywoływane, gdy aktywator się włącza. Można tu podpiąć dowolną funkcję dowolnego obiektu.")]
        public UnityEvent gdyWlaczony;
        [Tooltip("Wywoływane, gdy aktywator się wyłącza.")]
        public UnityEvent gdyWylaczony;

        /// <summary>Czy aktywator jest teraz włączony (przycisk wciśnięty, dźwignia przestawiona...).</summary>
        public bool CzyWlaczony { get; private set; }

        /// <summary>Włącza lub wyłącza aktywator i przekazuje sygnał do wszystkich celów.</summary>
        protected void UstawStan(bool wlaczony)
        {
            if (wlaczony == CzyWlaczony) return;
            CzyWlaczony = wlaczony;

            foreach (ObiektAktywowalny cel in cele)
            {
                if (cel == null) continue;
                if (wlaczony) cel.Aktywuj(); else cel.Dezaktywuj();
            }

            if (wlaczony) gdyWlaczony?.Invoke(); else gdyWylaczony?.Invoke();
            GdyZmianaStanu(wlaczony);
        }

        /// <summary>Miejsce na reakcję wizualną (zmiana koloru, wciśnięcie płytki itd.).</summary>
        protected virtual void GdyZmianaStanu(bool wlaczony) { }

        // Linie w oknie Scene łączące aktywator z celami - bardzo pomaga przy projektowaniu zagadek.
        protected virtual void OnDrawGizmos()
        {
            if (cele == null) return;
            Gizmos.color = (Application.isPlaying && CzyWlaczony) ? Paleta.Aktywny : Paleta.Polaczenie;
            foreach (ObiektAktywowalny cel in cele)
            {
                if (cel == null) continue;
                Vector3 a = transform.position + Vector3.up * 0.3f;
                Vector3 b = cel.transform.position + Vector3.up * 0.3f;
                Gizmos.DrawLine(a, b);
                Gizmos.DrawSphere(b, 0.12f);
            }
        }
    }
}
