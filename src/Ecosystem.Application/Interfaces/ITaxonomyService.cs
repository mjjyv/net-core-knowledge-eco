using Ecosystem.Application.Models;
using Ecosystem.Domain.Entities;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Giao diện hợp đồng quản lý CRUD và truy vấn Cây Tri Thức 3 cấp (Topic -> Aspect -> Category).
/// </summary>
public interface ITaxonomyService
{
    // Topics
    Task<IEnumerable<Topic>> GetAllTopicsAsync(TaxonomyFilterQuery? query = null);
    Task<Topic> GetTopicByIdAsync(Guid id);
    Task<Topic> GetTopicBySlugAsync(string slug);
    Task<Topic> CreateTopicAsync(CreateTopicRequest request);
    Task<Topic> UpdateTopicAsync(Guid id, UpdateTopicRequest request);
    Task DeleteTopicAsync(Guid id);

    // Aspects
    Task<IEnumerable<Aspect>> GetAspectsByTopicIdAsync(Guid topicId);
    Task<Aspect> GetAspectByIdAsync(Guid id);
    Task<Aspect> CreateAspectAsync(Guid topicId, CreateAspectRequest request);
    Task<Aspect> UpdateAspectAsync(Guid id, UpdateAspectRequest request);
    Task DeleteAspectAsync(Guid id);

    // Categories
    Task<IEnumerable<Category>> GetCategoriesByAspectIdAsync(Guid aspectId);
    Task<Category> GetCategoryByIdAsync(Guid id);
    Task<Category> CreateCategoryAsync(Guid aspectId, CreateCategoryRequest request);
    Task<Category> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(Guid id);
}
