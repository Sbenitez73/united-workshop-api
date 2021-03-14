using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkshopAPI.Contexts;
using WorkshopAPI.Entities;
using WorkshopAPI.Models;
using WorkshopAPI.Services;

namespace WorkshopAPI.Controllers
{
    [Route("api/[controller]")]
    public class UnAuthController: ControllerBase
    {

        private readonly ApplicationDbContext context;
        private readonly HashService _hashService;
        public UnAuthController(ApplicationDbContext context, HashService hasService)
        {
            this.context = context;
            _hashService = hasService;
        }

        [HttpGet("{id}", Name = "getUser")]
        public async Task<ActionResult<Users>> Get([BindRequired] int id)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.UserCode == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;

        }

        [HttpPost("register/user")]
        public async Task<ActionResult> Post([FromBody] Users user)
        {
            try
            {
                ApiError errorResponse = new ApiError();
                var userNameCreated = await context.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName);
                var emailRegistered = await context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
                var dniRegistered = await context.Users.FirstOrDefaultAsync(x => x.Identification == user.Identification);
                var telephoneRegistered = await context.Users.FirstOrDefaultAsync(x => x.Telephone == user.Telephone);

                if (userNameCreated != null)
                {
                    errorResponse.Success = false;
                    errorResponse.ErrorMessage = "El usuario ya existe";
                    return new JsonResult(errorResponse);
                }

                else if (emailRegistered != null)
                {
                    errorResponse.Success = false;
                    errorResponse.ErrorMessage = "El Email ya esta registrado";
                    return new JsonResult(errorResponse);
                }

                else if (dniRegistered != null)
                {
                    errorResponse.Success = false;
                    errorResponse.ErrorMessage = "El Número de Identificación ya esta registrado";
                    return new JsonResult(errorResponse);
                }

                else if (telephoneRegistered != null)
                {
                    errorResponse.Success = false;
                    errorResponse.ErrorMessage = "El telefono ya ha sido registrado";

                    return new JsonResult(errorResponse);
                }

                var hash = _hashService.Hash(user.Password);
                user.Password = hash.Hash;
                user.SaltPassword = hash.Salt;

                context.Users.Add(user);
                context.SaveChanges();
                return new CreatedAtRouteResult("getUser", new { id = user.UserCode }, user);
            }

            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

           
        }
    }
}
