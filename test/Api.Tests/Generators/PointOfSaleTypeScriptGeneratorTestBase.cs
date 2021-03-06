﻿using Api.Generators.JavaScript;
using Api.Generators.TypeScript;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Generic;
using System.IO;

namespace Api.Tests.Generators
{
    public abstract class PointOfSaleTypeScriptGeneratorTestBase<TJavaScriptGenerator, TTypeScriptTranspiler>
        where TJavaScriptGenerator: class, IJavaScriptGenerator
        where TTypeScriptTranspiler: class, ITypeScriptTranspiler
    {
        protected readonly TypeScriptGenerator _generator;
        protected readonly string _script;

        public PointOfSaleTypeScriptGeneratorTestBase(string scriptFileName)
        {
            var services = new ServiceCollection()
                .AddSingleton<IJavaScriptGenerator, TJavaScriptGenerator>()
                .AddSingleton<ITypeScriptTranspiler, TTypeScriptTranspiler>()
                .AddSingleton<TypeScriptGenerator>()
                .AddSingleton<ResourceScriptFactory>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddMemoryCache()
                .BuildServiceProvider();

            _generator = services.GetRequiredService<TypeScriptGenerator>();

            using var stream = GetType().Assembly.GetManifestResourceStream(GetType(), scriptFileName);
            using var reader = new StreamReader(stream);
            _script = reader.ReadToEnd();
        }
    }

    public class CaseModel
    {
        public int id { get; set; }
        public int packageId { get; set; }
        public int itemId { get; set; }
        public string createdByEmployeeName { get; set; }
        public string note { get; set; }
        public string arrived { get; set; }
        public string departed { get; set; }
    }

    public class KitchenModel
    {
        public bool isFood { get; set; }
        public CaseModel @case { get; set; }
        public string itemName { get; set; }
        public int sequenceNumber { get; set; }
        public string printedByEmployeeName { get; set; }
        public string printed { get; set; }
        public bool isCancellation { get; set; }
        public List<Line> added { get; set; } = new List<Line>();
        public List<LineDifference> changed { get; set; } = new List<LineDifference>();
        public List<Line> removed { get; set; } = new List<Line>();

        public class Line
        {
            public string name { get; set; }
            public decimal? quantity { get; set; }
            public string note { get; set; }
        }

        public class LineDifference
        {
            public Line baseline { get; set; }
            public Line target { get; set; }
        }
    }

    public class ReceiptModel
    {
        public string vendorDataResource { get; set; }
        public int slipId { get; set; }
        public CaseModel @case { get; set; }
        public List<Line> lines { get; set; } = new List<Line>();
        public string itemName { get; set; }
        public string printedByEmployeeName { get; set; }
        public decimal grossTotal { get; set; }
        public decimal discountTotal { get; set; }
        public decimal netTotal { get; set; }
        public decimal netTaxTotal { get; set; }
        public decimal netEuroTotal { get; set; }
        public string printed { get; set; }

        public class Line
        {
            public string name { get; set; }
            public decimal quantity { get; set; }
            public decimal unitPrice { get; set; }
            public decimal discount { get; set; }
        }
    }
}
