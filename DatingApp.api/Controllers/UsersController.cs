﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.api.Data;
using DatingApp.api.DTO;
using DatingApp.api.Helpers;
using DatingApp.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.api.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository datingRepository;
        private readonly IMapper mapper;

        public UsersController(IDatingRepository datingRepository, IMapper mapper)
        {
            this.datingRepository = datingRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var currentUser = await datingRepository.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = (currentUser.Gender == "male") ? "female" : "male";
            }

            var users = await datingRepository.GetUsers(userParams);

            var usersResult = mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersResult);
        }

        [HttpGet("{id}", Name ="GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await datingRepository.GetUser(id);

            var userResult = mapper.Map<UserForDetailedDto>(user);

            return Ok(userResult);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdate)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await datingRepository.GetUser(id);

            mapper.Map(userForUpdate, user);

            if (await datingRepository.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {id} falied to save");
        }

        [HttpPost("{id}/like/recipientId")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await datingRepository.GetLike(id, recipientId);

            if (like != null)
            {
                return BadRequest("You already like this person");
            }

            if (await datingRepository.GetUser(recipientId) == null)
            {
                return NotFound();
            }

            like = new Like()
            {
                LikerId = id,
                LikeeId = recipientId
            };

            datingRepository.Add<Like>(like);

            if (await datingRepository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to like this person");
        }
    }
}