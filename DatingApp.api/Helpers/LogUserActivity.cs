﻿using DatingApp.api.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.api.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var datingRepository = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();

            var user = await datingRepository.GetUser(userId);

            user.LastActive = DateTime.Now;

            await datingRepository.SaveAll();
        }
    }
}
