using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepositoy;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        _userRepositoy = userRepository;
        _mapper = mapper;
        _photoService = photoService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetAll([FromQuery]UserParams userParams) {

            var user = await _userRepositoy.GetByUserNameAsync(User.GetUserName());

            userParams.CurrentUsername = user.UserName;

            userParams.Gender ??= userParams.Gender == "male" ? "male" : "male";

            var users =  await _userRepositoy.GetMembersAsync(userParams);

            //var usersToReturn = _mapper.Map<List<MemberDto>>(users);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);

    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> Get(string username) 
    {
        var user = await _userRepositoy.GetMemberAsync(username);
        
        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateMember(UpdateMemberDto updateDto)
    { 
        var user = await _userRepositoy.GetByUserNameAsync(User.GetUserName());

        if (user == null) return NotFound();

        _mapper.Map(updateDto, user);

        if(await _userRepositoy.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update the user!");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _userRepositoy.GetByUserNameAsync(User.GetUserName());

        if (user == null) return NotFound();

        var result = await _photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error);

        var photo = new Photo()
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            IsMain = user.Photos.Count == 0,
        };

        user.Photos.Add(photo);

        if(await _userRepositoy.SaveAllAsync())
        {
            return CreatedAtAction(nameof(Get), new {username = user.UserName}, _mapper.Map<PhotoDto>(photo)); 
        }

        return BadRequest("Unable to Upload the Photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _userRepositoy.GetByUserNameAsync(User.GetUserName());

        if(user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if(photo == null) return NotFound();

        if(photo.IsMain) return BadRequest("Photo is alredy set to main");

        var currentDp = user.Photos.FirstOrDefault(x => x.IsMain);
        if(currentDp != null) currentDp.IsMain = false;

        photo.IsMain = true;

        if(await _userRepositoy.SaveAllAsync()) return NoContent();

        return BadRequest("");

    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _userRepositoy.GetByUserNameAsync(User.GetUserName());

        if(user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if(photo == null) return NotFound();

        if(photo.IsMain) return BadRequest("Please change this photo from main.");

        if(photo.PublicId != null) {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest();
        }

        user.Photos.Remove(photo);

        if(await _userRepositoy.SaveAllAsync()) return Ok();

        return BadRequest("Error in deleting photos.");
        
    }

}
