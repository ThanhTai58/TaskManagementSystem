#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem_FinalProject.Data;
using TaskManagementSystem_FinalProject.Models;

namespace TaskManagementSystem_FinalProject.Controllers
{
    [Authorize(Roles ="ProjectManager")]
    public class ProjectHelperController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectHelperController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProjectHelper
        public async Task<IActionResult> Index(bool ishideComplete,Priority priority)
        {
            
            var projects = await _context.Project.Include(p=>p.AppTasks).ThenInclude(a=>a.AppUser)
                                                 .Include(p=>p.Notifications)
                                                 .Include(p=>p.ProejectAndUsers).ThenInclude(pu=>pu.AppUser)
                                                 .ToListAsync();



            //When ishideComplete is true, hide tasks with completepercentage is 100
            foreach (var project in projects)
            {
                if (ishideComplete == true)
                {
                    ViewBag.IsHideComplete = ishideComplete;
                    project.AppTasks = project.AppTasks.OrderByDescending(a=>a.CompletePercentage)
                                                       .Where(a=>a.CompletePercentage < 100)
                                                       .ToList();    
                }else
                {
                    ViewBag.IsHideComplete = ishideComplete;
                    project.AppTasks = project.AppTasks.OrderByDescending(a => a.CompletePercentage)
                                                       .ToList();
                }
            }

            //implement priority And pick list
            if (priority == Priority.Newest)
            {
                ViewBag.Priority = priority;
                projects = projects.OrderByDescending(p => p.StartDate).ToList();
            }
            else if (priority == Priority.Budget)
            {
                ViewBag.Priority = priority;
                projects = projects.OrderByDescending(p => p.Budget).ToList();
            }
            else if (priority == Priority.DeadLine)
            {
                ViewBag.Priority = priority;
                projects = projects.OrderBy(p => p.DeadLine).ToList();
            }

            else if (priority == Priority.ExceededCost)
            {
                ViewBag.Priority = priority;
                var exceededCostProjects = projects.Where(p =>
                {
                    var totalCost = p.TotalCost(); // Lấy tổng chi phí của dự án
                    return p.Budget < totalCost; // Chỉ lấy các dự án vượt quá ngân sách
                }).ToList();

                projects = exceededCostProjects;
            }


            else if (priority == Priority.Finish)
            {
                ViewBag.Priority = priority;
                var temp = new List<Project>();
                foreach (var p in projects)
                {
                    if (!p.AppTasks.All(a=>a.CompletePercentage==100))
                    {
                        temp.Add(p);
                    }
                }
                foreach (var p in temp)
                {
                    projects.Remove(p);
                }
            }
            else if (priority == null)
            {
               // ViewBag.Priority = priority;
                projects = projects.OrderBy(p => p.DeadLine).ToList();
            }

            var PriorityList = new Priority();
            int numOfNoticefromProject = 0;
            foreach (var project in projects)
            {
                numOfNoticefromProject += project.Notifications.Where(n=>n.Isopen==false).Count();
            }
            ViewBag.NumOfNotice = numOfNoticefromProject;
            
            
           


            var viewModel = new ViewModel(PriorityList,projects);
            return View(viewModel);
        }

