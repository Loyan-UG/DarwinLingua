using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record CourseIndexPageViewModel(
    IReadOnlyList<CoursePathListItemModel> Courses,
    IReadOnlyList<string> CefrLevels,
    string? SelectedCefrLevel,
    string? Query,
    string PrimaryMeaningLanguageCode);

public sealed record CourseDetailPageViewModel(
    CoursePathDetailModel Course,
    string PrimaryMeaningLanguageCode);

public sealed record CourseLessonPageViewModel(
    CourseLessonDetailModel Lesson,
    UserContentProgressModel? Progress,
    string PrimaryMeaningLanguageCode);

public sealed record AdminCoursesPageViewModel(
    IReadOnlyList<CoursePathListItemModel> Courses);
