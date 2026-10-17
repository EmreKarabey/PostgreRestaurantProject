using Amazon;
using AutoMapper;
using CofeeAdmin.Dto.AppRole;
using CofeeAdmin.Dto.AppUser;
using CofeeAdmin.Dto.Category;
using CofeeAdmin.Dto.Product;
using CofeeAdmin.Dto.Reservation;
using CofeeAdmin.Dto.RestaurantTable;
using EntityLayer.Entities;

namespace CofeeAdmin.Mapping
{
    public class AutoMapping : AutoMapper.Profile
    {
        public AutoMapping()
        {
            CreateMap<AddCategoryDto, Category>()
                .ForMember(dest => dest.ImageURL, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UpdateCategoryDto, Category>()
            .ReverseMap();

            CreateMap<AddProductDto, Product>()
              .ForMember(dest => dest.ImageURL, opt => opt.Ignore())
              .ReverseMap();

            CreateMap<UpdateProductDto, Product>()
              .ReverseMap();


            CreateMap<AddUserDto, AppUser>()
             .ForMember(dest => dest.ProfileImageURL, opt => opt.Ignore())
             .ReverseMap();

            CreateMap<UpdateUserDto, AppUser>()
             .ForMember(dest => dest.ProfileImageURL, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ProfileImageURL)))
             .ReverseMap();

            CreateMap<AddRoleDto, AppRole>()
             .ReverseMap();

            CreateMap<UpdateRoleDto, AppRole>()
             .ReverseMap();

            CreateMap<AddReservationDto, Reservation>()
             .ReverseMap();

            CreateMap<UpdateReservationDto, Reservation>()
             .ReverseMap();

            CreateMap<CategoryListDto, Category>()
            .ReverseMap();

            CreateMap<AddRestaurantTableDto, RestaurantTable>()
            .ReverseMap();

            CreateMap<SaveRestaurantTableLayoutDto, RestaurantTable>();
        }
    }
}
