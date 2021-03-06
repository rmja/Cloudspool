﻿using Api.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api.Tests
{
    // To avoid ID collisions we use negative IDs for seeding data, see
    // - https://github.com/npgsql/efcore.pg/issues/367, and
    // - http://www.npgsql.org/efcore/modeling/generated-properties.html#identity-sequence-options
    public static partial class SeedData
    {
        public static readonly Guid TestProjectKey = Guid.Parse("893530ba-1e5c-423b-b154-fe79b2ef7121");
        public static readonly Guid TestSpoolerKey = Guid.Parse("49668ec2-2bac-4a17-a7ae-901ea997e18a");
        public static readonly Guid TestTerminalKey = Guid.Parse("8de343f2-6387-4b06-a4fa-456f0d19e55b");

        public static Project TestProject => GetProjects().First(x => x.Key == TestProjectKey);

        public static IEnumerable<Project> GetProjects()
        {
            var id = -1;

            yield return new Project() { Id = id--, Name = "TEST", Key = TestProjectKey };
        }

        public static IEnumerable<Template> GetTemplates()
        {
            var id = -1;

            yield return new Template(projectId: -1, "Receipt", @"
export default class Builder {
    build(model) {
        return 'name: ' + model.name + ', json: ' + JSON.stringify(model);
    }
}", "application/javascript") { Id = id-- };
        }

        public static IEnumerable<Resource> GetResources()
        {
            var id = -1;

            yield return new Resource(projectId: -1, alias: "my-resource.json", Encoding.UTF8.GetBytes("\"a string\""), "application/json") { Id = id-- };
        }

        public static IEnumerable<Document> GetDocuments()
        {
            var id = -1;

            yield return new Document(projectId: -1, content: Encoding.UTF8.GetBytes("ESC/POS"), contentType: "application/escpos") { Id = id-- };
        }

        public static IEnumerable<Spooler> GetSpoolers()
        {
            var id = -1;

            yield return new Spooler(zoneId: -1, "Test Spooler") { Id = id--, Key = TestSpoolerKey };
        }

        public static IEnumerable<Zone> GetZones()
        {
            var id = -1;

            yield return new Zone(projectId: -1, "Test Zone") { Id = id--, Routes = { new ZoneRoute("RouteAlias") { SpoolerId = -1, PrinterName = "Test Printer" } } };
        }

        public static IEnumerable<Terminal> GetTerminals()
        {
            var id = -1;

            yield return new Terminal(zoneId: -1, "Test Terminal") { Id = id--, Key = TestTerminalKey, Routes = { new TerminalRoute("RouteAlias") { SpoolerId = -1, PrinterName = "Test Printer" } } };
        }

        public static IEnumerable<Format> GetFormats()
        {
            var id = -1;

            yield return new Format(zoneId: -1, alias: "slip", templateId: -1) { Id = id-- };
        }

        public static void Populate(CloudspoolContext db)
        {
            db.Projects.AddRange(GetProjects());
            db.Templates.AddRange(GetTemplates());
            db.Resources.AddRange(GetResources());
            db.Documents.AddRange(GetDocuments());
            db.Spoolers.AddRange(GetSpoolers());
            db.Zones.AddRange(GetZones());
            db.Terminals.AddRange(GetTerminals());
            db.Formats.AddRange(GetFormats());

            db.SaveChanges();
        }
    }
}
