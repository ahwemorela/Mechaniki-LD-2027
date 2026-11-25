using System.Collections;
using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// ZMIANA SKALI - przedmiot, po którego zebraniu postać zmienia rozmiar.
    /// Trzy gotowe prefaby różnią się tylko polem "Skala": Pomniejszacz (0.3), Powiekszacz (2.5)
    /// i PrzywracaczRozmiaru (1).
    ///
    /// Zebrany przedmiot znika i po chwili pojawia się ponownie ("Czas Odnowienia") - dzięki temu
    /// gracz nie utknie w złym rozmiarze. Ustaw 0, jeśli ma być jednorazowy.
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ:
    ///  - dowolna "Skala": 0.5 i 1.5 dają subtelniejszą różnicę, 0.15 i 4 - ekstremalną,
    ///  - "Czas Trwania" > 0: efekt mija sam (eliksir, grzyb) i gracz musi zdążyć - na ekranie widać odliczanie,
    ///  - podmień model i teksty: grzyb, eliksir, promień, strefa, w którą się wchodzi,
    ///  - wyłącz "Znika Po Zebraniu", żeby zrobić stałą "bramkę" zmieniającą rozmiar przy każdym przejściu.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Zmiana Skali")]
    public class ZmianaSkali : MonoBehaviour, IStrefaGracza
    {
        [Header("Efekt")]
        [Tooltip("Rozmiar postaci po zebraniu: 1 = normalny, 0.3 = bardzo mała, 2.5 = bardzo duża.")]
        [Min(0.1f)] public float skala = 0.3f;
        [Tooltip("Ile sekund trwa efekt. 0 = do zebrania innego przedmiotu zmiany skali.")]
        [Min(0f)] public float czasTrwania = 0f;

        [Header("Przedmiot")]
        [Tooltip("Zaznaczone: przedmiot znika po zebraniu. Odznaczone: działa jak stała bramka.")]
        public bool znikaPoZebraniu = true;
        [Tooltip("Po ilu sekundach przedmiot pojawia się ponownie. 0 = nie wraca (jednorazowy).")]
        [Min(0f)] public float czasOdnowienia = 4f;

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        public Powiadomienie poZebraniu = new Powiadomienie("Zmieniasz rozmiar");

        [Header("Wygląd")]
        [Tooltip("Część, która się obraca i znika po zebraniu. Tu podmieniasz model na własny.")]
        public Transform model;
        public float predkoscObrotu = 60f;
        [Tooltip("Kolor przedmiotu.")]
        public Color kolor = Color.white;
        [Tooltip("Elementy, które mają przyjąć ten kolor. Puste = model zachowuje własne kolory.")]
        public Renderer[] kolorowane;

        bool _dostepny = true;

        void Awake() { OdswiezKolor(); }
        void OnValidate() { OdswiezKolor(); }
        void OdswiezKolor() { Paleta.Pomaluj(kolorowane, kolor, 0.7f); }

        void Update()
        {
            if (model != null && _dostepny) model.Rotate(0f, predkoscObrotu * Time.deltaTime, 0f, Space.World);
        }

        public void GraczWszedl(Gracz gracz) { }
        public void GraczWyszedl(Gracz gracz) { }

        // sprawdzamy co klatkę (a nie tylko przy wejściu): jeśli zabrakło miejsca, żeby urosnąć,
        // wystarczy się odsunąć od ściany - nie trzeba wychodzić ze strefy i wracać
        public void GraczWStrefie(Gracz gracz)
        {
            if (!_dostepny || gracz.Skala == null) return;
            if (Mathf.Approximately(gracz.Skala.Docelowa, skala)) return;   // postać ma już ten rozmiar
            if (!gracz.Skala.UstawSkale(skala, czasTrwania)) return;        // brak miejsca

            poZebraniu.Wyslij(transform.position);

            if (znikaPoZebraniu) StartCoroutine(ZniknijIWroc());
        }

        IEnumerator ZniknijIWroc()
        {
            _dostepny = false;
            if (model != null) model.gameObject.SetActive(false);

            if (czasOdnowienia <= 0f) yield break;   // jednorazowy
            yield return new WaitForSeconds(czasOdnowienia);

            if (model != null) model.gameObject.SetActive(true);
            _dostepny = true;
        }
    }
}