        public IActionResult Notification(int nNumber)
        {
            var notices = _context.Notification.Include(n=>n.Project).Include(n=>n.AppUser)
                                               .Where(n => n.AppUserId != null || n.ProjectId!=null)
                                               .OrderBy(n=>n.Isopen)
                                               .ToList();
            ViewBag.NumOfNotice = nNumber;
            return View(notices);
        }
        [HttpPost]
        public IActionResult Notification(int id, bool isOpen,int nNumber)
        {
            try
            {

                var notification = _context.Notification.First(n => n.Id == id);
                notification.Isopen = isOpen;
                _context.Update(notification);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            int numOfNoticefromProject = 0;
            foreach (var project in _context.Project)
            {
                numOfNoticefromProject += project.Notifications.Where(n => n.Isopen == false).Count();
            }
            ViewBag.NumOfNotice = numOfNoticefromProject;

            return RedirectToAction("Notification", new {nNumber=(nNumber-1)});
        }
        
        public IActionResult CreateNotificationFromProject(int id)
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateNotificationFromProject()
        {
            try
            {

                //thông báo về thời hạn đã qua với các nhiệm vụ chưa hoàn thành của dự án
                var today = DateTime.Now.Date;
                var allprojectwitTasks = _context.Project.Include(p => p.AppTasks)
                                                         .Include(p => p.Notifications);
                foreach (var project in allprojectwitTasks)
                {
                    var IsAllcompletedProject = project.AppTasks.Select(a => a.CompletePercentage).All(c => c == 100);
                    var newNotice = new Notification();
                    var descriptions = project.Notifications.Select(n => n.Description);
                    if ((project.DeadLine - today).Days < 0 && IsAllcompletedProject == false)
                    {
                        newNotice.ProjectId = project.Id;
                        newNotice.Description = $"{project.Name} deadline was passed with some incomplete tasks";
                        if (!descriptions.Contains(newNotice.Description))
                            _context.Notification.Add(newNotice);
                    }

                }

                //tạo thông báo với nhiệm vụ hoặc dự án đã hoàn thành
                var alltasks = _context.AppTask;
                foreach (var task in alltasks)
                {

                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return RedirectToAction("Index");
        }
         
        // GET: ProjectHelper/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projectstest/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Budget,StartDate,DeadLine")] Project project)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Chuyển đổi ngày tháng sang định dạng UTC
                    if (project.StartDate.Kind != DateTimeKind.Utc)
                    {
                        project.StartDate = project.StartDate.ToUniversalTime();
                    }
                    if (project.DeadLine.Kind != DateTimeKind.Utc)
                    {
                        project.DeadLine = project.DeadLine.ToUniversalTime();
                    }

                    _context.Add(project);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return View(project);
        }


        // GET: ProjectHelper/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: ProjectHelper/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Budget,StartDate,DeadLine")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Convert StartDate and DeadLine to UTC if they are not already
                    if (project.StartDate.Kind != DateTimeKind.Utc)
                    {
                        project.StartDate = project.StartDate.ToUniversalTime();
                    }
                    if (project.DeadLine.Kind != DateTimeKind.Utc)
                    {
                        project.DeadLine = project.DeadLine.ToUniversalTime();
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }


        // GET: ProjectHelper/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: ProjectHelper/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var project = _context.Project.Include(p => p.AppTasks)
                                      .Include(p => p.Notifications)
                                      .FirstOrDefault(p => p.Id == id);
                foreach (var notification in project.Notifications)
                {
                    notification.ProjectId = default;
                    notification.AppTaskId = default;
                }
                foreach (var task in project.AppTasks)
                {
                    _context.AppTask.Remove(task);
                }

                _context.Project.Remove(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }



        public IActionResult AssignUser(int id)
        {
            var Project = _context.Project.First(P => P.Id == id);

            var users = _context.Users.Include(u => u.ProjectAndUsers)
                                      .Where(u => !u.ProjectAndUsers.Select(p => p.ProjectId).Contains(id));

            ViewBag.UserList = new SelectList(users, "Id", "UserName");
            ViewBag.ProjectId = Project.Id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignUser(int projectId, string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var newPUser = new ProjectAndUser
                {
                    ProjectId = projectId,
                    AppUserId = userId,
                    StartDate = startDate.ToUniversalTime(), // Chuyển đổi sang UTC
                    EndDate = endDate.ToUniversalTime() // Chuyển đổi sang UTC
                };

                _context.ProjectAndUser.Add(newPUser);
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest($"An error occurred while saving the entity changes: {innerException}");
            }
            catch (Exception ex)
            {
                return BadRequest($"An unexpected error occurred: {ex.Message}");
            }
        }

        public IActionResult TotalCostDetails(int id)
        {
            // Lấy dự án từ cơ sở dữ liệu dựa trên id
            var project = _context.Project
                                   .Include(p => p.ProejectAndUsers)
                                   .ThenInclude(pu => pu.AppUser)
                                   .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy dự án
            }

            return View(project); // Trả về view chứa thông tin chi tiết chi phí tổng của dự án
        }

    }
}
