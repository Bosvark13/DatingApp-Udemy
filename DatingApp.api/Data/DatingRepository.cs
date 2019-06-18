using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.api.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext dataContext;

        public DatingRepository(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public void Add<T>(T entity) where T : class
        {
            dataContext.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            dataContext.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await dataContext.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await dataContext.Users.Include(p => p.Photos).ToListAsync();

            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await dataContext.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await dataContext.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await dataContext.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }
    }
}
