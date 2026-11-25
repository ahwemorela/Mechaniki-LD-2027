using System.Collections.Generic;
using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// Ekwipunek postaci - przechowuje zebrane klucze (każdy klucz to plik "Rodzaj klucza").
    /// Zebrane klucze widać w rogu ekranu. Każda postać ma własny ekwipunek.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Gracz/Gracz Ekwipunek")]
    public class GraczEkwipunek : MonoBehaviour
    {
        [Tooltip("Klucze, które postać ma w tej chwili. Dodaj tu coś, żeby zaczynała poziom z kluczem.")]
        public List<RodzajKlucza> klucze = new List<RodzajKlucza>();

        public void DodajKlucz(RodzajKlucza rodzaj)
        {
            if (rodzaj != null) klucze.Add(rodzaj);
        }

        public bool MaKlucz(RodzajKlucza rodzaj)
        {
            return rodzaj != null && klucze.Contains(rodzaj);
        }

        /// <summary>Zabiera jeden klucz danego rodzaju. Zwraca false, jeśli postać go nie miała.</summary>
        public bool ZuzyjKlucz(RodzajKlucza rodzaj)
        {
            return klucze.Remove(rodzaj);
        }
    }
}
