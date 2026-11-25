using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// KLUCZ.
    /// Przedmiot do zebrania. Po zebraniu trafia do ekwipunku postaci i pozwala otworzyć
    /// drzwi, które wymagają klucza TEGO SAMEGO RODZAJU (pole "Rodzaj").
    ///
    /// Rodzaj klucza to osobny plik (nazwa + kolor + ikona). Gotowe rodzaje leżą w "Ustawienia/Klucze",
    /// własny tworzysz przez: PPM w oknie Project > Create > Mechaniki LD > Rodzaj klucza.
    ///
    /// Klucz jest też aktywatorem: w momencie zebrania może coś uruchomić (lista "Cele"),
    /// np. zamknąć za graczem bramę - klasyczna "pułapka po zabraniu skarbu".
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ: klucz nie musi być kluczem. Podmień model (dziecko "Model"),
    /// zmień nazwę w pliku rodzaju i teksty poniżej - i masz kartę dostępu, baterię, artefakt, bilet.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Klucz")]
    public class Klucz : Aktywator, IInteraktywny, IStrefaGracza
    {
        [Header("Klucz")]
        [Tooltip("Rodzaj klucza (plik z nazwą i kolorem). Otwiera drzwi, które wskazują ten sam plik.")]
        public RodzajKlucza rodzaj;

        [Tooltip("Zaznaczone: klucz zbiera się sam, gdy postać w niego wejdzie.\nOdznaczone: trzeba na niego spojrzeć i wcisnąć E.")]
        public bool zbierajDotykiem = true;

        [Header("Komunikaty i dźwięki  ({0} = nazwa klucza; puste = wyłączone)")]
        [Tooltip("Po zebraniu klucza.")]
        public Powiadomienie poZebraniu = new Powiadomienie("Zebrano klucz: {0}");
        [Tooltip("Podpowiedź pod celownikiem, gdy klucz zbiera się klawiszem E.")]
        public string podpowiedzInterakcji = "Weź klucz ({0})";

        [Header("Wygląd")]
        [Tooltip("Część, która się obraca i unosi (sam model klucza). Tu podmieniasz model na własny.")]
        public Transform model;
        [Tooltip("Prędkość obrotu modelu (stopnie na sekundę). Ruch przyciąga wzrok gracza! 0 = stoi w miejscu.")]
        public float predkoscObrotu = 90f;
        [Tooltip("Jak wysoko model unosi się i opada (metry). 0 = nie unosi się.")]
        public float wysokoscUnoszenia = 0.1f;
        [Tooltip("Elementy, które mają przyjąć kolor klucza. Puste = model zachowuje własne kolory.")]
        public Renderer[] kolorowane;
        [Tooltip("Światełko przy kluczu (przyjmuje jego kolor). Świecący obiekt łatwiej zauważyć z daleka.")]
        public Light swiatlo;

        bool _zebrany;
        Vector3 _pozycjaModelu;

        string Nazwa => rodzaj != null ? rodzaj.nazwa : "";

        void Awake()
        {
            if (model != null) _pozycjaModelu = model.localPosition;
            OdswiezKolor();
        }

        void OnValidate() { OdswiezKolor(); }

        void OdswiezKolor()
        {
            if (rodzaj == null) return;
            Paleta.Pomaluj(kolorowane, rodzaj.kolor, 0.6f);
            if (swiatlo != null) swiatlo.color = rodzaj.kolor;
        }

        void Update()
        {
            if (model == null || _zebrany) return;
            model.Rotate(0f, predkoscObrotu * Time.deltaTime, 0f, Space.World);
            model.localPosition = _pozycjaModelu + Vector3.up * (Mathf.Sin(Time.time * 2f) * wysokoscUnoszenia);
        }

        void Zbierz(Gracz gracz)
        {
            if (_zebrany || gracz.Ekwipunek == null) return;
            if (rodzaj == null)
            {
                Debug.LogWarning("Klucz \"" + name + "\" nie ma ustawionego pola Rodzaj - nie da się go zebrać.", this);
                return;
            }
            _zebrany = true;

            gracz.Ekwipunek.DodajKlucz(rodzaj);
            poZebraniu.Wyslij(transform.position, Nazwa);

            // zebranie klucza może coś uruchomić (lista "Cele")
            UstawStan(true);

            // chowamy klucz (nie niszczymy obiektu, żeby połączenia z celami dalej działały)
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (Light l in GetComponentsInChildren<Light>()) l.enabled = false;
        }

        // --- zbieranie dotykiem (strefa) ---
        public void GraczWszedl(Gracz gracz) { if (zbierajDotykiem) Zbierz(gracz); }
        public void GraczWStrefie(Gracz gracz) { }
        public void GraczWyszedl(Gracz gracz) { }

        // --- zbieranie klawiszem E ---
        public string TekstPodpowiedzi(Gracz gracz) { return Powiadomienie.Wstaw(podpowiedzInterakcji, Nazwa); }
        public bool CzyMoznaUzyc(Gracz gracz) { return !_zebrany; }
        public void Uzyj(Gracz gracz) { Zbierz(gracz); }
    }
}
