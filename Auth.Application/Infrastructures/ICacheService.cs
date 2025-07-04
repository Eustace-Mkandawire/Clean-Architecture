using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Repositories
{
    public interface ICacheService
    {
        void AddEmailToListAsync(string email);
        Task<List<string>> GetCachedEmailListAsync();
    }
}
