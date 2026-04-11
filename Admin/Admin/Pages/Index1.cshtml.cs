using Admin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages
{
    public class Index1Model : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public Index1Model(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<AudioViewModel> AudioList { get; set; } = new();
        public List<TourismSite> Sites { get; set; } = new();
        public List<Language> Languages { get; set; } = new();

        [BindProperty]
        public int SelectedSiteId { get; set; }
        [BindProperty]
        public int SelectedLangId { get; set; }
        [BindProperty]
        public string VoiceGender { get; set; } = "Nam";
        [BindProperty]
        public IFormFile? UploadedFile { get; set; }

        public async Task OnGetAsync(string? searchTerm)
        {
            Sites = await _context.TourismSites.ToListAsync();
            Languages = await _context.Languages.ToListAsync();

            var query = from audio in _context.SiteAudios
                        join site in _context.TourismSites on audio.SiteID equals site.SiteID
                        join lang in _context.Languages on audio.LangID equals lang.LangID
                        select new AudioViewModel
                        {
                            AudioId = audio.AudioID,
                            SiteName = site.TourismName,
                            Language = lang.LangName,
                            AudioUrl = audio.AudioURL,
                            Gender = audio.VoiceGender
                        };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.SiteName.Contains(searchTerm));
            }

            AudioList = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (UploadedFile == null || SelectedSiteId == 0 || SelectedLangId == 0) return RedirectToPage();

            // Lưu file vào thư mục wwwroot/uploads/audio
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "audio");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + UploadedFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(fileStream);
            }

            // Lưu vào DB
            var newAudio = new SiteAudio
            {
                SiteID = SelectedSiteId,
                LangID = SelectedLangId,
                AudioURL = "/uploads/audio/" + uniqueFileName,
                VoiceGender = VoiceGender
            };

            _context.SiteAudios.Add(newAudio);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var audio = await _context.SiteAudios.FindAsync(id);
            if (audio != null)
            {
                // Xóa file vật lý
                var filePath = Path.Combine(_environment.WebRootPath, audio.AudioURL.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                _context.SiteAudios.Remove(audio);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }

    public class AudioViewModel
    {
        public int AudioId { get; set; }
        public string SiteName { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string AudioUrl { get; set; } = null!;
        public string? Gender { get; set; }
    }
}
