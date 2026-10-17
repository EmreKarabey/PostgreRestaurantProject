using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Cloudflare;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Login.Command.Register
{
    public class RegisterCommand : IRequest<RegisterResult>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile Image { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResult>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICloudflareStorageService _cloudflareStorageService;

        public RegisterHandler(UserManager<AppUser> userManager, ICloudflareStorageService cloudflareStorageService, IMapper mapper)
        {
            _userManager = userManager;
            _cloudflareStorageService = cloudflareStorageService;
            _mapper = mapper;
        }

        public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {

            if (request.Image == null || request.Image.Length == 0)
            {
                return new RegisterResult(false, "Lütfen kullanıcı için bir görsel yükleyiniz.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(request.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return new RegisterResult(false, "Yalnızca JPG, JPEG, PNG veya WEBP formatında görsel yükleyebilirsiniz.");
            }

            var appuser = _mapper.Map<AppUser>(request);

            try
            {
                var folder = "user";
                var imageURL = await _cloudflareStorageService.UploadImageAsync(request.Image, folder);
                appuser.ProfileImageURL = imageURL;
            }
            catch (Exception ex)
            {
                return new RegisterResult(false, $"Görsel Cloudflare R2'ye yüklenirken hata oluştu: {ex.Message}");
            }

            var result = await _userManager.CreateAsync(appuser, request.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    string turkishMessage = TranslateIdentityError(item.Code, item.Description);

                    if (item.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
                    {
                        return new RegisterResult(false, turkishMessage);
                    }
                    else if (item.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase))
                    {
                        return new RegisterResult(false, turkishMessage);
                    }
                    else if (item.Code.Contains("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        return new RegisterResult(false, turkishMessage);
                    }
                    else
                    {
                        return new RegisterResult(false, turkishMessage);
                    }
                }
            }

            return new RegisterResult(true, $"{appuser.UserName} kullanıcısı başarıyla eklendi.");
        }

        private static string TranslateIdentityError(string code, string defaultDesc)
        {
            return code switch
            {
                "DuplicateUserName" => "Bu kullanıcı adı sistemde zaten kayıtlı.",
                "DuplicateEmail" => "Bu e-posta adresi sistemde zaten kayıtlı.",
                "PasswordTooShort" => "Şifre en az 6 karakter uzunluğunda olmalıdır.",
                "PasswordRequiresNonAlphanumeric" => "Şifreniz en az bir özel karakter (@, #, $, !, . vb.) içermelidir.",
                "PasswordRequiresDigit" => "Şifreniz en az bir rakam ('0'-'9') içermelidir.",
                "PasswordRequiresLower" => "Şifreniz en az bir küçük harf ('a'-'z') içermelidir.",
                "PasswordRequiresUpper" => "Şifreniz en az bir büyük harf ('A'-'Z') içermelidir.",
                "InvalidUserName" => "Kullanıcı adı yalnızca harf, rakam ve izin verilen karakterleri içerebilir.",
                "InvalidEmail" => "Lütfen geçerli bir e-posta formatı giriniz.",
                _ => defaultDesc
            };
        }
    }
}
