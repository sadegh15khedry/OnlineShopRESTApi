﻿using AuthenticationPlugin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using test.Data;
using test.Models;

namespace test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private ShopDbContext _dbCotext;
        private IConfiguration _configuration;
        private readonly AuthService _auth;


        public UsersController(ShopDbContext dbCotext, IConfiguration configuration)
        {
            _configuration = configuration;
            _auth = new AuthService(_configuration);
            _dbCotext = dbCotext;
        }

        // GET: api/<UsersController1>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_dbCotext.Users);
        }

        // GET api/<UsersController1>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var myUser = _dbCotext.Users.Find(id);
            if (myUser == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(myUser);
            }
        }

        // POST api/<UsersController1>/register
        [HttpPost("[action]")]
        public IActionResult Register([FromForm] User user)
        {
            //ckecking email and username to see if already exists
            var userWithSameEmail = _dbCotext.Users.Where(u => u.UserEmail == user.UserEmail).SingleOrDefault();
            if (userWithSameEmail != null)
            {
                return BadRequest("user with this email already exists");
            }
            var userObj = new User
            {
                UserFirstName = user.UserFirstName,
                UserLastName = user.UserLastName,
                UserEmail = user.UserEmail,
                UserPassword = SecurePasswordHasherHelper.Hash(user.UserPassword),
                UserPhone = user.UserPhone,
                ImageUrl = "/img\\default_profile_pic.jpg",
                UserRole = "user"
            };
            _dbCotext.Users.Add(userObj);
            _dbCotext.SaveChanges();
            return StatusCode(201, "user created");


        }

        // POST api/<UsersController1>/login
        [HttpPost("[action]")]
        public IActionResult Login([FromForm] string userEmail, [FromForm] string userPassword)
        {
            var myUser = _dbCotext.Users.FirstOrDefault(u => u.UserEmail == userEmail);
            if (myUser == null)
            {
                return NotFound();
            }
            if (!SecurePasswordHasherHelper.Verify(userPassword, myUser.UserPassword))
            {
                return Unauthorized();
            }
            var claims = new[]
{
            new Claim(JwtRegisteredClaimNames.Email, myUser.UserEmail),
            new Claim(ClaimTypes.Email, myUser.UserEmail),
            new Claim(ClaimTypes.Role, myUser.UserRole),
            new Claim(ClaimTypes.NameIdentifier, myUser.UserId.ToString()),
            };

            var token = _auth.GenerateAccessToken(claims);
            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn,
                token_type = token.TokenType,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
                user_id = myUser.UserId,
                user_role = myUser.UserRole,
            });

        }

        // PUT api/<UsersController1>/AddPhoto/5
        [HttpPut("[action]/{id}")]
        [Authorize]
        public IActionResult AddPhoto(int id, [FromForm] IFormFile photo)
        {
            int userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString());
            var myUser = _dbCotext.Users.Find(userId);
            if (myUser == null)
            {
                return NotFound("not found");
            }

            else if (photo != null)
            {
                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot/img", guid + ".jpg");
                var fileStream = new FileStream(filePath, FileMode.Create);
                photo.CopyTo(fileStream);
                myUser.ImageUrl = filePath.Remove(0, 7);
                _dbCotext.SaveChanges();
                return Ok("updated");
            }
            else
            {
                return BadRequest("not a valid image");
            }
        }


        // PUT api/<UsersController1>/RemovePhoto/5
        [HttpPut("[action]/{id}")]
        [Authorize]
        public IActionResult RemovePhoto(int id)
        {
            int userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString());
            var myUser = _dbCotext.Users.Find(userId);
            if (myUser == null)
            {
                return NotFound("not found");
            }

            else if (myUser != null)
            {
                myUser.ImageUrl = "/img\\default_profile_pic.jpg";
                _dbCotext.SaveChanges();
                return Ok("updated");
            }
            else
            {
                return BadRequest("not valid");
            }
        }


        // PUT api/<UsersController1>/update/5
        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromForm] string? userFirstName, [FromForm] string? userLastName,
            [FromForm] string? userSSN)
        {
            int userId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString());
            var myUser = _dbCotext.Users.Find(userId);

            if (myUser == null)
            {
                return NotFound("no found");
            }

            if (userFirstName != null)
            {
                myUser.UserFirstName = userFirstName;
            }
            if (userLastName != null)
            {
                myUser.UserLastName = userLastName;
            }
            if (userSSN != null)
            {
                myUser.UserSSN = Int16.Parse(userSSN);
            }
            _dbCotext.SaveChanges();
            return Ok("updated");


        }

        // DELETE api/<UsersController1>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            var myUser = _dbCotext.Users.Find(id);
            if (myUser == null)
            {
                return NotFound("no found");
            }
            else
            {
                _dbCotext.Users.Remove(myUser);
                _dbCotext.SaveChanges();
                return Ok("deleted");
            }
        }


        //public static string SubjectId(this ClaimsPrincipal user) { return user?.Claims?.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value; }

    }
}