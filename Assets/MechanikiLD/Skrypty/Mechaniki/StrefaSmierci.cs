using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// STREFA ŚMIERCI.
    /// Obszar, który "zabija" gracza - wraca on wtedy do ostatniego punktu kontrolnego.
    /// Używaj jej na dnie przepaści, jako lawę, wodę albo kolce.
    /// Przedmioty do podnoszenia, które tu wpadną, wracają na swoje miejsce startowe.
    ///
    /// Strefa to collider z zaznaczonym "Is Trigger". Jej rozmiar zmieniasz skalą obiektu lub rozmiarem collidera.
    /// W tym projekcie wszystko, co zabija, jest CZERWONE.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Strefa Śmierci")]
    public class StrefaSmierci : MonoBehaviour, IStrefaGracza
    {
        [Tooltip("Komunikat pokazywany graczowi po odrodzeniu. Warto napisać, CO go zabiło - gracz uczy się na błędach.")]
        public string komunikat = "Zginąłeś";

        public void GraczWszedl(Gracz gracz) { }
        public void GraczWyszedl(Gracz gracz) { }

        public void GraczWStrefie(Gracz gracz)
        {
            if (!isActiveAndEnabled) return;
            if (gracz.Odrodzenie != null) gracz.Odrodzenie.Zgin(komunikat);
        }

        void OnTriggerEnter(Collider other)
        {
            PrzedmiotDoPodnoszenia przedmiot = other.GetComponentInParent<PrzedmiotDoPodnoszenia>();
            if (przedmiot != null) przedmiot.WrocNaStart();
        }

        void OnDrawGizmos()
        {
            BoxCollider pudelko = GetComponent<BoxCollider>();
            if (pudelko == null) return;
            Gizmos.color = new Color(Paleta.Niebezpieczny.r, Paleta.Niebezpieczny.g, Paleta.Niebezpieczny.b, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(pudelko.center, pudelko.size);
            Gizmos.color = Paleta.Niebezpieczny;
            Gizmos.DrawWireCube(pudelko.center, pudelko.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
