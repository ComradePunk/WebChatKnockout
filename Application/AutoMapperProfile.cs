using Application.Models;
using AutoMapper;
using Domain.Entities;

namespace Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterUserViewModel, WebChatUser>();
            CreateMap<WebChatUser, WebChatUserViewModel>();
        }
    }
}
