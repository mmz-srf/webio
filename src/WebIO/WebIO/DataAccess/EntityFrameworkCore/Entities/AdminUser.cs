namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System.ComponentModel.DataAnnotations;

public class AdminUser
{
  [Key] public required string Email { get; set; }
}