using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.DictionaryBuilder.Configuration;
using Umbraco.DictionaryBuilder.Models;
using Umbraco.DictionaryBuilder.Services;

namespace Umbraco.DictionaryBuilder.Building
{
    public class ModelsGenerator
    {
        private readonly IUmbracoService _umbracoService;
        private readonly IDictionaryBuilderConfiguration _config;

        public ModelsGenerator(IUmbracoService umbracoService, IDictionaryBuilderConfiguration config)
        {
            _umbracoService = umbracoService;
            _config = config;
        }

        internal void GenerateModels()
        {
            // Get the models from Umbraco
            DictionaryModel[] models = _umbracoService.GetDictionaryModels();
            if(models == null)
                // If no models, do nothing
                return;

            // Create directory if it doesn't exist
            if (!Directory.Exists(_config.DictionaryDirectory))
                Directory.CreateDirectory(_config.DictionaryDirectory);

            // Delete all generated.cs files
            List<string> generatedFiles = Directory.GetFiles(_config.DictionaryDirectory, "*.generated.cs").ToList();
            foreach (string file in generatedFiles)
                File.Delete(file);
            
            if (_config.GenerateFilePerDictionaryItem)
            {
                // Generate a file per model
                foreach (DictionaryModel model in models) 
                    GenerateModels(model.GetItemKey(), model);
            }
            else
            {
                // Generate a file containing all models
                GenerateModels(_config.DictionaryItemsPartialClassName, models);
            }
        }

        private void GenerateModels(string key, params DictionaryModel[] models)
        {
            // Generate the models code
            CodeBuilder builder = new CodeBuilder(_config);
            builder.Generate(models);
            
            // Save the file
            string filename = Path.Combine(_config.DictionaryDirectory, $"{key}.generated.cs");
            File.WriteAllText(filename, builder.ToString());
        }
    }
}