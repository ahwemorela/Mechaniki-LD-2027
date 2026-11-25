using UnityEngine;
using UnityEngine.Events;

namespace MechanikiLD
{
    /// <summary>
    /// ODBIORNIK SYGNAŁU.
    /// Klasa bazowa dla wszystkiego, co można WŁĄCZYĆ albo WYŁĄCZYĆ z zewnątrz:
    /// drzwi, ruchome platformy, windy, wysuwane mosty.
    ///
    /// Sygnał wysyłają "aktywatory" (przycisk naciskowy, przełącznik, klucz, strażnik...).
    /// Każdy aktywator ma listę "Cele" - przeciągasz tam obiekt z tym komponentem i gotowe.
    ///
    /// Jak to działa:
    ///  - każdy włączony aktywator dokłada jeden sygnał, a wyłączony go zabiera,
    ///  - obiekt jest aktywny, gdy liczba sygnałów >= "Wymagane Sygnaly"
    ///    (ustaw 2, jeśli np. drzwi mają się otworzyć dopiero po wciśnięciu DWÓCH przycisków naraz).
    /// </summary>
    public abstract class ObiektAktywowalny : MonoBehaviour
    {
        [Header("Aktywacja (sygnały z przycisków, przełączników itd.)")]
        [Tooltip("Ile aktywatorów musi być włączonych JEDNOCZEŚNIE, żeby ten obiekt zadziałał.\n1 = wystarczy jeden przycisk.\n2 = potrzebne dwa przyciski naraz itd.")]
        [Min(1)] public int wymaganeSygnaly = 1;

        [Tooltip("Zaznacz, jeśli obiekt ma działać od początku, BEZ żadnego sygnału.\nSygnał z przycisku będzie go wtedy WYŁĄCZAŁ (np. alarm zamyka otwarte drzwi albo przycisk zatrzymuje platformę).")]
        public bool aktywnyBezSygnalu = false;

        [Header("Zdarzenia (opcjonalne, dla chętnych)")]
        [Tooltip("Wywoływane w momencie, gdy obiekt staje się aktywny.")]
        public UnityEvent gdyAktywowany;
        [Tooltip("Wywoływane w momencie, gdy obiekt przestaje być aktywny.")]
        public UnityEvent gdyDezaktywowany;

        int _sygnaly;
        bool _recznySygnal;
        bool _stan;
        bool _zainicjowany;

        /// <summary>Czy obiekt jest w tej chwili aktywny (drzwi otwarte, platforma jedzie...).</summary>
        public bool CzyAktywny
        {
            get
            {
                if (!_zainicjowany) return aktywnyBezSygnalu;
                return _stan;
            }
        }

        protected virtual void Start()
        {
            _zainicjowany = true;
            _stan = ObliczStan();
            GdyZmianaStanu(_stan, true);
        }

        /// <summary>Dokłada jeden sygnał (wywołuje to aktywator, gdy się włącza).</summary>
        public void Aktywuj()
        {
            _sygnaly++;
            Odswiez();
        }

        /// <summary>Zabiera jeden sygnał (wywołuje to aktywator, gdy się wyłącza).</summary>
        public void Dezaktywuj()
        {
            _sygnaly = Mathf.Max(0, _sygnaly - 1);
            Odswiez();
        }

        /// <summary>Przełącza obiekt "ręcznie" - wygodne do podpinania pod UnityEvent.</summary>
        public void Przelacz()
        {
            _recznySygnal = !_recznySygnal;
            if (_recznySygnal) _sygnaly++; else _sygnaly = Mathf.Max(0, _sygnaly - 1);
            Odswiez();
        }

        bool ObliczStan()
        {
            bool wystarczySygnalow = _sygnaly >= wymaganeSygnaly;
            // "aktywnyBezSygnalu" odwraca logikę: działa, dopóki NIE dostanie sygnału
            return aktywnyBezSygnalu ? !wystarczySygnalow : wystarczySygnalow;
        }

        void Odswiez()
        {
            if (!_zainicjowany) return; // stan policzymy w Start()
            bool nowy = ObliczStan();
            if (nowy == _stan) return;
            _stan = nowy;
            GdyZmianaStanu(_stan, false);
            if (_stan) gdyAktywowany?.Invoke(); else gdyDezaktywowany?.Invoke();
        }

        /// <summary>
        /// Tu konkretny obiekt reaguje na zmianę stanu (drzwi zaczynają się otwierać itd.).
        /// "natychmiast" = true na starcie sceny - wtedy ustawiamy stan bez animacji.
        /// </summary>
        protected abstract void GdyZmianaStanu(bool aktywny, bool natychmiast);
    }
}
