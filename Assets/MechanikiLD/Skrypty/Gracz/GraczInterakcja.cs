using UnityEngine;
using UnityEngine.InputSystem;

namespace MechanikiLD
{
    /// <summary>
    /// INTERAKCJA (klawisz E).
    /// Co klatkę wypuszcza z oczu gracza promień. Jeśli trafi w obiekt, którego da się użyć
    /// (klucz, skrzynia, przełącznik, drzwi), na ekranie pojawia się podpowiedź, a klawisz E go używa.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz Interakcja")]
    public class GraczInterakcja : MonoBehaviour
    {
        [Tooltip("Z jak daleka (w metrach) gracz może używać obiektów.")]
        public float zasieg = 3f;

        [Tooltip("Grubość promienia celowania. Większa wartość = łatwiej trafić w małe obiekty.")]
        public float tolerancjaCelowania = 0.2f;

        [Tooltip("Klawisz interakcji.")]
        public InputAction klawiszInterakcji = Wejscie.Przycisk("Interakcja", "<Keyboard>/e", "<Gamepad>/buttonWest");

        /// <summary>Tekst do pokazania pod celownikiem (pusty, gdy gracz na nic nie patrzy).</summary>
        public string Podpowiedz { get; private set; } = "";

        Gracz _gracz;
        IInteraktywny _cel;

        void Awake() { _gracz = GetComponent<Gracz>(); }
        void OnEnable() { klawiszInterakcji.Enable(); }
        void OnDisable() { klawiszInterakcji.Disable(); Podpowiedz = ""; }

        void Update()
        {
            // postać, którą gracz akurat nie steruje, na nic nie patrzy i niczego nie używa
            if (!_gracz.CzySterowana) { _cel = null; Podpowiedz = ""; return; }

            bool wcisnieto = klawiszInterakcji.WasPressedThisFrame();

            // Gdy gracz coś niesie, E służy do odłożenia przedmiotu.
            if (_gracz.Podnoszenie != null && _gracz.Podnoszenie.Trzymany != null)
            {
                _cel = null;
                Podpowiedz = _gracz.Podnoszenie.podpowiedzOdlozenia;
                if (wcisnieto) _gracz.Podnoszenie.Upusc();
                return;
            }

            _cel = ZnajdzCel();
            Podpowiedz = _cel != null ? _cel.TekstPodpowiedzi(_gracz) : "";

            if (wcisnieto && _cel != null) _cel.Uzyj(_gracz);
        }

        IInteraktywny ZnajdzCel()
        {
            if (_gracz.oczy == null) return null;
            Vector3 start = _gracz.oczy.position;
            Vector3 kierunek = _gracz.oczy.forward;
            int maska = _gracz.MaskaSwiata;

            // 1) dokładny promień
            if (Physics.Raycast(start, kierunek, out RaycastHit trafienie, zasieg, maska, QueryTriggerInteraction.Ignore))
            {
                IInteraktywny obiekt = trafienie.collider.GetComponentInParent<IInteraktywny>();
                if (obiekt != null && obiekt.CzyMoznaUzyc(_gracz)) return obiekt;
            }

            // 2) "gruby" promień - wybacza niedokładne celowanie
            if (tolerancjaCelowania > 0f &&
                Physics.SphereCast(start, tolerancjaCelowania, kierunek, out trafienie, zasieg, maska, QueryTriggerInteraction.Ignore))
            {
                IInteraktywny obiekt = trafienie.collider.GetComponentInParent<IInteraktywny>();
                if (obiekt != null && obiekt.CzyMoznaUzyc(_gracz)) return obiekt;
            }

            return null;
        }
    }
}
