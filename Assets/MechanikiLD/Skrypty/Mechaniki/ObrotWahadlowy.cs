using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// OBRÓT WAHADŁOWY - obraca obiekt w lewo i w prawo, z postojem na końcach.
    /// Typowe zastosowanie: kamera ochrony "zamiatająca" wzrokiem pomieszczenie.
    /// Gracz obserwuje rytm i przebiega, gdy kamera patrzy w drugą stronę.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Obrót Wahadłowy (kamera)")]
    public class ObrotWahadlowy : MonoBehaviour
    {
        [Tooltip("O ile stopni obiekt odchyla się w każdą stronę od pozycji początkowej.")]
        public float kat = 45f;
        [Tooltip("Ile sekund trwa przejście z jednej skrajnej pozycji do drugiej.")]
        [Min(0.1f)] public float czasObrotu = 3f;
        [Tooltip("Postój w skrajnych pozycjach (sekundy). Dłuższy postój = większe \"okno\" dla gracza.")]
        public float czasPostoju = 1f;
        [Tooltip("Przesunięcie rytmu (sekundy) - żeby dwie kamery nie ruszały się identycznie.")]
        public float przesuniecieCyklu = 0f;

        Quaternion _obrotStartowy;

        void Awake() { _obrotStartowy = transform.localRotation; }

        void Update()
        {
            // cykl: obrót w prawo -> postój -> obrót w lewo -> postój
            float okres = 2f * (czasObrotu + czasPostoju);
            float t = Mathf.Repeat(Time.time + przesuniecieCyklu, okres);

            float polozenie; // od -1 (skrajnie w lewo) do 1 (skrajnie w prawo)
            if (t < czasObrotu) polozenie = Mathf.Lerp(-1f, 1f, Mathf.SmoothStep(0f, 1f, t / czasObrotu));
            else if (t < czasObrotu + czasPostoju) polozenie = 1f;
            else if (t < 2f * czasObrotu + czasPostoju) polozenie = Mathf.Lerp(1f, -1f, Mathf.SmoothStep(0f, 1f, (t - czasObrotu - czasPostoju) / czasObrotu));
            else polozenie = -1f;

            // obracamy wokół pionowej osi świata, zachowując pochylenie kamery w dół
            transform.localRotation = Quaternion.AngleAxis(polozenie * kat, Vector3.up) * _obrotStartowy;
        }
    }
}
