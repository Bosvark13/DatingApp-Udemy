using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.api.Data;
using DatingApp.api.DTO;
using DatingApp.api.Helpers;
using DatingApp.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.api.Controllers
{
    [Authorize]
    [Route("api/users/{userid}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository datingRepository;
        private readonly IMapper mapper;
        private readonly IOptions<CloudinarySettings> cloudinaryConfig;

        private Cloudinary cloudinary;

        public PhotosController(IDatingRepository datingRepository, IMapper mapper, 
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this.datingRepository = datingRepository;
            this.mapper = mapper;
            this.cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(cloudinaryConfig.Value.CloudName, cloudinaryConfig.Value.ApiKey, 
                cloudinaryConfig.Value.ApiSecret);

            cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await datingRepository.GetPhoto(id);

            var result = mapper.Map<PhotoForReturnDto>(photo);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreation)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await datingRepository.GetUser(userId);

            var file = photoForCreation.File;

            var result = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    result = cloudinary.Upload(uploadParams);
                }
            }

            photoForCreation.Url = result.Uri.ToString();
            photoForCreation.PublicId = result.PublicId;

            var photo = mapper.Map<Photo>(photoForCreation);

            if (!user.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if ( await datingRepository.SaveAll())
            {
                var returnResult = mapper.Map<PhotoForReturnDto>(photo);

                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, returnResult); 
            }

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await datingRepository.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photo = await datingRepository.GetPhoto(id);

            if (photo.IsMain)
            {
                return BadRequest("This is already the main photo");
            }

            var currentMainPhoto = await datingRepository.GetMainPhotoForUser(userId);

            currentMainPhoto.IsMain = false;

            photo.IsMain = true;

            if (await datingRepository.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await datingRepository.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photo = await datingRepository.GetPhoto(id);

            if (photo.IsMain)
            {
                return BadRequest("You cannot delete your main photo");
            }

            if (photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);

                var result = cloudinary.Destroy(deleteParams);

                if (result.Result.Equals("ok"))
                {
                    datingRepository.Delete(photo);
                }
            }

            if (photo.PublicId == null)
            {
                datingRepository.Delete(photo);
            }

            if (await datingRepository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to delete the photo");
        }
    }
}