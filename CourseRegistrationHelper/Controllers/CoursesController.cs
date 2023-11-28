using CourseRegistrationHelper.Models;
using CourseRegistrationHelper.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CourseRegistrationHelper.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays the initial search form
        public IActionResult Search()
        {
            var model = new CourseSearchViewModel
            {
                Universities = _context.Universities
                                       .Select(u => new SelectListItem { Value = u.UniversityId.ToString(), Text = u.Name })
                                       .ToList()
            };

            return View(model);
        }

        public IActionResult SuggestedSchedules()
        {
            var draftSections = GetDraftSectionsFromSession();
            var nonOverlappingSchedules = GenerateNonOverlappingSchedules(draftSections);
            return View(nonOverlappingSchedules);
        }

        // Handle the search logic when user applies filters
        [HttpPost]
        public IActionResult Search(CourseSearchViewModel searchModel)
        {
            // Logic to filter sections based on selected values
            searchModel.Sections = GetFilteredSections(searchModel.SelectedUniversityId, searchModel.SelectedCollegeId, searchModel.SelectedDepartmentId, searchModel.SelectedCourseId);

            // Repopulate dropdown lists and retain the selected value
            searchModel.Universities = _context.Universities
                .Select(u => new SelectListItem { Value = u.UniversityId.ToString(), Text = u.Name, Selected = u.UniversityId == searchModel.SelectedUniversityId })
                .ToList();
            searchModel.Colleges = _context.Colleges
                .Where(c => c.UniversityId == searchModel.SelectedUniversityId)
                .Select(c => new SelectListItem { Value = c.CollegeId.ToString(), Text = c.Name, Selected = c.CollegeId == searchModel.SelectedCollegeId })
                .ToList();
            searchModel.Departments = _context.Departments
                .Where(d => d.CollegeId == searchModel.SelectedCollegeId)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name, Selected = d.DepartmentId == searchModel.SelectedDepartmentId })
                .ToList();
            searchModel.Courses = _context.Courses
                .Where(c => c.DepartmentId == searchModel.SelectedDepartmentId)
                .Select(c => new SelectListItem { Value = c.CourseId.ToString(), Text = c.Title, Selected = c.CourseId == searchModel.SelectedCourseId })
                .ToList();

            return View(searchModel);
        }

        private List<SectionViewModel> GetFilteredSections(int? universityId, int? collegeId, int? departmentId, int? courseId)
        {
            var query = _context.Sections.AsQueryable();

            // Apply filters
            if (universityId.HasValue)
            {
                query = query.Where(s => s.Course.Department.College.UniversityId == universityId.Value);
            }
            if (collegeId.HasValue)
            {
                query = query.Where(s => s.Course.Department.CollegeId == collegeId.Value);
            }
            if (departmentId.HasValue)
            {
                query = query.Where(s => s.Course.DepartmentId == departmentId.Value);
            }
            if (courseId.HasValue)
            {
                query = query.Where(s => s.CourseId == courseId.Value);
            }
            // Apply other filters for college, department, course similarly

            // Project the query to a list of SectionViewModel
            return query.Select(s => new SectionViewModel
            {
                CourseName = s.Course.Title,
                CourseSymbol = s.Course.Code,
                CreditHours = s.Course.CreditHours,
                SectionId = s.SectionId,
                CRN = s.CRN,
                Capacity = s.Capacity,
                Location = s.Location,
                Room = s.Room,
                Days = s.Days,
                Time = $"{s.StartTime.ToString(@"hh\:mm")} - {s.EndTime.ToString(@"hh\:mm")}", // Format the time here
                Instructor = s.Instructor.Name,

                EnrolledStudents = s.EnrolledStudents,
                // ... other mappings
            }).ToList();
        }
        public static bool SectionsOverlap(SectionViewModel section1, SectionViewModel section2)
        {
            var days1 = section1.Days.Split(',').Select(int.Parse).ToList();
            var days2 = section2.Days.Split(',').Select(int.Parse).ToList();
            bool daysOverlap = days1.Intersect(days2).Any();
            bool timesOverlap = section1.StartTime < section2.EndTime && section2.StartTime < section1.EndTime;

            return daysOverlap && timesOverlap;
        }
        public static List<List<T>> GetAllCombinations<T>(List<T> list)
        {
            var combinations = new List<List<T>>();
            var combination = new List<T>();

            void GenerateCombinations(int startIndex)
            {
                for (int i = startIndex; i < list.Count; i++)
                {
                    combination.Add(list[i]);
                    combinations.Add(new List<T>(combination));
                    GenerateCombinations(i + 1);
                    combination.RemoveAt(combination.Count - 1);
                }
            }

            GenerateCombinations(0);

            return combinations;
        }
        public List<List<SectionViewModel>> GenerateNonOverlappingSchedules(List<SectionViewModel> draftSections)
        {
            var allCombinations = GetAllCombinations(draftSections);
            var nonOverlappingSchedules = new List<List<SectionViewModel>>();

            foreach (var combination in allCombinations)
            {
                bool hasOverlap = false;

                for (int i = 0; i < combination.Count; i++)
                {
                    for (int j = i + 1; j < combination.Count; j++)
                    {
                        if (SectionsOverlap(combination[i], combination[j]))
                        {
                            hasOverlap = true;
                            break;
                        }
                    }
                    if (hasOverlap) break;
                }

                if (!hasOverlap)
                {
                    nonOverlappingSchedules.Add(combination);
                }
            }

            return nonOverlappingSchedules;
        }
        [HttpPost]
        public IActionResult AddToDraft(CourseSearchViewModel model, string action)
        {
            // If adding sections to the draft...
            if (action == "AddToDraft")
            {
                // Get selected sections and merge with existing draft
                var selectedSections = model.Sections.Where(s => s.IsSelected).ToList();
                var draftSections = GetDraftSectionsFromSession();
                var updatedDraft = draftSections.Union(selectedSections, new SectionViewModelComparer()).ToList();
                SaveDraftSectionsToSession(updatedDraft);

                // Rebuild the model to reflect current state and return to the Search view
                var updatedModel = PrepareSearchViewModel(model);
                return View("Search", updatedModel);
            }
            else if (action == "GenerateSchedule")
            {
                // Proceed to generate non-overlapping schedules
                return RedirectToAction("SuggestedSchedules");
            }

            // Default return (could also handle other actions)
            return View();
        }

        private CourseSearchViewModel PrepareSearchViewModel(CourseSearchViewModel originalModel)
        {
            var model = new CourseSearchViewModel
            {
                Universities = _context.Universities
                                       .Select(u => new SelectListItem
                                       {
                                           Value = u.UniversityId.ToString(),
                                           Text = u.Name,
                                           Selected = u.UniversityId == originalModel.SelectedUniversityId
                                       })
                                       .ToList(),
                // Repeat for other dropdowns...
                Colleges = originalModel.SelectedUniversityId.HasValue ?
                   _context.Colleges
                           .Where(c => c.UniversityId == originalModel.SelectedUniversityId.Value)
                           .Select(c => new SelectListItem
                           {
                               Value = c.CollegeId.ToString(),
                               Text = c.Name,
                               Selected = c.CollegeId == originalModel.SelectedCollegeId
                           })
                           .ToList() : new List<SelectListItem>(),
                Departments = originalModel.SelectedCollegeId.HasValue ?
                      _context.Departments
                              .Where(d => d.CollegeId == originalModel.SelectedCollegeId.Value)
                              .Select(d => new SelectListItem
                              {
                                  Value = d.DepartmentId.ToString(),
                                  Text = d.Name,
                                  Selected = d.DepartmentId == originalModel.SelectedDepartmentId
                              })
                              .ToList() : new List<SelectListItem>(),
                Courses = originalModel.SelectedDepartmentId.HasValue ?
                  _context.Courses
                          .Where(c => c.DepartmentId == originalModel.SelectedDepartmentId.Value)
                          .Select(c => new SelectListItem
                          {
                              Value = c.CourseId.ToString(),
                              Text = c.Title,
                              Selected = c.CourseId == originalModel.SelectedCourseId
                          })
                          .ToList() : new List<SelectListItem>(),

                Sections = GetFilteredSections(originalModel.SelectedUniversityId, originalModel.SelectedCollegeId, originalModel.SelectedDepartmentId, originalModel.SelectedCourseId),

                // Ensure the selected values are carried over to maintain state
                SelectedUniversityId = originalModel.SelectedUniversityId,
                SelectedCollegeId = originalModel.SelectedCollegeId,
                SelectedDepartmentId = originalModel.SelectedDepartmentId,
                SelectedCourseId = originalModel.SelectedCourseId
            };

            // Add the previously selected sections to the model
            model.Sections.AddRange(GetDraftSectionsFromSession());

            return model;
        }

        private void SaveDraftSectionsToSession(List<SectionViewModel> draftSections)
        {
            // Combine existing draft sections with new selections, avoiding duplicates
            var currentDraft = GetDraftSectionsFromSession() ?? new List<SectionViewModel>();
            var updatedDraft = currentDraft.Union(draftSections, new SectionViewModelComparer()).ToList();

            var draftSectionsBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(updatedDraft);
            HttpContext.Session.Set("DraftSections", draftSectionsBytes);
        }

        // Implement an equality comparer for SectionViewModel if needed
        public class SectionViewModelComparer : IEqualityComparer<SectionViewModel>
        {
            public bool Equals(SectionViewModel x, SectionViewModel y)
            {
                return x.SectionId == y.SectionId;
            }

            public int GetHashCode(SectionViewModel obj)
            {
                return obj.SectionId.GetHashCode();
            }
        }

        private List<SectionViewModel> GetDraftSectionsFromSession()
        {
            // Check if the session contains any draft sections
            if (HttpContext.Session.TryGetValue("DraftSections", out byte[] draftSectionsBytes))
            {
                // Deserialize the session data back into a List<SectionViewModel>
                var draftSections = System.Text.Json.JsonSerializer.Deserialize<List<SectionViewModel>>(draftSectionsBytes);
                return draftSections ?? new List<SectionViewModel>(); // Return the deserialized list or a new list if null
            }
            else
            {
                // If the session doesn't have draft sections, return an empty list
                return new List<SectionViewModel>();
            }
        }

        //public IActionResult GenerateSchedule()
        //{
        //    // Retrieve the user's draft sections from the session or database
        //    List<SectionViewModel> draftSections = GetDraftSectionsFromSession();

        //    // Proceed to generate non-overlapping schedules
        //    List<List<SectionViewModel>> nonOverlappingSchedules = GenerateNonOverlappingSchedules(draftSections);

        //    // Pass the non-overlapping schedules to the view
        //    return View("SuggestedSchedules", nonOverlappingSchedules);
        //}
        public IActionResult GenerateSchedule()
        {
            var draftSections = GetDraftSectionsFromSession();
            if (draftSections == null || !draftSections.Any())
            {
                // Handle the case where there are no draft sections
                // Redirect to an error page or show a message
                return RedirectToAction("Search");
            }

            var nonOverlappingSchedules = GenerateNonOverlappingSchedules(draftSections);
            return View("SuggestedSchedules", nonOverlappingSchedules);
        }

        // New action to get colleges based on university selection
        [HttpGet]
        public IActionResult GetColleges(int universityId)
        {
            var colleges = _context.Colleges
                .Where(c => c.UniversityId == universityId)
                .Select(c => new { c.CollegeId, c.Name })
                .ToList();

            return Json(colleges);
        }

        // New action to get departments based on college selection
        [HttpGet]
        public IActionResult GetDepartments(int collegeId)
        {
            var departments = _context.Departments
                .Where(d => d.CollegeId == collegeId)
                .Select(d => new { d.DepartmentId, d.Name })
                .ToList();

            return Json(departments);
        }

        // New action to get courses based on department selection
        [HttpGet]
        public IActionResult GetCourses(int departmentId)
        {
            var courses = _context.Courses
                .Where(c => c.DepartmentId == departmentId)
                .Select(c => new { c.CourseId, c.Code, c.Title })
                .ToList();

            return Json(courses);
        }
    }
}
