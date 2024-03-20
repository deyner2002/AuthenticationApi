using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using AuthenticationApi.DTOs;
using AuthenticationApi.Models;

namespace AuthenticationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {

        public readonly ConnectionStrings _ConnectionStrings;

        public AuthenticationController(IOptions<ConnectionStrings> ConnectionStrings)
        {
            _ConnectionStrings = ConnectionStrings.Value;
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register(Credentials credentials)
        {
            int userId;
            if (credentials.email == null || credentials.email == string.Empty)
            {
                return BadRequest("El email es requerido");
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(_ConnectionStrings.WebConnection))
                {
                    connection.Open();

                    string query = string.Format("INSERT INTO [dbo].[User] (Id, Email, Password, CreationDate, ModificationDate, CreationUser, ModificationUser, Status) " +
                "VALUES (NEXT VALUE FOR SequenceUser, '{0}', '{1}', GETDATE(), GETDATE(), 'APICOMM','APICOMM', 'A'); ",
                credentials.email, credentials.password);

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    string querySequence = "SELECT CURRENT_VALUE FROM sys.sequences WHERE name = 'SequenceUser';";

                    using (SqlCommand commandSequence = new SqlCommand(querySequence, connection))
                    {
                        userId = Convert.ToInt32(commandSequence.ExecuteScalar());
                    }
                }
                return Ok(userId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar el archivo: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(Credentials credentials)
        {
            int userId;
            if (credentials.email == null || credentials.email == string.Empty)
            {
                return BadRequest("El email es requerido");
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(_ConnectionStrings.WebConnection))
                {
                    connection.Open();

                    string querySequence = string.Format( "SELECT Id FROM [dbo].[User] WHERE Email = '{0}' AND Password = '{1}';", credentials.email, credentials.password);

                    using (SqlCommand commandSequence = new SqlCommand(querySequence, connection))
                    {
                        userId = Convert.ToInt32(commandSequence.ExecuteScalar());
                    }
                }
                if(userId < 1) return BadRequest("Credenciales invalidas");
                return Ok(userId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar el archivo: {ex.Message}");
            }
        }
    }
}
