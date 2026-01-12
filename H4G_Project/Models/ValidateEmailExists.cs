using System.ComponentModel.DataAnnotations;
using H4G_Project.DAL;
namespace H4G_Project.Models    
{
    public class ValidateEmailExists : ValidationAttribute
    {
        private StaffDAL staffContext = new StaffDAL();
        private MemberDAL memberContext = new MemberDAL();

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string email = Convert.ToString(value);
            if (validationContext.ObjectInstance is Staff)
            {
                Staff staff = (Staff)validationContext.ObjectInstance;
            }
            if (validationContext.ObjectInstance is Member)
            {
                Member member = (Member)validationContext.ObjectInstance;
            }
            if (staffContext.IsEmailExists(email) || memberContext.IsEmailExists(email))
            {
                return new ValidationResult("Email address already exists!");
            }
            else
            {
                return ValidationResult.Success;
            }
           

        }
    }
}