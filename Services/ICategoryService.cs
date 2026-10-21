using ProductApi.DTOs;

namespace ProductApi.Services;

public interface ICategoryService
{
    List<CategoryResponse> GetAll();
    CategoryResponse Create(CategoryRequest request);
    CategoryResponse Update(int id, CategoryRequest request);
    void Delete(int id);
}
