namespace HomemadeLMS.Models.Domain
{
    public enum TeamRole
    {
        TeamWorker,
        Coordinator,
        ResourceInvestigator,

        Shaper,
        Implementer,
        Completer,

        Plant,
        MonitorEvaluator,
    }

    public class RoleTestResult
    {
        public const int AnsCount = 8;
        public const int BlockCount = 7;
        private string username;

        public RoleTestResult(string username)
        {
            Username = username;
            this.username = Username;
        }

        public int CompleterScore { get; set; }
        public int CoordinatorScore { get; set; }
        public int ImplementerScore { get; set; }
        public int MonitorEvaluatorScore { get; set; }
        public int PlantScore { get; set; }
        public int ResourceInvestigatorScore { get; set; }
        public int ShaperScore { get; set; }
        public int TeamWorkerScore { get; set; }

        public string Username
        {
            get => username;
            set
            {
                if (!Account.HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                username = value;
            }
        }

        public static bool IsBlockTotalScoreValid(int score) => score == 10;
        public static bool IsScoreValid(int score) => 0 <= score && score <= 10 && score != 1;

        public void AddScore(int blockIndex, int ansIndex, int score)
        {
            var role = (blockIndex, ansIndex) switch
            {
                (1, 1) => TeamRole.ResourceInvestigator,
                (1, 2) => TeamRole.TeamWorker,
                (1, 3) => TeamRole.Plant,
                (1, 4) => TeamRole.Coordinator,
                (1, 5) => TeamRole.Completer,
                (1, 6) => TeamRole.Shaper,
                (1, 7) => TeamRole.Implementer,
                (1, 8) => TeamRole.MonitorEvaluator,

                (2, 1) => TeamRole.Implementer,
                (2, 2) => TeamRole.Coordinator,
                (2, 3) => TeamRole.ResourceInvestigator,
                (2, 4) => TeamRole.MonitorEvaluator,
                (2, 5) => TeamRole.Shaper,
                (2, 6) => TeamRole.TeamWorker,
                (2, 7) => TeamRole.Plant,
                (2, 8) => TeamRole.Completer,

                (3, 1) => TeamRole.Coordinator,
                (3, 2) => TeamRole.Completer,
                (3, 3) => TeamRole.Shaper,
                (3, 4) => TeamRole.Plant,
                (3, 5) => TeamRole.TeamWorker,
                (3, 6) => TeamRole.ResourceInvestigator,
                (3, 7) => TeamRole.MonitorEvaluator,
                (3, 8) => TeamRole.Implementer,

                (4, 1) => TeamRole.TeamWorker,
                (4, 2) => TeamRole.Shaper,
                (4, 3) => TeamRole.MonitorEvaluator,
                (4, 4) => TeamRole.Implementer,
                (4, 5) => TeamRole.Plant,
                (4, 6) => TeamRole.Completer,
                (4, 7) => TeamRole.ResourceInvestigator,
                (4, 8) => TeamRole.Coordinator,

                (5, 1) => TeamRole.MonitorEvaluator,
                (5, 2) => TeamRole.Implementer,
                (5, 3) => TeamRole.TeamWorker,
                (5, 4) => TeamRole.Shaper,
                (5, 5) => TeamRole.ResourceInvestigator,
                (5, 6) => TeamRole.Coordinator,
                (5, 7) => TeamRole.Completer,
                (5, 8) => TeamRole.Plant,

                (6, 1) => TeamRole.Plant,
                (6, 2) => TeamRole.TeamWorker,
                (6, 3) => TeamRole.Coordinator,
                (6, 4) => TeamRole.Completer,
                (6, 5) => TeamRole.MonitorEvaluator,
                (6, 6) => TeamRole.Implementer,
                (6, 7) => TeamRole.Shaper,
                (6, 8) => TeamRole.ResourceInvestigator,

                (7, 1) => TeamRole.Shaper,
                (7, 2) => TeamRole.MonitorEvaluator,
                (7, 3) => TeamRole.Completer,
                (7, 4) => TeamRole.ResourceInvestigator,
                (7, 5) => TeamRole.Implementer,
                (7, 6) => TeamRole.Plant,
                (7, 7) => TeamRole.Coordinator,
                (7, 8) => TeamRole.TeamWorker,

                _ => TeamRole.TeamWorker,
            };

            SetScore(role, GetScore(role) + score);
        }

        public int GetScore(TeamRole role)
        {
            var score = role switch
            {
                TeamRole.Completer => CompleterScore,
                TeamRole.Coordinator => CoordinatorScore,
                TeamRole.Implementer => ImplementerScore,
                TeamRole.MonitorEvaluator => MonitorEvaluatorScore,
                TeamRole.Plant => PlantScore,
                TeamRole.ResourceInvestigator => ResourceInvestigatorScore,
                TeamRole.Shaper => ShaperScore,
                TeamRole.TeamWorker => TeamWorkerScore,
                _ => throw new ArgumentException("Invalid role value."),
            };
            return score;
        }

        public void SetScore(TeamRole role, int score)
        {
            if (role == TeamRole.Completer)
            {
                CompleterScore = score;
            }
            if (role == TeamRole.Coordinator)
            {
                CoordinatorScore = score;
            }
            if (role == TeamRole.Implementer)
            {
                ImplementerScore = score;
            }
            if (role == TeamRole.MonitorEvaluator)
            {
                MonitorEvaluatorScore = score;
            }
            if (role == TeamRole.Plant)
            {
                PlantScore = score;
            }
            if (role == TeamRole.ResourceInvestigator)
            {
                ResourceInvestigatorScore = score;
            }
            if (role == TeamRole.Shaper)
            {
                ShaperScore = score;
            }
            if (role == TeamRole.TeamWorker)
            {
                TeamWorkerScore = score;
            }
        }

        public Dictionary<TeamRole, int> GetScoreDict()
        {
            var dict = new Dictionary<TeamRole, int>();
            foreach (var role in Enum.GetValues<TeamRole>())
            {
                dict.Add(role, GetScore(role));
            }
            return dict;
        }
    }
}