using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using AccountAPI.Exceptions;

namespace AccountAPI.Services.Implements
{
    public class CloudinaryService: ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("File is empty");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "account_avatars"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new BadRequestException(result.Error.Message);

            return result.SecureUrl.ToString();
        }
    }
}
