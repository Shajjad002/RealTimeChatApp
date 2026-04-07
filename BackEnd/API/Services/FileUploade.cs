using System;

namespace API.Services;

public class FileUpload
{
    public static async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is null or empty.");
        }
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadPath, fileName);

        try
        {
            //Directory.CreateDirectory(uploadPath); // Ensure the directory exists

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            

            return fileName; // Return the unique file name for storage in the database
        }
        catch (Exception ex)
        {
            throw new Exception($"File upload failed: {ex.Message}");
        }
    }

}
