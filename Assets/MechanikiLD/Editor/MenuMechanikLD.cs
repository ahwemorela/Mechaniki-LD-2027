using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MechanikiLD.Edytor
{
    /// <summary>
    /// Menu "Mechaniki LD" na górnym pasku Unity - skróty do miejsc, w których
    /// dostosowuje się projekt: kolory mechanik, rodzaje kluczy, interfejs, prefab gracza,
    /// dokumentacja oraz tworzenie nowego poziomu z szablonu.
    /// </summary>
    public static class MenuMechanikLD
    {
        const string Folder = "Assets/MechanikiLD";

        [MenuItem("Mechaniki LD/Ustawienia: kolory mechanik", priority = 0)]
        static void Ustawienia() { Pokaz(Folder + "/Resources/UstawieniaLD.asset"); }

        [MenuItem("Mechaniki LD/Rodzaje kluczy", priority = 1)]
        static void RodzajeKluczy() { Pokaz(Folder + "/Ustawienia/Klucze/Klucz_Czerwony.asset"); }

        [MenuItem("Mechaniki LD/Edytuj interfejs (HUD)", priority = 2)]
        static void EdytujHud() { Otworz(Folder + "/Prefaby/UI/HUD.prefab"); }

        [MenuItem("Mechaniki LD/Edytuj gracza", priority = 3)]
        static void EdytujGracza() { Otworz(Folder + "/Prefaby/Gracz/Gracz.prefab"); }

        [MenuItem("Mechaniki LD/Pokaż prefaby mechanik", priority = 20)]
        static void Prefaby() { Pokaz(Folder + "/Prefaby/Gracz"); }

        [MenuItem("Mechaniki LD/Dokumentacja", priority = 21)]
        static void Dokumentacja() { Pokaz(Folder + "/Dokumentacja/00_Start.md"); }

        [MenuItem("Mechaniki LD/Nowy poziom z szablonu...", priority = 40)]
        static void NowyPoziom()
        {
            string szablon = Folder + "/Sceny/00_Szablon.unity";
            if (!File.Exists(szablon))
            {
                EditorUtility.DisplayDialog("Mechaniki LD", "Nie znaleziono sceny szablonu:\n" + szablon, "OK");
                return;
            }

            string sciezka = EditorUtility.SaveFilePanelInProject("Nowy poziom z szablonu", "MojPoziom", "unity",
                "Wybierz nazwę i miejsce dla nowej sceny.", "Assets");
            if (string.IsNullOrEmpty(sciezka)) return;

            // najpierw pytamy o zapisanie zmian w aktualnie otwartej scenie
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (!AssetDatabase.CopyAsset(szablon, sciezka))
            {
                EditorUtility.DisplayDialog("Mechaniki LD", "Nie udało się utworzyć sceny:\n" + sciezka, "OK");
                return;
            }
            EditorSceneManager.OpenScene(sciezka);
        }

        // Zaznacza plik lub folder w oknie Project (i pokazuje go w Inspektorze).
        static void Pokaz(string sciezka)
        {
            Object obiekt = AssetDatabase.LoadAssetAtPath<Object>(sciezka);
            if (obiekt == null)
            {
                EditorUtility.DisplayDialog("Mechaniki LD", "Nie znaleziono:\n" + sciezka, "OK");
                return;
            }
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obiekt;
            EditorGUIUtility.PingObject(obiekt);
        }

        // Otwiera prefab do edycji.
        static void Otworz(string sciezka)
        {
            Object obiekt = AssetDatabase.LoadAssetAtPath<Object>(sciezka);
            if (obiekt == null)
            {
                EditorUtility.DisplayDialog("Mechaniki LD", "Nie znaleziono:\n" + sciezka, "OK");
                return;
            }
            AssetDatabase.OpenAsset(obiekt);
        }
    }
}
