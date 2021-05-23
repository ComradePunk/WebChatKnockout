using Application.Models;
using AutoMapper;
using Domain.Entities;
using System;
using System.Linq;

namespace Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterUserViewModel, WebChatUser>();
            CreateMap<WebChatUser, WebChatUserViewModel>();
            CreateMap<WebChat, WebChatViewModel>()
                .ForMember(d => d.Users, opt => opt.MapFrom(s => s.Users.Select(u => u.User)));

            CreateMap<WebChatMessage, MessageViewModel>()
                .ForMember(d => d.SenderName, opt => opt.MapFrom(s => s.Sender != null ? s.Sender.UserName : null));

            CreateMap<SendMessageModel, WebChatMessage>()
                .ForMember(d => d.SentTime, opt => opt.MapFrom(s => DateTimeOffset.Now));
        }
    }
}
