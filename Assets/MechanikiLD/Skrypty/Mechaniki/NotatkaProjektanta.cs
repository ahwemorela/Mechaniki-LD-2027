using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// NOTATKA PROJEKTANTA.
    /// Tekst widoczny TYLKO w oknie Scene (w grze go nie ma). Działa jak karteczka samoprzylepna
    /// przyklejona do miejsca w poziomie: opisz nią zamiar ("tu gracz ma zobaczyć klucz przez kratę"),
    /// problem do poprawy albo uwagę z testów.
    ///
    /// Sceny przykładowe w tym projekcie są opisane właśnie takimi notatkami.
    /// </summary>
    [AddComponentMenu("Mechaniki LD/Notatka Projektanta")]
    public class NotatkaProjektanta : MonoBehaviour
    {
        [TextArea(2, 12)] public string tekst = "Notatka";
        public Color kolor = new Color(1f, 0.92f, 0.4f);
        [Tooltip("Z jakiej odległości kamery okna Scene notatka jest jeszcze widoczna (metry).")]
        public float zasiegWidocznosci = 45f;

#if UNITY_EDITOR
        static GUIStyle _styl;

        void OnDrawGizmos()
        {
            Gizmos.color = kolor;
            Gizmos.DrawSphere(transform.position, 0.15f);

            Camera kamera = Camera.current;
            if (kamera == null || string.IsNullOrEmpty(tekst)) return;
            if (Vector3.Distance(kamera.transform.position, transform.position) > zasiegWidocznosci) return;

            if (_styl == null)
            {
                _styl = new GUIStyle(UnityEditor.EditorStyles.helpBox)
                {
                    fontSize = 12,
                    wordWrap = true,
                    richText = true,
                    fixedWidth = 280f,
                    padding = new RectOffset(8, 8, 6, 6)
                };
            }
            Color stary = GUI.backgroundColor;
            GUI.backgroundColor = kolor;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, tekst, _styl);
            GUI.backgroundColor = stary;
        }
#endif
    }
}
