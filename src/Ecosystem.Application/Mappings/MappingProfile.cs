using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Mappers;
using Ecosystem.Domain.Entities;

namespace Ecosystem.Application.Mappings;

/// <summary>
/// Hồ sơ cấu hình AutoMapper ánh xạ tự động giữa Domain Entities và DTOs (Bài tập 3 - Buổi 7).
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Taxonomy Hierarchy Maps
        CreateMap<Topic, TopicResponseDto>()
            .ForMember(dest => dest.AspectsCount, opt => opt.MapFrom(src => src.Aspects != null ? src.Aspects.Count : 0));

        CreateMap<Topic, TopicDetailResponseDto>()
            .IncludeBase<Topic, TopicResponseDto>()
            .ForMember(dest => dest.Aspects, opt => opt.MapFrom(src => src.Aspects));

        CreateMap<Aspect, AspectResponseDto>()
            .ForMember(dest => dest.CategoriesCount, opt => opt.MapFrom(src => src.Categories != null ? src.Categories.Count : 0));

        CreateMap<Aspect, AspectDetailResponseDto>()
            .IncludeBase<Aspect, AspectResponseDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));

        CreateMap<Category, CategoryResponseDto>();

        // Student Maps
        CreateMap<Student, StudentResponseDto>();
        CreateMap<StudentCreateDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<StudentUpdateDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // User Maps
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(_ => 25))
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore());

        // Article Maps (Flattening 4-tier Breadcrumbs & Tags JSON)
        CreateMap<Article, ArticleResponseDto>()
            .ForMember(dest => dest.CategoryTitle, opt => opt.MapFrom(src => src.Category != null ? src.Category.Title : string.Empty))
            .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src => src.Category != null ? src.Category.Slug : string.Empty))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null
                ? (!string.IsNullOrEmpty(src.Author.FullName) ? src.Author.FullName : src.Author.Username)
                : string.Empty))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => ArticleMappingExtensions.ParseTags(src.TagsJson)));

        CreateMap<Article, ArticleDetailResponseDto>()
            .IncludeBase<Article, ArticleResponseDto>()
            .ForMember(dest => dest.AspectId, opt => opt.MapFrom(src => src.Category != null ? src.Category.AspectId : Guid.Empty))
            .ForMember(dest => dest.AspectTitle, opt => opt.MapFrom(src => src.Category != null && src.Category.Aspect != null ? src.Category.Aspect.Title : string.Empty))
            .ForMember(dest => dest.AspectSlug, opt => opt.MapFrom(src => src.Category != null && src.Category.Aspect != null ? src.Category.Aspect.Slug : string.Empty))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.Category != null && src.Category.Aspect != null ? src.Category.Aspect.TopicId : Guid.Empty))
            .ForMember(dest => dest.TopicTitle, opt => opt.MapFrom(src => src.Category != null && src.Category.Aspect != null && src.Category.Aspect.Topic != null ? src.Category.Aspect.Topic.Title : string.Empty))
            .ForMember(dest => dest.TopicSlug, opt => opt.MapFrom(src => src.Category != null && src.Category.Aspect != null && src.Category.Aspect.Topic != null ? src.Category.Aspect.Topic.Slug : string.Empty));
    }
}
