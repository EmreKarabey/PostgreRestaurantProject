using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Login.Command.Login
{
    public class LoginResult
    {
        public LoginResult(bool status, string message, string? token = null)
        {
            Status = status;
            Message = message;
            Token = token;
        }

        public bool Status { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }
    }
}
