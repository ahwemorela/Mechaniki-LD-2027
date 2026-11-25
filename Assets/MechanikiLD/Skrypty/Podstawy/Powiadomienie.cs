using System;
using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// POWIADOMIENIE - komunikat na ekranie i/lub dźwięk towarzyszący jakiemuś zdarzeniu
    /// (zebranie klucza, otwarcie drzwi, wciśnięcie przycisku...).
    ///
    /// Każda mechanika ma w Inspektorze sekcję "Komunikaty i dźwięki" złożoną z takich powiadomień.
    /// Zasady są zawsze te same:
    ///  - pole "Tekst" PUSTE  = żadnego komunikatu,
    ///  - pole "Dzwiek" PUSTE = cisza,
    ///  - {0} w tekście jest zastępowane nazwą (klucza, postaci itp.) - możesz je pominąć.
    ///
    /// Dzięki temu każdy obiekt na scenie może mówić coś innego - albo nic.
    /// Żeby zmienić tekst we WSZYSTKICH obiektach danego typu naraz, zmień go w prefabie.
    /// </summary>
    [Serializable]
    public class Powiadomienie
    {
        [Tooltip("Tekst pokazywany graczowi. PUSTE = brak komunikatu.\n{0} zostanie zastąpione nazwą (np. klucza).")]
        [TextArea(1, 3)] public string tekst = "";

        [Tooltip("Dźwięk odtwarzany przy tym zdarzeniu. PUSTE = cisza.")]
        public AudioClip dzwiek;

        public Powiadomienie() { }
        public Powiadomienie(string tekst) { this.tekst = tekst; }

        /// <summary>Pokazuje komunikat (jeśli jest) i odtwarza dźwięk (jeśli jest) w podanym miejscu.</summary>
        public void Wyslij(Vector3 pozycja, object nazwa = null)
        {
            string gotowy = Tekst(nazwa);
            if (!string.IsNullOrEmpty(gotowy)) Gracz.Komunikat(gotowy);
            Dzwiek.Odtworz(dzwiek, pozycja);
        }

        /// <summary>Zwraca tekst z wstawioną nazwą (pusty, jeśli komunikat jest wyłączony).</summary>
        public string Tekst(object nazwa = null)
        {
            return Wstaw(tekst, nazwa);
        }

        /// <summary>Wstawia nazwę w miejsce {0}. Działa też dla tekstów bez {0}.</summary>
        public static string Wstaw(string wzor, object nazwa)
        {
            if (string.IsNullOrEmpty(wzor)) return "";
            if (nazwa == null || !wzor.Contains("{0")) return wzor;
            try { return string.Format(wzor, nazwa); }
            catch (FormatException) { return wzor; } // np. samotny nawias klamrowy w tekście
        }
    }

    /// <summary>Pomocnik do odtwarzania dźwięków. Pusty dźwięk = nic się nie dzieje.</summary>
    public static class Dzwiek
    {
        /// <summary>Odtwarza dźwięk w konkretnym miejscu świata (z bliska słychać go głośniej).</summary>
        public static void Odtworz(AudioClip klip, Vector3 pozycja)
        {
            if (klip == null || !Application.isPlaying) return;
            float glosnosc = UstawieniaLD.Aktualne != null ? UstawieniaLD.Aktualne.glosnoscDzwiekow : 0.8f;
            AudioSource.PlayClipAtPoint(klip, pozycja, glosnosc);
        }
    }
}
