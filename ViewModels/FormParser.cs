using HomemadeLMS.Models.Domain;
using Microsoft.Extensions.Primitives;

namespace HomemadeLMS.ViewModels
{
    public class FormParser
    {
        private readonly IFormCollection form;

        public FormParser(IFormCollection form)
        {
            this.form = form;
        }

        public string? GetString(string key)
        {
            if (!form.TryGetValue(key, out StringValues values) || values.Count == 0)
            {
                return null;
            }
            return values[0];
        }

        public bool TryGetInt(string key, out int value)
        {
            return int.TryParse(GetString(key), out value);
        }

        public bool TryGetUserRole(string key, out UserRole value)
        {
            var roleCode = GetString(key);
            foreach (var role in Enum.GetValues<UserRole>())
            {
                if (roleCode == role.ToString())
                {
                    value = role;
                    return true;
                }
            }
            value = UserRole.None;
            return false;
        }

        public bool TryGetCourseRole(string key, out CourseRole value)
        {
            var roleCode = GetString(key);
            foreach (var role in Enum.GetValues<CourseRole>())
            {
                if (roleCode == role.ToString())
                {
                    value = role;
                    return true;
                }
            }
            value = CourseRole.Student;
            return false;
        }
    }
}