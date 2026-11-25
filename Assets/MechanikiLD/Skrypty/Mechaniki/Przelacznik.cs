using UnityEngine;

namespace MechanikiLD
{
    public enum TrybPrzelacznika
    {
        [InspectorName("Przełączany")]
        [Tooltip("Każde użycie zmienia stan: włączony / wyłączony (dźwignia).")]
        Przelaczany,
        [Tooltip("Po użyciu działa przez określony czas, potem sam się wyłącza (przycisk czasowy).")]
        Czasowy,
        [Tooltip("Można użyć tylko raz - zostaje włączony na stałe.")]
        Jednorazowy
    }

    /// <summary>
    /// PRZEŁĄCZNIK (dźwignia / przycisk na ścianie).
    /// Gracz podchodzi, patrzy na niego i wciska E. Przełącznik wysyła sygnał do obiektów z listy "Cele".
    ///
    /// Trzy tryby:
    ///  - Przełączany - działa jak włącznik światła,
    ///  - Czasowy     - włącza cele na kilka sekund (na ekranie widać odliczanie). Klasyczna zagadka
    ///                  "wciśnij i zdąż dobiec do drzwi" - długość trasy i czas ustalasz Ty,
    ///  - Jednorazowy - raz użyty zostaje włączony.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Przełącznik")]
    public class Przelacznik : Aktywator, IInteraktywny
    {
        [Header("Przełącznik")]
        public TrybPrzelacznika tryb = TrybPrzelacznika.Przelaczany;
        [Tooltip("Ile sekund działa w trybie Czasowym.")]
        [Min(0.1f)] public float czasDzialania = 5f;
        [Tooltip("Tekst podpowiedzi pokazywany graczowi po słowie [E]. Napisz, CO przełącznik robi.")]
        public string opis = "Użyj przełącznika";

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        public Powiadomienie gdyWlaczonyKomunikat = new Powiadomienie();
        public Powiadomienie gdyWylaczonyKomunikat = new Powiadomienie();

        [Header("Wygląd")]
        [Tooltip("Dźwignia, która przechyla się po włączeniu (opcjonalnie).")]
        public Transform dzwignia;
        [Tooltip("Kąt przechylenia dźwigni po włączeniu (stopnie, wokół osi X).")]
        public float katDzwigni = 60f;
        [Tooltip("Elementy zmieniające kolor: pomarańczowy = wyłączony, zielony = włączony.")]
        public Renderer[] kolorowane;

        float _wylaczO;
        Quaternion _obrotDzwigni;

        void Awake()
        {
            if (dzwignia != null) _obrotDzwigni = dzwignia.localRotation;
            OdswiezWyglad();
        }

        void OnValidate() { OdswiezWyglad(); }

        void Update()
        {
            if (tryb == TrybPrzelacznika.Czasowy && CzyWlaczony)
            {
                float zostalo = _wylaczO - Time.time;
                if (zostalo <= 0f) UstawStan(false);
                else if (Gracz.Instancja != null && Gracz.Instancja.HUD != null)
                    Gracz.Instancja.HUD.ZglosOdliczanie(zostalo, czasDzialania);
            }

            if (dzwignia != null)
            {
                Quaternion cel = _obrotDzwigni * Quaternion.Euler(CzyWlaczony ? katDzwigni : 0f, 0f, 0f);
                dzwignia.localRotation = Quaternion.RotateTowards(dzwignia.localRotation, cel, 400f * Time.deltaTime);
            }
        }

        protected override void GdyZmianaStanu(bool wlaczony)
        {
            OdswiezWyglad();
            (wlaczony ? gdyWlaczonyKomunikat : gdyWylaczonyKomunikat).Wyslij(transform.position);
        }

        void OdswiezWyglad()
        {
            bool wl = Application.isPlaying && CzyWlaczony;
            Paleta.Pomaluj(kolorowane, wl ? Paleta.Aktywny : Paleta.Nieaktywny, wl ? 1.2f : 0.4f);
        }

        // --- interakcja (klawisz E) ---

        public string TekstPodpowiedzi(Gracz gracz) { return opis; }

        public bool CzyMoznaUzyc(Gracz gracz)
        {
            if (tryb == TrybPrzelacznika.Jednorazowy && CzyWlaczony) return false;
            return isActiveAndEnabled;
        }

        public void Uzyj(Gracz gracz)
        {
            switch (tryb)
            {
                case TrybPrzelacznika.Przelaczany:
                    UstawStan(!CzyWlaczony);
                    break;
                case TrybPrzelacznika.Czasowy:
                    _wylaczO = Time.time + czasDzialania; // ponowne użycie odnawia czas
                    UstawStan(true);
                    break;
                case TrybPrzelacznika.Jednorazowy:
                    UstawStan(true);
                    break;
            }
        }
    }
}
