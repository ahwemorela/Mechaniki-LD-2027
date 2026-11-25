using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// PRZYCISK NACISKOWY (płyta naciskowa).
    /// Włącza się, gdy coś na nim stoi: gracz albo przedmiot fizyczny (np. skrzynia).
    /// Gdy ciężar zniknie - wyłącza się (chyba że zaznaczysz "Zostaje Wcisniety").
    ///
    /// To, co przycisk uruchamia, wskazujesz na liście "Cele" (drzwi, platforma, winda, most...).
    ///
    /// Jak wykrywa ciężar: co krok fizyki sprawdza, czy w niewidzialnym pudełku nad płytką
    /// ("Strefa wykrywania", widoczna w oknie Scene jako zielona ramka) znajduje się gracz
    /// albo obiekt z Rigidbody.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Przycisk Naciskowy")]
    public class PrzyciskNaciskowy : Aktywator
    {
        [Header("Kto może wcisnąć przycisk")]
        public bool reagujeNaGracza = true;
        public bool reagujeNaPrzedmioty = true;
        [Tooltip("Przedmioty lżejsze niż ta wartość (kg) nie wcisną przycisku. 0 = każdy przedmiot.")]
        public float minimalnaMasa = 0f;
        [Tooltip("Postać mniejsza niż ten rozmiar nie wciśnie przycisku (mechanika zmiany skali).\n0 = każda postać, 1 = normalna i większa, 2 = tylko powiększona (\"ciężki przycisk\").")]
        public float minimalnyRozmiarGracza = 0f;

        [Header("Zachowanie")]
        [Tooltip("Zaznaczone: przycisk po pierwszym wciśnięciu zostaje wciśnięty na stałe (jednorazowy).")]
        public bool zostajeWcisniety = false;
        [Tooltip("Ile sekund po zejściu z przycisku nadal jest on wciśnięty.\nMała wartość zapobiega \"migotaniu\"; duża daje graczowi czas, żeby zdążyć do drzwi.")]
        public float opoznienieZwolnienia = 0.15f;

        [Header("Strefa wykrywania (zielona ramka w oknie Scene)")]
        public Vector3 srodekStrefy = new Vector3(0f, 0.4f, 0f);
        public Vector3 rozmiarStrefy = new Vector3(1.5f, 0.6f, 1.5f);

        [Header("Komunikaty i dźwięki  (puste = wyłączone)")]
        public Powiadomienie gdyWcisniety = new Powiadomienie();
        public Powiadomienie gdyZwolniony = new Powiadomienie();

        [Header("Wygląd")]
        [Tooltip("Płytka, która zapada się po wciśnięciu.")]
        public Transform plytka;
        public float glebokoscWcisniecia = 0.07f;
        [Tooltip("Elementy zmieniające kolor: pomarańczowy = czeka, zielony = wciśnięty.")]
        public Renderer[] kolorowane;

        readonly Collider[] _bufor = new Collider[24];
        Vector3 _pozycjaPlytki;
        float _ostatniNacisk = -100f;

        void Awake()
        {
            if (plytka != null) _pozycjaPlytki = plytka.localPosition;
            OdswiezWyglad();
        }

        void OnValidate() { OdswiezWyglad(); }

        void FixedUpdate()
        {
            if (CzyCosNaciska()) _ostatniNacisk = Time.time;

            bool wcisniety = Time.time - _ostatniNacisk <= opoznienieZwolnienia;
            if (zostajeWcisniety && CzyWlaczony) wcisniety = true;
            UstawStan(wcisniety);

            // animacja płytki
            if (plytka != null)
            {
                Vector3 cel = _pozycjaPlytki + Vector3.down * (CzyWlaczony ? glebokoscWcisniecia : 0f);
                plytka.localPosition = Vector3.MoveTowards(plytka.localPosition, cel, Time.fixedDeltaTime * 0.6f);
            }
        }

        bool CzyCosNaciska()
        {
            Vector3 srodek = transform.TransformPoint(srodekStrefy);
            Vector3 polowa = Vector3.Scale(rozmiarStrefy, transform.lossyScale) * 0.5f;
            int ile = Physics.OverlapBoxNonAlloc(srodek, polowa, _bufor, transform.rotation, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < ile; i++)
            {
                Collider c = _bufor[i];
                if (c.transform.IsChildOf(transform)) continue; // własne części przycisku

                Gracz gracz = Gracz.Z(c);
                if (gracz != null)
                {
                    if (reagujeNaGracza && gracz.Rozmiar >= minimalnyRozmiarGracza - 0.01f) return true;
                    continue;
                }

                if (!reagujeNaPrzedmioty) continue;
                Rigidbody cialo = c.attachedRigidbody;
                if (cialo == null || cialo.isKinematic || cialo.mass < minimalnaMasa) continue;

                // przedmiot niesiony przez gracza "wisi w powietrzu" - nie naciska
                PrzedmiotDoPodnoszenia przedmiot = cialo.GetComponent<PrzedmiotDoPodnoszenia>();
                if (przedmiot != null && przedmiot.CzyNiesiony) continue;

                return true;
            }
            return false;
        }

        protected override void GdyZmianaStanu(bool wlaczony)
        {
            OdswiezWyglad();
            (wlaczony ? gdyWcisniety : gdyZwolniony).Wyslij(transform.position);
        }

        void OdswiezWyglad()
        {
            bool wl = Application.isPlaying && CzyWlaczony;
            Paleta.Pomaluj(kolorowane, wl ? Paleta.Aktywny : Paleta.Nieaktywny, wl ? 1.2f : 0.3f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Paleta.Aktywny;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(srodekStrefy, rozmiarStrefy);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
