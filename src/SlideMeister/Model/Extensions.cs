using System.Text;

namespace SlideMeister.Model
{
    public static class Extensions
    {
        /// <summary>
        /// Converts the machine and its item to a string
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        public static string ConvertToString(this Machine machine)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Machine: {machine.Name}");
            foreach (var item in machine.Items)
            {
                builder.AppendLine($"- {item}");
            }

            return builder.ToString();
        }
       
    }
}