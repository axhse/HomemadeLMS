using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class NamedRoleScore
    {
        public NamedRoleScore(TeamRole role, int score)
        {
            Role = role;
            Score = score;
        }

        public static string GetRoleName(TeamRole role)
        {
            var name = role switch
            {
                TeamRole.Completer => "Педант",
                TeamRole.Coordinator => "Координатор",
                TeamRole.Implementer => "Исполнитель",
                TeamRole.MonitorEvaluator => "Аналитик-стратег",
                TeamRole.Plant => "Генератор идей",
                TeamRole.ResourceInvestigator => "Исследователь ресурсов",
                TeamRole.Shaper => "Мотиватор",
                TeamRole.TeamWorker => "Душа команды",
                _ => throw new ArgumentException("Invalid role value."),
            };
            return name;
        }

        public static List<NamedRoleScore> GetAllSortedFromTestResult(RoleTestResult testResult)
        {
            var allNamedScore = new List<NamedRoleScore>();
            var dict = testResult.GetScoreDict();
            foreach (var (role, score) in dict)
            {
                allNamedScore.Add(new(role, score));
            }
            allNamedScore.Sort((first, second) => -first.Score.CompareTo(second.Score));
            return allNamedScore;
        }

        public TeamRole Role { get; private set; }
        public int Score { get; private set; }

        public string Name => GetRoleName(Role);
    }
}