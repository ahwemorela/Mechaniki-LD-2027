namespace MechanikiLD
{
    /// <summary>
    /// Wspólny "kontrakt" dla obiektów, których gracz może użyć klawiszem interakcji (E):
    /// klucz, skrzynia do podniesienia, przełącznik, drzwi otwierane ręcznie.
    ///
    /// Komponent GraczInterakcja co klatkę sprawdza, na co patrzy gracz. Jeśli obiekt
    /// ma komponent spełniający ten kontrakt, na ekranie pojawia się podpowiedź,
    /// a po wciśnięciu E wywoływana jest metoda Uzyj().
    /// </summary>
    public interface IInteraktywny
    {
        /// <summary>Tekst pokazywany pod celownikiem, np. "Podnieś skrzynię".</summary>
        string TekstPodpowiedzi(Gracz gracz);

        /// <summary>Czy w tej chwili da się użyć obiektu (np. zebranego klucza już nie).</summary>
        bool CzyMoznaUzyc(Gracz gracz);

        /// <summary>Co się dzieje po wciśnięciu klawisza interakcji.</summary>
        void Uzyj(Gracz gracz);
    }
}
