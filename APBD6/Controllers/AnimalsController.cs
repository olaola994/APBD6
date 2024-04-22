using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial5.Models;
using Tutorial5.Models.DTOs;

namespace Tutorial5.Controllers;

[ApiController]
// [Route("api/animals")]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpGet]
    public IActionResult GetAnimals(string orderBy = null)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT * FROM Animal";
        
        var reader = command.ExecuteReader();
        
        List<Animal> animals = new List<Animal>();

        int idAnimalOrdinal = reader.GetOrdinal("IdAnimal");
        int nameOrdinal = reader.GetOrdinal("Name");
        int descriptionOrdinal = reader.GetOrdinal("Description");
        int categoryOrdinal = reader.GetOrdinal("Category");
        int areaOrdinal = reader.GetOrdinal("Area");
        
        while (reader.Read())
        {
            animals.Add(new Animal()
            {
                IdAnimal = reader.GetInt32(idAnimalOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.GetString(descriptionOrdinal),
                Category = reader.GetString(categoryOrdinal),
                Area = reader.GetString(areaOrdinal)
            });
        }

        List<Animal> orderedList = null;
        if (orderBy == null)
        {
            orderedList = animals.OrderBy(a => a.Name).ToList();
        }
        else
        {
            orderedList = orderBy switch
            {
                "Name" => animals.OrderBy(a => a.Name).ToList(),
                "Description" => animals.OrderBy(a => a.Description).ToList(),
                "Category" => animals.OrderBy(a => a.Category).ToList(),
                "Area" => animals.OrderBy(a => a.Area).ToList(),
                _ => animals.OrderBy(a => a.Name).ToList()
            };
        }

        return Ok(orderedList);
    }

    [HttpPost]
    public IActionResult AddAnimal(Animal animal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT INTO Animal (Name, Description, Category, Area) VALUES(@animalName, @description, @category, @area)";
        command.Parameters.AddWithValue("@animalName", animal.Name);
        command.Parameters.AddWithValue("@description", animal.Description);
        command.Parameters.AddWithValue("@category", animal.Category);
        command.Parameters.AddWithValue("@area", animal.Area);
        command.ExecuteNonQuery();
        
        return Created("", null);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateAnimal(int id, Animal animal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "UPDATE Animal SET Name = @animalName, Description = @description, Category = @category, Area = @area WHERE IdAnimal = @id";
        command.Parameters.AddWithValue("@animalName", animal.Name);
        command.Parameters.AddWithValue("@description", animal.Description);
        command.Parameters.AddWithValue("@category", animal.Category);
        command.Parameters.AddWithValue("@area", animal.Area);
        command.Parameters.AddWithValue("@id", id);

        command.ExecuteNonQuery();
    
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteAnimal(int id)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM Animal WHERE IdAnimal = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            
            transaction.Commit();
    
            return NoContent();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, $"Wystąpił błąd podczas usuwania zwierzęcia: {ex.Message}");
        }
    }
    
}