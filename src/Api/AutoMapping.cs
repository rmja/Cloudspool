using AutoMapper;
using System.Collections.Generic;

namespace Api
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<DataModels.Document, Client.Models.Document>();
            CreateMap<DataModels.Format, Client.Models.Format>();
            CreateMap<DataModels.Project, Client.Models.Project>();
            CreateMap<DataModels.Resource, Client.Models.Resource>()
                .ForMember(dest => dest.ContentType, opts => opts.MapFrom(src => src.MediaType));
            CreateMap<DataModels.Spooler, Client.Models.Spooler>()
                .ForMember(dest => dest.ProjectId, opts => opts.MapFrom(src => src.Zone.ProjectId));
            CreateMap<DataModels.Template, Client.Models.Template>()
                .ForMember(dest => dest.ScriptContentType, opts => opts.MapFrom(src => src.ScriptMediaType));
            CreateMap<DataModels.Terminal, Client.Models.Terminal>()
                .ForMember(dest => dest.ProjectId, opts => opts.MapFrom(src => src.Zone.ProjectId))
                .ForMember(dest => dest.Routes, opts => opts.Ignore())
                .ForMember(dest => dest.RoutesList, opts => opts.MapFrom(src => src.Routes));
            CreateMap<DataModels.TerminalRoute, Client.Models.Terminal.Route>();
            // These mappings to dictionary does not work - we instead map to RoutesList which constructs the dictionary internally
            //CreateMap<DataModels.TerminalRoute, KeyValuePair<string, Client.Models.Terminal.Route>>()
            //    .ConvertUsing(route => new KeyValuePair<string, Client.Models.Terminal.Route>(route.Alias, new Client.Models.Terminal.Route() { TerminalId = route.TerminalId, Alias = route.Alias, SpoolerId = route.SpoolerId, PrinterName = route.PrinterName }));
            CreateMap<DataModels.Zone, Client.Models.Zone>()
                .ForMember(dest => dest.Routes, opts => opts.Ignore())
                .ForMember(dest => dest.RoutesList, opts => opts.MapFrom(src => src.Routes));
            CreateMap<DataModels.ZoneRoute, Client.Models.Zone.Route>();
            //CreateMap<DataModels.ZoneRoute, KeyValuePair<string, Client.Models.Zone.Route>>()
            //    .ConvertUsing(route => new KeyValuePair<string, Client.Models.Zone.Route>(route.Alias, new Client.Models.Zone.Route() { ZoneId = route.ZoneId, Alias = route.Alias, SpoolerId = route.SpoolerId, PrinterName = route.PrinterName }));
        }
    }
}
