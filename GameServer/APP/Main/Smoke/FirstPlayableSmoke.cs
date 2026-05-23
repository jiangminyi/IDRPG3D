using MongoDB.Bson;
using MongoDB.Driver;

namespace Fantasy;

public static class FirstPlayableSmoke
{
    private const string ConnectionString = "mongodb://127.0.0.1:27017";
    private const string DatabaseName = "idrpg3d_dev";

    public static async Task Run()
    {
        var client = new MongoClient(ConnectionString);
        var database = client.GetDatabase(DatabaseName);

        await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));

        var players = database.GetCollection<BsonDocument>("smoke_players");
        var battles = database.GetCollection<BsonDocument>("smoke_idle_battles");
        var teams = database.GetCollection<BsonDocument>("smoke_teams");

        var account = "smoke_player_001";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var playerId = StableId(account);

        await players.ReplaceOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", playerId),
            new BsonDocument
            {
                ["_id"] = playerId,
                ["account"] = account,
                ["name"] = account,
                ["level"] = 1,
                ["teamId"] = 0L,
                ["updatedAt"] = now
            },
            new ReplaceOptions { IsUpsert = true });

        var battleId = StableId($"{account}:battle");
        await battles.ReplaceOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", battleId),
            new BsonDocument
            {
                ["_id"] = battleId,
                ["playerId"] = playerId,
                ["mapId"] = 1,
                ["startTime"] = now - 120,
                ["lastSettleTime"] = now,
                ["exp"] = 20L,
                ["gold"] = 10L,
                ["isRunning"] = false,
                ["rewards"] = new BsonArray
                {
                    new BsonDocument
                    {
                        ["itemId"] = 1001,
                        ["count"] = 2L
                    }
                }
            },
            new ReplaceOptions { IsUpsert = true });

        var teamId = StableId($"{account}:team");
        await teams.ReplaceOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", teamId),
            new BsonDocument
            {
                ["_id"] = teamId,
                ["leaderPlayerId"] = playerId,
                ["memberPlayerIds"] = new BsonArray { playerId },
                ["createdAt"] = now
            },
            new ReplaceOptions { IsUpsert = true });

        await players.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", playerId),
            Builders<BsonDocument>.Update.Set("teamId", teamId).Set("updatedAt", now));

        var savedPlayer = await players.Find(Builders<BsonDocument>.Filter.Eq("_id", playerId)).FirstOrDefaultAsync();
        var savedBattle = await battles.Find(Builders<BsonDocument>.Filter.Eq("_id", battleId)).FirstOrDefaultAsync();
        var savedTeam = await teams.Find(Builders<BsonDocument>.Filter.Eq("_id", teamId)).FirstOrDefaultAsync();

        if (savedPlayer == null || savedBattle == null || savedTeam == null)
        {
            throw new InvalidOperationException("Smoke data was not persisted correctly.");
        }

        Console.WriteLine("First playable smoke passed.");
        Console.WriteLine($"PlayerId={playerId}, BattleId={battleId}, TeamId={teamId}, Database={DatabaseName}");
    }

    private static long StableId(string value)
    {
        unchecked
        {
            var hash = 1469598103934665603L;
            foreach (var c in value)
            {
                hash ^= c;
                hash *= 1099511628211L;
            }

            return Math.Abs(hash);
        }
    }
}
