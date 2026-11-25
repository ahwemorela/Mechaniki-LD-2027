using System.Collections.Generic;
using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// PODPOWIEDŹ.
    /// Strefa, w której na dole ekranu wyświetla się panel z tekstem (tytuł + treść).
    /// Służy do krótkich samouczków: "Wciśnij E, żeby podnieść skrzynię".
    ///
    /// Uwaga projektowa: tekst to ostatnia deska ratunku. Najpierw spróbuj nauczyć gracza samą
    /// przestrzenią - bezpieczną sytuacją, w której mechaniki nie da się nie odkryć.
    ///
    /// JAK PRZEROBIĆ POD SWOJĄ GRĘ: tej samej strefy możesz użyć do narracji - myśli bohatera,
    /// notatki znalezionej na ścianie, komunikatu z radia. Wygląd panelu zmienisz w prefabie HUD.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Podpowiedź")]
    public class Podpowiedz : MonoBehaviour, IStrefaGracza
    {
        [Tooltip("Nagłówek panelu (może być pusty).")]
        public string tytul = "";
        [Tooltip("Treść podpowiedzi.")]
        [TextArea(3, 10)] public string tresc = "Tekst podpowiedzi";
        [Tooltip("Zaznaczone: podpowiedź pokaże się tylko przy pierwszym wejściu do strefy.")]
        public bool tylkoRaz = false;

        readonly List<Gracz> _wStrefie = new List<Gracz>();
        bool _pokazywana;
        bool _zuzyta;

        public void GraczWszedl(Gracz gracz) { if (!_wStrefie.Contains(gracz)) _wStrefie.Add(gracz); }
        public void GraczWStrefie(Gracz gracz) { }
        public void GraczWyszedl(Gracz gracz) { _wStrefie.Remove(gracz); }

        void Update()
        {
            // podpowiedź widzi tylko postać, którą gracz akurat steruje
            Gracz sterowana = Gracz.Instancja;
            bool pokaz = sterowana != null && sterowana.HUD != null && _wStrefie.Contains(sterowana) && !(tylkoRaz && _zuzyta && !_pokazywana);
            if (pokaz == _pokazywana) return;

            _pokazywana = pokaz;
            if (sterowana == null || sterowana.HUD == null) return;
            if (pokaz) sterowana.HUD.DodajPodpowiedz(this);
            else { sterowana.HUD.UsunPodpowiedz(this); _zuzyta = true; }
        }

        void OnDisable()
        {
            _wStrefie.Clear();
            _pokazywana = false;
            if (Gracz.Instancja != null && Gracz.Instancja.HUD != null) Gracz.Instancja.HUD.UsunPodpowiedz(this);
        }

        void OnDrawGizmos()
        {
            BoxCollider pudelko = GetComponent<BoxCollider>();
            if (pudelko == null) return;
            Gizmos.color = new Color(1f, 1f, 1f, 0.08f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(pudelko.center, pudelko.size);
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawWireCube(pudelko.center, pudelko.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
