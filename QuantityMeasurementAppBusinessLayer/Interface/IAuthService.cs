using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppModelLayer.Entity;

namespace QuantityMeasurementAppBusinessLayer.Interface
{
    public interface IAuthService
    {
        string Register(RegisterDTO user);
        UserEntity? Login(LoginDTO user);
        string GenerateJwtToken(UserEntity user);
        Task<UserEntity?> LoginWithGoogle(string idToken);
        UserEntity? GetUserById(int id);
    }
}
