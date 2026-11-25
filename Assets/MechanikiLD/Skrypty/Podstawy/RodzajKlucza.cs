using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// RODZAJ KLUCZA - mały plik opisujący jeden rodzaj klucza: jego nazwę, kolor i ikonę.
    /// Klucz i drzwi pasują do siebie, gdy wskazują TEN SAM plik.
    ///
    /// Gotowe rodzaje (czerwony, niebieski, zielony, żółty) leżą w "Ustawienia/Klucze".
    ///
    /// WŁASNY RODZAJ KLUCZA (bez programowania):
    ///  1) w oknie Project kliknij prawym przyciskiem > Create > Mechaniki LD > Rodzaj klucza,
    ///  2) nazwij plik i ustaw mu nazwę, kolor i ewentualnie ikonę,
    ///  3) przeciągnij go na pole "Rodzaj" klucza i na pole "Wymagany Klucz" drzwi.
    ///
    /// "Klucz" nie musi być kluczem: to może być karta dostępu, bateria, kryształ, kod, bilet...
    /// Zmień nazwę tutaj, model w prefabie Klucz i teksty komunikatów - mechanika zostaje ta sama.
    /// </summary>
    [CreateAssetMenu(menuName = "Mechaniki LD/Rodzaj klucza", fileName = "Klucz_Nowy")]
    public class RodzajKlucza : ScriptableObject
    {
        [Tooltip("Nazwa pokazywana graczowi w komunikatach w miejscu {0}, np. \"czerwony\", \"karta ochrony\", \"bateria\".")]
        public string nazwa = "nowy";

        [Tooltip("Kolor klucza, pasa i lampek na drzwiach oraz ikony w interfejsie.")]
        public Color kolor = Color.white;

        [Tooltip("Ikona w interfejsie (opcjonalna). Puste = domyślna ikona klucza z prefabu HUD.")]
        public Sprite ikona;

        [Tooltip("Czy ikonę barwić kolorem klucza. Odznacz, jeśli ikona jest już kolorowym obrazkiem.")]
        public bool barwIkone = true;

#if UNITY_EDITOR
        // Zmiana koloru w tym pliku od razu przemalowuje klucze i drzwi na otwartej scenie.
        void OnValidate() { UstawieniaLD.OdswiezSceneWEdytorze(); }
#endif
    }
}
