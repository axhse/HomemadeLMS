namespace HomemadeLMS.Services
{
    public class Token
    {
        public Token(string id, string value) : this(id, value, TimeSpan.MaxValue)
        { }

        public Token(string id, string value, TimeSpan lifeTime)
        {
            Id = id;
            Value = value;
            LifeTime = lifeTime;
            CreationTime = DateTime.UtcNow;
        }

        public string Id { get; private set; }
        public string Value { get; private set; }
        public DateTime CreationTime { get; private set; }
        public TimeSpan LifeTime { get; private set; }

        public bool IsExpired => DateTime.UtcNow - CreationTime > LifeTime;
    }

    public static class RandomSeedSource
    {
        public static int Next() => (int)DateTime.UtcNow.Ticks;
    }

    public class TokenValueSource
    {
        private readonly Random randomGenerator;

        public TokenValueSource()
        {
            randomGenerator = new Random(RandomSeedSource.Next());
        }

        public string Next()
        {
            var bytes = new byte[32];
            randomGenerator.NextBytes(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }
    }

    public class TokenService
    {
        private readonly List<Token> tokens = new();
        private readonly TokenValueSource tokenValueSource;

        public TokenService() : this(TimeSpan.MaxValue)
        { }

        public TokenService(TimeSpan tokenLifeTime)
        {
            tokenValueSource = new();
            TokenLifeTime = tokenLifeTime;
        }

        public TimeSpan TokenLifeTime { get; private set; }

        public string Add(string id)
        {
            var tokenValue = tokenValueSource.Next();
            var token = new Token(id, tokenValue, TokenLifeTime);
            tokens.Add(token);
            return tokenValue;
        }

        public string? FetchId(string? tokenValue)
        {
            var id = TouchId(tokenValue);
            tokens.RemoveAll(token => token.Value == tokenValue);
            return id;
        }

        public string? TouchId(string? tokenValue)
        {
            if (tokenValue is null)
            {
                return null;
            }
            tokens.RemoveAll(token => token.IsExpired);
            return tokens.FirstOrDefault(token => token.Value == tokenValue)?.Id;
        }

        public bool TryFetchId(string? tokenValue, out string id)
        {
            var nullableId = FetchId(tokenValue);
            id = nullableId ?? string.Empty;
            return nullableId is not null;
        }

        public bool TryTouchId(string? tokenValue, out string id)
        {
            var nullableId = TouchId(tokenValue);
            id = nullableId ?? string.Empty;
            return nullableId is not null;
        }
    }
}