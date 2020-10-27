using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ApiNPTNorthwind.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApiNPTNorthwind.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        public string secretKey = "0406b130-bd65-11ea-b3de-0242ac130004-ab0660bb-a138-45e9-bdf1-9ff1ac62ae5e-0242ac130004-ab0660bb-a138-45e9-b";
        private IConfiguration _config;

        public UserController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public string AddNewUser([FromBody] UserLoginDTO user)
        {

            try
            {
                DataAccess.NORTHWNDContext dataContext = new DataAccess.NORTHWNDContext();

                if (dataContext.Users.Any(a => a.UserName == user.UserName))
                    throw new Exception("No se pudo crear el usuario, ya existe otro con el mismo nombre");

                var salt = Guid.NewGuid().ToString();

                var password = Encrypt (user.Password, salt, 1);


                var newUser = new DataAccess.Users()
                {
                    Id = Guid.NewGuid(),
                    Password = password,
                    Salt = salt,
                    UserName = user.UserName
                };

                dataContext.Users.Add(newUser);
                dataContext.SaveChanges();

                return "Usuario creado";

            }

            catch (Exception ex)
            {
                return "No se pudo crear el usuario, revisa los errores: " + ex.Message;
            }

        }

        [HttpPost]
        public string Login([FromBody] UserLoginDTO user)
        {

            try
            {
                DataAccess.NORTHWNDContext dataContext = new DataAccess.NORTHWNDContext();
                var myUser = dataContext.Users.Where(w => w.UserName == user.UserName).FirstOrDefault();

                if (myUser == null)
                    return "No se pudo encontrar el usuario especificado";

                var encryptedPassword = myUser.Password;
                var salt = myUser.Salt;


                var originalPassword = Decrypt(encryptedPassword, salt);

                if (originalPassword == user.Password)
                    return "Autenticación exitosa";
                else
                    return "El password proporcionado es incorrecto";


            }

            catch (Exception ex)
            {
                return "No se pudo ejecutar la consulta " + ex.Message;
            }

        }

        [HttpPost]
        public string GetToken([FromBody] UserLoginDTO user)
        {

            var token = "";

            DataAccess.NORTHWNDContext dataContext = new DataAccess.NORTHWNDContext();
            var myUser = dataContext.Users.Where(w => w.UserName == user.UserName).FirstOrDefault();

            if (myUser == null)
                return "";

            var encryptedPassword = myUser.Password;
            var salt = myUser.Salt;


            var originalPassword = Decrypt(encryptedPassword, salt);

            if (originalPassword == user.Password)
                token = GenerateToken();

            return token;
        }

        private string GenerateToken()
        {
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:PrivateKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(60), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string Encrypt(string password, string saltValue, int? opcion)
        {

            string secretKey = this.secretKey;
            var saltBuffer = Encoding.UTF8.GetBytes(saltValue);
            byte[] clearBytes = Encoding.Unicode.GetBytes(password);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(secretKey, saltBuffer);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    password = Convert.ToBase64String(ms.ToArray());
                }
            }

            return password;
        }

        public string Decrypt(string cipherText, string saltValue)
        {
            string secretKey = this.secretKey;
            var saltBuffer = Encoding.UTF8.GetBytes(saltValue);
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(secretKey, saltBuffer);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
