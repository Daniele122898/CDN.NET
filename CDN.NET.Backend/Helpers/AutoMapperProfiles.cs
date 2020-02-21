using System;
using AutoMapper;
using CDN.NET.Backend.Dtos.AlbumDtos;
using CDN.NET.Backend.Dtos.AuthDtos;
using CDN.NET.Backend.Dtos.FileDtos;
using CDN.NET.Backend.Dtos.UploadDtos;
using CDN.NET.Backend.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CDN.NET.Backend.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            RegistrationMaps();
            FileMaps();
        }

        private void FileMaps()
        {
            CreateMap<UFile, FileDeleteReturnDto>();
            CreateMap<Album, AlbumsToReturnSparseDto>();
            CreateMap<Album, AlbumToReturnDto>()
                .ForMember(m => m.Files,
                    opt => opt.MapFrom(src => src.UFiles));
        }

        private void RegistrationMaps()
        {
            CreateMap<UserForRegisterDto, User>();
            CreateMap<User, UserDetailDto>();
        }
        
        public static void ConfigureAutoMapper(IServiceProvider services, IMapperConfigurationExpression mapper)
        {
            var constants = services.GetService<Constants>();
            // Upload Maps
            mapper.CreateMap<UFile, UFileReturnDto>()
                .ForMember(dest => dest.Url,
                    opt => opt.MapFrom(
                        src => $"{constants.BaseUrl}/file/{(src.IsPublic ? "" : "private/")}{src.PublicId}{src.FileExtension}"));
        }
        
        /*
         * CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.PhotoUrl, 
                    opt => opt.MapFrom(
                        src => src.Photos.FirstOrDefault(
                            p => p.IsMain).Url))
                .ForMember(dest => dest.Age,
                    opt => opt.MapFrom(
                        src => src.DateOfBirth.CalculateAge()));
            
            CreateMap<User, UserForDetailedDto>()
                .ForMember(dest => dest.PhotoUrl, 
                    opt => opt.MapFrom(
                        src => src.Photos.FirstOrDefault(
                            p => p.IsMain).Url))
                .ForMember(dest => dest.Age,
                    opt => opt.MapFrom(
                        src => src.DateOfBirth.CalculateAge()));
            
            CreateMap<Photo, PhotosForDetailedDto>();

            CreateMap<UserForUpdateDto, User>();

            CreateMap<Photo, PhotoForReturnDto>();

            CreateMap<PhotoForCreationDto, Photo>();

            CreateMap<UserForRegisterDto, User>();

            CreateMap<MessageForCreationDto, Message>().ReverseMap();

            CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, 
                    opt => opt.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, 
                    opt => opt.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));
         */
    }
}