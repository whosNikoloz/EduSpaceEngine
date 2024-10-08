﻿using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.User.LoginRequest
{
    public class UserLoginEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
