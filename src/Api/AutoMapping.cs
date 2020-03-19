using AutoMapper;

namespace Api
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<DataModels.Document, Client.Models.Document>();
            CreateMap<DataModels.Format, Client.Models.Format>();
            CreateMap<DataModels.Project, Client.Models.Project>();
            CreateMap<DataModels.Spooler, Client.Models.Spooler>()
                .ForMember(dest => dest.ProjectId, opts => opts.MapFrom(src => src.Zone.ProjectId));
            CreateMap<DataModels.Template, Client.Models.Template>();
            CreateMap<DataModels.Terminal, Client.Models.Terminal>()
                .ForMember(dest => dest.ProjectId, opts => opts.MapFrom(src => src.Zone.ProjectId));
            CreateMap<DataModels.TerminalRoute, Client.Models.Terminal.Route>();
            CreateMap<DataModels.Zone, Client.Models.Zone>();
            CreateMap<DataModels.ZoneRoute, Client.Models.Zone.Route>();
        }
    }
}
