using System;
using UnityEngine;

namespace MechanikiLD
{
    /// <summary>
    /// USTAWIENIA PROJEKTU - wspólne kolory mechanik ("język kolorów" poziomu) i głośność dźwięków.
    /// Bez programowania: zaznacz plik "UstawieniaLD" (menu Mechaniki LD > Ustawienia)
    /// i zmień kolory w Inspektorze - przemalują się wszystkie przyciski, drzwi, punkty kontrolne itd.
    ///
    /// Teksty i dźwięki ustawia się osobno NA KAŻDYM OBIEKCIE (sekcja "Komunikaty i dźwięki"),
    /// a rodzaje kluczy w plikach "Rodzaj klucza".
    ///
    /// Plik musi leżeć w folderze o nazwie "Resources" i nazywać się "UstawieniaLD" -
    /// dzięki temu mechaniki znajdują go same. Jeśli go nie ma, używane są kolory domyślne.
    /// </summary>
    [CreateAssetMenu(menuName = "Mechaniki LD/Ustawienia projektu", fileName = "UstawieniaLD")]
    public class UstawieniaLD : ScriptableObject
    {
        [Header("KOLORY MECHANIK  (język kolorów poziomu)")]
        [Tooltip("Włączone / otwarte / zaliczone (wciśnięty przycisk, otwarte drzwi, aktywny punkt kontrolny).")]
        public Color kolorAktywny = new Color(0.25f, 0.9f, 0.35f);
        [Tooltip("Czeka na akcję gracza (przycisk, przełącznik, zamknięte drzwi, punkt kontrolny).")]
        public Color kolorNieaktywny = new Color(0.95f, 0.55f, 0.1f);
        [Tooltip("Zagrożenie: pasek wykrycia, światło strażnika tuż przed wykryciem.")]
        public Color kolorNiebezpieczny = new Color(1f, 0.1f, 0.1f);
        [Tooltip("Hak: podgląd zasięgu w oknie Scene.\n(Kolor samych punktów haka to materiał Mech_Hak.)")]
        public Color kolorHaka = new Color(1f, 0.85f, 0.1f);
        [Tooltip("Trasy ruchomych platform i strażników w oknie Scene.")]
        public Color kolorTras = new Color(0.2f, 0.9f, 0.9f);
        [Tooltip("Stożki widzenia strażników i kamer w oknie Scene.")]
        public Color kolorStraznika = new Color(0.7f, 0.3f, 1f);
        [Tooltip("Linie łączące aktywatory z celami w oknie Scene.")]
        public Color kolorPolaczen = new Color(1f, 0.8f, 0.2f);

        [Header("DŹWIĘK")]
        [Tooltip("Głośność wszystkich dźwięków mechanik (0 = cisza, 1 = pełna głośność).")]
        [Range(0f, 1f)] public float glosnoscDzwiekow = 0.8f;

        // ---------- dostęp z kodu ----------

        static UstawieniaLD _aktualne;

        /// <summary>Ustawienia używane przez mechaniki (plik Resources/UstawieniaLD). Null, gdy pliku nie ma.</summary>
        public static UstawieniaLD Aktualne
        {
            get
            {
                if (_aktualne == null)
                {
                    // Unity nie pozwala wczytywać zasobów w niektórych momentach (np. podczas ładowania sceny);
                    // wtedy po prostu spróbujemy ponownie przy następnym użyciu.
                    try { _aktualne = Resources.Load<UstawieniaLD>("UstawieniaLD"); }
                    catch (Exception) { _aktualne = null; }
                }
                return _aktualne;
            }
        }

        /// <summary>Odświeża kolory obiektów na otwartej scenie (działa tylko w edytorze, poza trybem Play).</summary>
        public static void OdswiezSceneWEdytorze()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= OdswiezScene;
            UnityEditor.EditorApplication.delayCall += OdswiezScene;
#endif
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            _aktualne = this;
            OdswiezSceneWEdytorze();
        }

        // Wywołuje OnValidate na wszystkich skryptach mechanik - to one malują obiekty.
        static void OdswiezScene()
        {
            if (Application.isPlaying) return;
            const System.Reflection.BindingFlags flagi = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                                                         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly;
            foreach (MonoBehaviour skrypt in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (skrypt == null || skrypt.GetType().Namespace != "MechanikiLD") continue;
                for (Type typ = skrypt.GetType(); typ != null && typ != typeof(MonoBehaviour); typ = typ.BaseType)
                {
                    var metoda = typ.GetMethod("OnValidate", flagi);
                    if (metoda == null) continue;
                    try { metoda.Invoke(skrypt, null); } catch (Exception) { }
                    break;
                }
            }
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }
}
