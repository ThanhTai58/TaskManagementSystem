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
    [Authorize(Roles = "ProjectManager")]
    public class TaskHelperController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskHelperController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskHelper/Index
        // Hiển thị danh sách tất cả các nhiệm vụ
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AppTask.Include(a => a.AppUser).Include(a => a.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TaskHelper/AssignTaskToProject/5
        // Hiển thị form gán nhiệm vụ vào dự án
        public IActionResult AssignTaskToProject(int id)
        {
            // Tìm nhiệm vụ cần gán dựa trên ID
            var task = _context.AppTask.First(t => t.Id == id);
            ViewBag.TaskId = task.Id;
            ViewBag.TaskName = task.Name;

            // Lấy danh sách tất cả các dự án để chọn
            var allprojects = _context.Project;
            var projectList = new SelectList(allprojects, "Id", "Name");

            return View(projectList);
        }

        // POST: TaskHelper/AssignTaskToProjectPost
        // Xử lý gán nhiệm vụ vào dự án
        [HttpPost]
        public IActionResult AssignTaskToProjectPost(int projectid, int taskid)
        {
            // Tìm nhiệm vụ cần gán và cập nhật ProjectId
            var task = _context.AppTask.First(t => t.Id == taskid);
            task.ProjectId = projectid;
            _context.Update(task);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: TaskHelper/Details/5
        // Hiển thị chi tiết nhiệm vụ
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appTask = await _context.AppTask
                .Include(a => a.AppUser)
                .Include(a => a.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appTask == null)
            {
                return NotFound();
            }

            return View(appTask);
        }

        // GET: TaskHelper/Create
        // Hiển thị form tạo mới nhiệm vụ
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_context.AppUser, "Id", "UserName");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            return View();
        }

        // POST: TaskHelper/Create
        // Xử lý tạo mới nhiệm vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeadLine,Name,CompletePercentage,Comment,ProjectId,AppUserId")] AppTask appTask)
        {
            if (ModelState.IsValid)
            {
                // Đặt DateTimeKind của DeadLine thành Utc
                appTask.DeadLine = appTask.DeadLine.ToUniversalTime();

                _context.Add(appTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, trả về form với dữ liệu và thông báo lỗi
            ViewData["AppUserId"] = new SelectList(_context.AppUser, "Id", "UserName");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            return View(appTask);
        }

        // GET: TaskHelper/Edit/5
        // Hiển thị form chỉnh sửa nhiệm vụ
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appTask = await _context.AppTask.FindAsync(id);
            if (appTask == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.AppUser, "Id", "UserName");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name");
            return View(appTask);
        }

        // POST: TaskHelper/Edit/5
        // Xử lý chỉnh sửa nhiệm vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeadLine,Id,Name,CompletePercentage,Comment,ProjectId,AppUserId")] AppTask appTask)
        {
            if (id != appTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Đặt DateTimeKind của DeadLine thành Utc
                    appTask.DeadLine = appTask.DeadLine.ToUniversalTime();
                    _context.Update(appTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppTaskExists(appTask.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.AppUser, "Id", "Id", appTask.AppUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Id", appTask.ProjectId);
            return View(appTask);
        }

        // GET: TaskHelper/Delete/5
        // Hiển thị form xác nhận xóa nhiệm vụ
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appTask = await _context.AppTask
                .Include(a => a.AppUser)
                .Include(a => a.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appTask == null)
            {
                return NotFound();
            }

            return View(appTask);
        }

        // POST: TaskHelper/Delete/5
        // Xử lý xóa nhiệm vụ
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appTask = await _context.AppTask.FindAsync(id);
            _context.AppTask.Remove(appTask);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: TaskHelper/NotFinishedAndPassedDeadlineTasks
        // Hiển thị danh sách các nhiệm vụ chưa hoàn thành và đã qua hạn
        public IActionResult NotFinishedAndPassedDeadlineTasks()
        {
            // Lấy danh sách các nhiệm vụ chưa hoàn thành
            var notCompletedtasks = _context.AppTask.Include(a => a.Project).Include(a => a.AppUser)
                                                    .Where(a => a.CompletePercentage < 100).ToList();

            var passedDeadLineTasks = new List<AppTask>();
            foreach (var task in notCompletedtasks)
            {
                // Kiểm tra các nhiệm vụ có DeadLine đã qua hạn
                if ((task.DeadLine - DateTime.Now.Date).Days < 0)
                {
                    passedDeadLineTasks.Add(task);
                }
            }
            return View(passedDeadLineTasks);
        }

        // Kiểm tra xem nhiệm vụ có tồn tại không
        private bool AppTaskExists(int id)
        {
            return _context.AppTask.Any(e => e.Id == id);
        }
    }
}
