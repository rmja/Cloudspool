using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using System.IO;
using System.Threading.Tasks;

namespace Api.Generators.JavaScript.V8
{
    public class CustomLoader : DocumentLoader
    {
        private readonly DocumentLoader _defaultLoader;
        private readonly IResourceManager _resourceManager;
        private readonly ResourceScriptFactory _resourceScriptFactory;

        public CustomLoader(DocumentLoader defaultLoader, ResourceScriptFactory resourceScriptFactory, IResourceManager resourceManager)
        {
            _defaultLoader = defaultLoader;
            _resourceManager = resourceManager;
            _resourceScriptFactory = resourceScriptFactory;
        }

        public override Document LoadDocument(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            var alias = ResourceModuleUtils.GetResourceAlias(specifier);

            if (alias is object)
            {
                var resource = _resourceManager.GetResource(alias);

                if (resource is object)
                {
                    var script = _resourceScriptFactory.CreateFromExtension(resource, Path.GetExtension(alias));

                    if (script is object)
                    {
                        return new StringDocument(new DocumentInfo(specifier) { Category = ModuleCategory.Standard }, script);
                    }
                }
            }

            return _defaultLoader.LoadDocument(settings, sourceInfo, specifier, category, contextCallback);
        }

        public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            var alias = ResourceModuleUtils.GetResourceAlias(specifier);

            if (alias is object)
            {
                var resource = await _resourceManager.GetResourceAsync(alias);

                if (resource is object)
                {
                    var script = _resourceScriptFactory.CreateFromExtension(resource, Path.GetExtension(alias));

                    if (script is object)
                    {
                        return new StringDocument(new DocumentInfo(specifier) { Category = ModuleCategory.Standard }, script);
                    }
                }
            }

            return await _defaultLoader.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback);
        }
    }
}
