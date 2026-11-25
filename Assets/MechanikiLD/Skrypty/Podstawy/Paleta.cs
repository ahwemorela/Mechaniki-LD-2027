using UnityEngine;
using UnityEngine.InputSystem;

namespace MechanikiLD
{
    /// <summary>
    /// JĘZYK KOLORÓW projektu. W blockoucie kolor to informacja dla gracza:
    /// ten sam kolor zawsze oznacza tę samą funkcję. Wszystkie mechaniki biorą kolory stąd.
    ///
    /// Same wartości kolorów zmienia się BEZ programowania, w pliku "UstawieniaLD"
    /// (menu Mechaniki LD > Ustawienia). Ta klasa tylko je stamtąd odczytuje.
    /// Kolory kluczy są w plikach "Rodzaj klucza".
    /// </summary>
    public static class Paleta
    {
        static UstawieniaLD U => UstawieniaLD.Aktualne;

        // Po znaku "?" jest kolor z pliku ustawień, po ":" - kolor zapasowy, gdyby pliku nie było.
        public static Color Aktywny => U != null ? U.kolorAktywny : new Color(0.25f, 0.9f, 0.35f);          // włączone / otwarte
        public static Color Nieaktywny => U != null ? U.kolorNieaktywny : new Color(0.95f, 0.55f, 0.1f);    // czeka na akcję gracza
        public static Color Niebezpieczny => U != null ? U.kolorNiebezpieczny : new Color(1f, 0.1f, 0.1f);  // zagrożenie
        public static Color Hak => U != null ? U.kolorHaka : new Color(1f, 0.85f, 0.1f);                    // hak
        public static Color Polaczenie => U != null ? U.kolorPolaczen : new Color(1f, 0.8f, 0.2f);          // linie w oknie Scene
        public static Color Trasa => U != null ? U.kolorTras : new Color(0.2f, 0.9f, 0.9f);                 // trasy platform
        public static Color Straznik => U != null ? U.kolorStraznika : new Color(0.7f, 0.3f, 1f);           // strażnicy i kamery

        static MaterialPropertyBlock _blok;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        /// <summary>
        /// Zmienia kolor obiektu bez tworzenia nowego materiału (działa też w edytorze).
        /// "swiecenie" > 0 sprawia, że obiekt dodatkowo świeci tym kolorem.
        /// </summary>
        public static void Pomaluj(Renderer renderer, Color kolor, float swiecenie = 0f)
        {
            if (renderer == null) return;
            if (_blok == null) _blok = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_blok);
            _blok.SetColor(BaseColor, kolor);
            _blok.SetColor(EmissionColor, kolor * swiecenie);
            renderer.SetPropertyBlock(_blok);
        }

        public static void Pomaluj(Renderer[] renderery, Color kolor, float swiecenie = 0f)
        {
            if (renderery == null) return;
            foreach (Renderer r in renderery) Pomaluj(r, kolor, swiecenie);
        }
    }

    /// <summary>Pomocnik do tworzenia klawiszy (akcji wejścia) z kilkoma przypisaniami naraz.</summary>
    public static class Wejscie
    {
        public static InputAction Przycisk(string nazwa, params string[] klawisze)
        {
            var akcja = new InputAction(nazwa, InputActionType.Button);
            foreach (string k in klawisze) akcja.AddBinding(k);
            return akcja;
        }
    }
}
