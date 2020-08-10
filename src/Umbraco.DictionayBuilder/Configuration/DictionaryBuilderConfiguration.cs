using System;
using System.Configuration;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace Umbraco.DictionaryBuilder.Configuration
{
    public class DictionaryBuilderConfiguration : IDictionaryBuilderConfiguration
    {
        private const string Prefix = "Umbraco.DictionaryBuilder.";
        private const string DefaultDictionaryNamespace = "Umbraco.Web";
        private const string DefaultDictionaryItemsPartialClassName = "Dictionaries";
        private const string DefaultDictionaryDirectory = "~/App_Data/Dictionaries";

        public ModelsMode ModelsMode { get; }
        public string DictionaryNamespace { get; }
        public string DictionaryItemsPartialClassName { get; }
        public string DictionaryDirectory { get; }
        public bool AcceptUnsafeModelsDirectory { get; }
        public bool UseNestedStructure { get; }
        public bool GenerateFilePerDictionaryItem { get; }
        public bool Enable { get; }

        public DictionaryBuilderConfiguration()
        {
            // giant kill switch, default: false
            // must be explicitely set to true for anything else to happen
            Enable = ConfigurationManager.AppSettings[Prefix + "Enable"] == "true";

            // ensure defaults are initialized for tests
            ModelsMode = ModelsMode.LiveAppData;
            DictionaryNamespace = DefaultDictionaryNamespace;
            DictionaryItemsPartialClassName = DefaultDictionaryItemsPartialClassName;
            DictionaryDirectory = IOHelper.MapPath(DefaultDictionaryDirectory);

            // stop here, everything is false
            if (!Enable) return;

            // default: false
            AcceptUnsafeModelsDirectory = ConfigurationManager.AppSettings[Prefix + "AcceptUnsafeModelsDirectory"].InvariantEquals("true");
            UseNestedStructure = ConfigurationManager.AppSettings[Prefix + "UseNestedStructure"].InvariantEquals("true");
            GenerateFilePerDictionaryItem = ConfigurationManager.AppSettings[Prefix + "GenerateFilePerDictionaryItem"].InvariantEquals("true");

            // default: initialized above with DefaultDictionaryNamespace const
            string value = ConfigurationManager.AppSettings[Prefix + "ModelsMode"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                if(!Enum.TryParse(value, true, out ModelsMode modelsMode))
                    throw new ConfigurationErrorsException($"Invalid model mode \"{value}\".");
                ModelsMode = modelsMode;
            }

            // default: initialized above with DefaultDictionaryNamespace const
            value = ConfigurationManager.AppSettings[Prefix + "ModelsNamespace"];
            if (!string.IsNullOrWhiteSpace(value))
                DictionaryNamespace = value;

            // default: initialized above with DefaultDictionaryItemsPartialClassName const
            value = ConfigurationManager.AppSettings[Prefix + "DictionaryItemsPartialClassName"];
            if (!string.IsNullOrWhiteSpace(value))
                DictionaryItemsPartialClassName = value;

            // default: initialized above with DefaultDictionaryDirectory const
            value = ConfigurationManager.AppSettings[Prefix + "ModelsDirectory"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                var root = IOHelper.MapPath("~/");
                if (root == null)
                    throw new ConfigurationErrorsException("Could not determine root directory.");

                // GetModelsDirectory will ensure that the path is safe
                DictionaryDirectory = GetModelsDirectory(root, value, AcceptUnsafeModelsDirectory);
            }

            //value = ConfigurationManager.AppSettings[Prefix + "HttpHandlerUrl"];
            //if (!string.IsNullOrWhiteSpace(value))
            //    DictionaryHttpHandlerUrl = value;

            //value = ConfigurationManager.AppSettings[Prefix + "DictionaryClientCache"];
            //if (!int.TryParse(value, out int clientCache))
            //    DictionaryClientCache = clientCache;
        }
        
        // internal for tests
        internal static string GetModelsDirectory(string root, string config, bool acceptUnsafe)
        {
            // making sure it is safe, ie under the website root,
            // unless AcceptUnsafeModelsDirectory and then everything is OK.

            if (!Path.IsPathRooted(root))
                throw new ConfigurationErrorsException($"Root is not rooted \"{root}\".");

            if (config.StartsWith("~/"))
            {
                string dir = Path.Combine(root, config.TrimStart("~/"));

                // sanitize - GetFullPath will take care of any relative
                // segments in path, eg '../../foo.tmp' - it may throw a SecurityException
                // if the combined path reaches illegal parts of the filesystem
                dir = Path.GetFullPath(dir);
                root = Path.GetFullPath(root);

                if (!dir.StartsWith(root) && !acceptUnsafe)
                    throw new ConfigurationErrorsException($"Invalid models directory \"{config}\".");

                return dir;
            }

            if (acceptUnsafe)
                return Path.GetFullPath(config);

            throw new ConfigurationErrorsException($"Invalid models directory \"{config}\".");
        }
    }
}