using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems.ModLocalization;
using LanguageLoaderPlugin.SimpleJson;

namespace LanguageLoaderPlugin
{
    [BepInPlugin(GUID, DISPNAME, VERSION)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [CommonAPISubmoduleDependency(nameof(LocalizationModule))]
    public class LoaderPlugin : BaseUnityPlugin
    {
        public const string GUID = "org.kremnev8.LanguageLoaderPlugin";
        public const string DISPNAME = "Language Loader Plugin";
        public const string VERSION = "1.0.0";

        public static ManualLogSource logger;
        
        private void Awake()
        {
            // Plugin startup logic
            logger = Logger;

            Assembly assembly = Assembly.GetExecutingAssembly();
            
            string currentDir = Path.GetDirectoryName(assembly.Location);
            string pluginsDir = Directory.GetParent(currentDir).FullName;
            
            LoadLanguages(pluginsDir, assembly);
            
            logger.LogInfo($"Finished loading all JSON mods");
        }
        
        private static void LoadLanguages(string pluginsDir, Assembly assembly)
        {
            foreach (string directory in Directory.EnumerateDirectories(pluginsDir))
            {
                string myAssemblyPath = Path.Combine(directory, $"{assembly.GetName().Name}.dll");
                if (!File.Exists(myAssemblyPath)) continue;
                
                try
                {
                    LoadDirectory(directory);
                }
                catch (Exception e)
                {
                    logger.LogError($"Exception while loading folder {directory} language data!\n{e}");
                }
            }
        }


        internal static void LoadDirectory(string directory)
        {
            string languageMetadata = Path.Combine(directory, "language.tsv");
            if (!File.Exists(languageMetadata))
            {
                logger.LogError($"Failed to load language folder {directory}, because manifest file is missing!");
                return;
            }
            
            string text = File.ReadAllText(languageMetadata);
            string[] data = text.Split('\t');

            //English,enUS,en,2052,0        
            
            string fallback = data[3];
            string font = data[4];
            string glyphName = data[5];

            Localization.Language language = new Localization.Language()
            {
                name = data[0],
                abbr = data[1],
                abbr2 = data[2],
            };

            if (int.TryParse(fallback, out int fallbackId))
            {
                language.fallback = fallbackId;
            }
            else
            {
                fallbackId = LocalizationModule.GetLanguageId(fallback);
                if (fallbackId == 0) fallbackId = Localization.LCID_ENUS;

                language.fallback = fallbackId;
            }

            if (Enum.TryParse<Localization.EGlyph>(glyphName, true, out var glyph))
            {
                language.glyph = glyph;
            }

            int languageId = LocalizationModule.AddLanguage(language);

            if (!string.IsNullOrEmpty(font))
            {
                if (font == "$bundle")
                {
                    string bundlePath = Path.Combine(directory, "font_bundle");
                    LocalizationModule.RegisterFontForLanguageFromBundle(languageId, bundlePath);
                }
                else
                {
                    LocalizationModule.RegisterFontForLanguage(languageId, font);
                }
            }

            string localePath = Path.Combine(directory, "Locale");
            LocalizationModule.LoadTranslationsFromFolder(localePath);

            logger.LogInfo($"Done adding custom language {language.name}!");
        }
    }
}