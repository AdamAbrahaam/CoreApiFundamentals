using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            //Auto Mapping, maps SAME NAME! 
            this.CreateMap<Camp, CampModel>()

                //For mapping another model in model(public Location Location)
                .ForMember(c => c.Address2, o => o.MapFrom( m => m.Location.Address2))
                .ForMember(c => c.Address3, o => o.MapFrom( m => m.Location.Address3))
                .ForMember(c => c.VenueName, o => o.MapFrom( m => m.Location.VenueName))
                .ForMember(c => c.CityTown, o => o.MapFrom( m => m.Location.CityTown))
                .ForMember(c => c.StateProvince, o => o.MapFrom( m => m.Location.StateProvince))
                .ForMember(c => c.PostalCode, o => o.MapFrom( m => m.Location.PostalCode))
                .ForMember(c => c.Country, o => o.MapFrom( m => m.Location.Country))
                .ForMember(c => c.Address1, o => o.MapFrom( m => m.Location.Address1))
                
                // Mapping from model back to entity
                .ReverseMap();

            this.CreateMap<Talk, TalkModel>()
                .ReverseMap();

            this.CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();
        }
    }
}
