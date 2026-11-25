namespace MechanikiLD
{
    /// <summary>
    /// Wspólny "kontrakt" dla STREF, czyli obszarów reagujących na obecność gracza
    /// (strefa śmierci, punkt kontrolny, podpowiedź, przedmiot zmiany skali, meta...).
    ///
    /// Strefa to obiekt z colliderem, który ma zaznaczone "Is Trigger" - gracz przez niego przechodzi.
    /// Komponent Gracz co klatkę sprawdza, w których strefach stoi, i wywołuje poniższe metody.
    /// </summary>
    public interface IStrefaGracza
    {
        /// <summary>Wywoływane raz, w klatce w której gracz wszedł do strefy.</summary>
        void GraczWszedl(Gracz gracz);

        /// <summary>Wywoływane co klatkę, dopóki gracz jest w strefie.</summary>
        void GraczWStrefie(Gracz gracz);

        /// <summary>Wywoływane raz, gdy gracz opuścił strefę.</summary>
        void GraczWyszedl(Gracz gracz);
    }
}
