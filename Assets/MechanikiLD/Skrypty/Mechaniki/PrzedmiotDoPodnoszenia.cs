using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// PRZEDMIOT DO PODNOSZENIA (np. skrzynia).
    /// Obiekt fizyczny, który postać może podnieść (E), przenieść, odłożyć lub rzucić.
    /// Najczęstsze zastosowanie: położenie go na przycisku naciskowym albo zbudowanie "schodka".
    ///
    /// Jeśli przedmiot wypadnie poza mapę albo wpadnie do strefy śmierci, wraca na miejsce startowe -
    /// dzięki temu gracz nie zablokuje sobie zagadki.
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ: ten komponent zadziała na KAŻDYM obiekcie z Rigidbody i colliderem.
    /// Zmień model, masę (Rigidbody > Mass) i tekst podpowiedzi - i masz kulę, baterię, wazon, klocek.
    /// Masa decyduje o tym, kto go udźwignie i który przycisk wciśnie.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Mechaniki LD/Przedmiot Do Podnoszenia")]
    public class PrzedmiotDoPodnoszenia : MonoBehaviour, IInteraktywny
    {
        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        [Tooltip("Podpowiedź pod celownikiem, np. \"Podnieś skrzynię\", \"Weź baterię\".")]
        public string podpowiedz = "Podnieś skrzynię";
        public Powiadomienie poPodniesieniu = new Powiadomienie();
        public Powiadomienie poOdlozeniu = new Powiadomienie();
        public Powiadomienie poRzucie = new Powiadomienie();

        [Header("Powrót na miejsce")]
        [Tooltip("Czy przedmiot ma wracać na miejsce startowe po wypadnięciu poza mapę lub wpadnięciu do strefy śmierci.")]
        public bool wracaNaStart = true;
        [Tooltip("Poniżej tej wysokości (oś Y) przedmiot uznajemy za stracony.")]
        public float minimalnaWysokosc = -30f;

        public Rigidbody Cialo { get; private set; }
        public bool CzyNiesiony => _niosacy != null;

        Vector3 _pozycjaStartowa;
        Quaternion _obrotStartowy;
        Gracz _niosacy;

        void Awake()
        {
            Cialo = GetComponent<Rigidbody>();
            _pozycjaStartowa = transform.position;
            _obrotStartowy = transform.rotation;
        }

        void Update()
        {
            if (transform.position.y < minimalnaWysokosc) WrocNaStart();
        }

        /// <summary>Przenosi przedmiot z powrotem tam, gdzie leżał na początku poziomu.</summary>
        public void WrocNaStart()
        {
            if (!wracaNaStart) return;

            // jeśli ktoś go akurat niesie - najpierw wypada z rąk
            if (_niosacy != null && _niosacy.Podnoszenie != null) _niosacy.Podnoszenie.Upusc();

            Cialo.linearVelocity = Vector3.zero;
            Cialo.angularVelocity = Vector3.zero;
            Cialo.position = _pozycjaStartowa;
            Cialo.rotation = _obrotStartowy;
            transform.SetPositionAndRotation(_pozycjaStartowa, _obrotStartowy);
        }

        public void GdyPodniesiony(Gracz gracz)
        {
            _niosacy = gracz;
            poPodniesieniu.Wyslij(transform.position);
        }

        public void GdyUpuszczony(bool rzut)
        {
            _niosacy = null;
            (rzut ? poRzucie : poOdlozeniu).Wyslij(transform.position);
        }

        // --- interakcja (klawisz E) ---

        public string TekstPodpowiedzi(Gracz gracz) { return podpowiedz; }

        public bool CzyMoznaUzyc(Gracz gracz)
        {
            return gracz.Podnoszenie != null && gracz.Podnoszenie.MozePodniesc(this);
        }

        public void Uzyj(Gracz gracz)
        {
            if (gracz.Podnoszenie != null) gracz.Podnoszenie.Podnies(this);
        }
    }
}
