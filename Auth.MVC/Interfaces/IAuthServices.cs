using Auth.MVC.DTO;
using Auth.MVC.Models;

namespace Auth.MVC.Interfaces
{
    public interface IAuthServices
    {
        Task<LoginDTO> LoginAsync(LoginViewModel loginDTO);

        Task<RegisterDTO> RegisterAsync(RegisterViewModel registerDTO);
        Task<EmailListDTO> GetCacheEmail();
    }
}
