using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Jwt;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Login.Command.Login
{
    public class LoginCommand : IRequest<LoginResult>
    {
        public string EmailorUserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool rememberMe { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;

        public LoginCommandHandler(UserManager<AppUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.EmailorUserName) ?? await _userManager.FindByNameAsync(request.EmailorUserName);

            if (user == null)
            {
                return new LoginResult(false, "Giriş Yapılamadı kullanıcı adı/maili yada şifreniz yanlış!");
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!passwordCheck)
            {
                return new LoginResult(false, "Giriş Yapılamadı kullanıcı adı/maili yada şifreniz yanlış!");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateToken(
                user.Id,
                user.UserName ?? user.Id.ToString(),
                userRoles.FirstOrDefault() ?? "User",
                request.rememberMe);

            return new LoginResult(true, $"Hoş geldiniz, {user.UserName ?? user.Id.ToString()}!", token);
        }
    }
}
