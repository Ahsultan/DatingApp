using Api.Entities;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(AppDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppUser> GetByUserNameAsync(string userName)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == userName);
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {

           //var test = _context.Users
           //     .Where(u => u.UserName == username)
           //     .Select(user => new MemberDto()
           //     {
           //         Id = user.Id,
           //         UserName = user.UserName,
           //     })
           //     .SingleOrDefaultAsync();

           // var member = _context.Users.SingleOrDefaultAsync(u => u.UserName == username);

           // var memberToReturn = _mapper.Map<MemberDto>(member);



            return await _context.Users
                .Where(u => u.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            var query = _context.Users
            .Where(u => u.UserName != userParams.CurrentUsername && 
                        u.Gender == userParams.Gender &&
                        u.DateOfBirth >= minDob &&
                        u.DateOfBirth <= maxDob)
            .OrderByDescending(u => userParams.OrderBy == "created" ? u.Created : u.LastActive)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking();

            // var test = userParams.OrderBy switch
            // {
            //     "created" => query.OrderByDescending(u => u.Created),
            //     _ => query.OrderByDescending(u => u.LastActive)
            // };

            return await PagedList<MemberDto>.CreateAsync(query, userParams.pageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos)
            .AsNoTracking().ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
