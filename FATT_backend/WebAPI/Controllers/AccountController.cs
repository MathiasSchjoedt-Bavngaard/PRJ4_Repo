﻿using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Dto.Account;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Dto.Workout;
using System.Collections.Generic;
using WebAPI.Dto.Calender;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AccountGetDto>> Register(AccountDto request)
        {
            if (!IsVaildEmail(request.Email))
            {
                return BadRequest("Email is not valid");
            }

            if (await _context.Accounts.AnyAsync(x => x.Email == request.Email))
                return BadRequest("Email is already taken");

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var account = new Account
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Age = 0,
                Weigth = 0,
                Gender = ""
            };
            //_context.Calender.Add(Calender);
            _context.Accounts.Add(account);

            var id = await _context.SaveChangesAsync();

            return Accepted(account.Email);
            //return CreatedAtAction(nameof(GetAccountEmail), new { email = account.Email }, account); ;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AccountGetLoginDto>> Login(AccountLoginDto request)
        {
            try
            {
                var dbAcccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Email == request.Email);
                if (dbAcccount == null)
                {
                    return NotFound(request.Email);
                }

                if (!TryVerifyPasswordHash(request.Password, dbAcccount.PasswordHash, dbAcccount.PasswordSalt))
                {
                    return BadRequest("Not a valid Password");
                }

                var token = CreateToken(dbAcccount);
                var account = dbAcccount.Adapt<AccountGetLoginDto>();
                account.Token = token;
                return Ok(account);
            }
            catch (Exception)
            {
                return BadRequest("Not a valid login");
            }
        }


        [HttpPut("ChangeEmail")]
        public async Task<ActionResult<string>> ChangeEmail(AccountChangeEmailDto request)
        {
            var dbAccount = await _context.Accounts.Where(x => x.Email == request.Email).FirstOrDefaultAsync();
            try
            {
                if (!TryVerifyPasswordHash(request.Password, dbAccount.PasswordHash, dbAccount.PasswordSalt))
                {
                    return BadRequest("Not a valid Password");
                }

                dbAccount.Email = request.NewEmail;

                await _context.SaveChangesAsync();
                return Accepted(dbAccount);
            }
            catch (Exception)
            {
                return BadRequest("Not a valid login");
            }
        }

        [HttpPut("ChangePassword")]
        public async Task<ActionResult<string>> ChangePassword(AccountChangePasswordDto request)
        {
            try
            {
                var dbAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Email == request.Email);
                if (dbAccount == null) { return NotFound(request.Email); }

                if (!TryVerifyPasswordHash(request.Password, dbAccount.PasswordHash, dbAccount.PasswordSalt))
                {
                    return BadRequest("Wrong password");
                }

                CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

                dbAccount.PasswordHash = passwordHash;
                dbAccount.PasswordSalt = passwordSalt;

                dbAccount.Adapt(dbAccount);
                await _context.SaveChangesAsync();

                //string token = CreateToken(dbAccount);
                return Ok(CreateToken(dbAccount));
            }
            catch (Exception)
            {
                return BadRequest("Error in changing password");
            }

        }

        [HttpPut("{email}/Name/{name}")]
        public async Task<ActionResult<AccountGetDto>> UpdateAccountName(string email, string name)
        {
            var dbAccount = await _context.Accounts.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (dbAccount == null) { return NotFound(email); }

            dbAccount.Name = name;
            await _context.SaveChangesAsync();

            return Accepted(dbAccount.Adapt<AccountGetDto>());
        }

        [HttpPut("{email}/Age/{age}")]
        public async Task<ActionResult<AccountGetDto>> UpdateAccountAge(string email, int age)
        {
            var dbAccount = await _context.Accounts.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (dbAccount == null) { return NotFound(email); }

            dbAccount.Age = age;
            await _context.SaveChangesAsync();

            return Accepted(dbAccount.Adapt<AccountGetDto>());
        }

        [HttpPut("{email}/Gender/{gender}")]
        public async Task<ActionResult<AccountGetDto>> UpdateAccountGender(string email, string gender)
        {
            var dbAccount = await _context.Accounts.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (dbAccount == null) { return NotFound(email); }

            dbAccount.Gender = gender;
            await _context.SaveChangesAsync();

            return Accepted(dbAccount.Adapt<AccountGetDto>());
        }

        [HttpPut("{email}/Weight/{weight}")]
        public async Task<ActionResult<AccountGetDto>> UpdateAccountWeight(string email, int weight)
        {
            var dbAccount = await _context.Accounts.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (dbAccount == null) { return NotFound(email); }

            dbAccount.Weigth = weight;
            await _context.SaveChangesAsync();

            return Accepted(dbAccount.Adapt<AccountGetDto>());
        }

        [HttpDelete()]
        public async Task<ActionResult<string>> DeleteAccount(AccountDeleteDto account)
        {
            var dbAccount = await _context.Accounts.Include(a => a.Calender).FirstOrDefaultAsync(x => x.Email == account.Email);
            if (dbAccount == null) { return NotFound(account.Email); }
            
            try
            {
                if (!TryVerifyPasswordHash(account.Password, dbAccount.PasswordHash, dbAccount.PasswordSalt))
                {
                    return BadRequest("Can't delete account");
                }

                _context.Accounts.Remove(dbAccount);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Not a valid login");
            }
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<AccountGetDto>> GetAccountEmail(string email)
        {
            var dbAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Email == email);
            if (dbAccount == null) { return NotFound($"Could not find account with email {email}"); }
               
            return Ok(dbAccount.Adapt<AccountGetDto>());      
        }
        
        [HttpGet("{email}/calender")]
        public async Task<ActionResult<CalenderGetDto>> GetAccountCalender(string email)
        {
            var dbAccount = await _context.Accounts
                .Include(a => a.Calender)
                    .ThenInclude(c => c.WorkoutDays)
                .Include(a => a.Calender)
                    .ThenInclude(c => c.MealDays)
                .FirstOrDefaultAsync(x => x.Email == email);

            if (dbAccount == null) { return NotFound($"could not find account with email {email}"); }

            var result = dbAccount.Calender.Adapt<CalenderGetDto>();

            foreach (var workoutDay in dbAccount.Calender.WorkoutDays)
            {
                switch (workoutDay.Day)
                {
                    case "Monday":
                        result.WorkoutDays.Monday.Add(workoutDay.WorkoutId);
                        break;
                    case "Tuesday":
                        result.WorkoutDays.Tuesday.Add(workoutDay.WorkoutId);
                        break;
                    case "Wednesday":
                        result.WorkoutDays.Wednesday.Add(workoutDay.WorkoutId);
                        break;
                    case "Thursday":
                        result.WorkoutDays.Thursday.Add(workoutDay.WorkoutId);
                        break;
                    case "Friday":
                        result.WorkoutDays.Friday.Add(workoutDay.WorkoutId);
                        break;
                    case "Saturday":
                        result.WorkoutDays.Saturday.Add(workoutDay.WorkoutId);
                        break;
                    case "Sunday":
                        result.WorkoutDays.Sunday.Add(workoutDay.WorkoutId);
                        break;
                    default:
                        break;
                }
            }

            foreach (var mealDay in dbAccount.Calender.MealDays)
            {
                switch (mealDay.Day)
                {
                    case "Monday":
                        result.MealDays.Monday.Add(mealDay.MealId);
                        break;
                    case "Tuesday":
                        result.MealDays.Tuesday.Add(mealDay.MealId);
                        break;
                    case "Wednesday":
                        result.MealDays.Wednesday.Add(mealDay.MealId);
                        break;
                    case "Thursday":
                        result.MealDays.Thursday.Add(mealDay.MealId);
                        break;
                    case "Friday":
                        result.MealDays.Friday.Add(mealDay.MealId);
                        break;
                    case "Saturday":
                        result.MealDays.Saturday.Add(mealDay.MealId);
                        break;
                    case "Sunday":
                        result.MealDays.Sunday.Add(mealDay.MealId);
                        break;
                    default:
                        break;
                }
            }

            return Ok(result);


        }

        private string CreateToken(Account account)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.Name),
                new Claim(ClaimTypes.Role, "Admin")

            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private static bool IsVaildEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }

        }
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

        }
        private static bool TryVerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

    }

}
