namespace HomemadeLMS.Models.Domain
{
    public class HomeworkStatus
    {
        public const string UidSeparator = ":";
        public static readonly int MaxSubjectIdSize = Math.Max(Account.MaxUsernameSize, Team.TagSize);
        public static readonly int MaxUidSize = DataUtils.MaxNumericStringSize
                                                + UidSeparator.Length + MaxSubjectIdSize;
        private string subjectId;
        private string? evaluatorUsername;
        private string? submitUsername;

        public HomeworkStatus(int homeworkId, string subjectId)
        {
            HomeworkId = homeworkId;
            SubjectId = subjectId;
            this.subjectId = SubjectId;
        }

        public static string BuildUid(int homeworkId, string subjectId)
            => $"{homeworkId}{UidSeparator}{subjectId}";

        public int HomeworkId { get; set; }
        public int? Mark { get; set; }
        public DateTime? SubmitTime { get; set; }

        public string SubjectId
        {
            get => subjectId;
            set
            {
                if (!Account.HasUsernameValidFormat(value) && !Team.HasTagValidFormat(value))
                {
                    throw new ArgumentException("Invalid subjectId format.");
                }
                subjectId = value;
            }
        }

        public string Uid
        {
            get => BuildUid(HomeworkId, SubjectId);
            set { }
        }

        public string? EvaluatorUsername
        {
            get => evaluatorUsername;
            set
            {
                if (value is not null && !Account.HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                evaluatorUsername = value;
            }
        }

        public string? SubmitUsername
        {
            get => submitUsername;
            set
            {
                if (value is not null && !Account.HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                submitUsername = value;
            }
        }

        public bool IsEvaluated => Mark is not null && EvaluatorUsername is not null;
        public bool IsSubmitted => SubmitTime is not null && SubmitUsername is not null;

        public void Evaluate(int mark, string evaluatorUsername)
        {
            Mark = mark;
            EvaluatorUsername = evaluatorUsername;
        }

        public void MarkAsNotSumbitted()
        {
            SubmitUsername = null;
            SubmitTime = null;
        }

        public void MarkAsSumbitted(string username)
        {
            SubmitUsername = username;
            SubmitTime = DateTime.UtcNow;
        }

        public void ResetEvaluation()
        {
            Mark = null;
            EvaluatorUsername = null;
        }
    }
}