using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record CourseIndexPageViewModel(
    IReadOnlyList<CoursePathListItemModel> Courses,
    IReadOnlyList<string> CefrLevels,
    string? SelectedCefrLevel,
    string? Query);

public sealed record CourseDetailPageViewModel(
    CoursePathDetailModel Course);

public sealed record CourseLessonPageViewModel(
    CourseLessonDetailModel Lesson,
    UserContentProgressModel? Progress);

public sealed record AdminCoursesPageViewModel(
    IReadOnlyList<CoursePathListItemModel> Courses);
