using MongoDB.Driver;

public class ReferralRepository : IReferralRepository
{
    private IMongoDatabase _database;

    public ReferralRepository()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb+srv://admin:admin@cluster0.ctjt6.mongodb.net/USI?retryWrites=true&w=majority");
        var client = new MongoClient(settings);
        _database = client.GetDatabase("USI");
    }

    public async Task CreateReferralForPatient(int id, Referral referral)
    {
        var patients = _database.GetCollection<Patient>("Patients");
        var updatePatients = Builders<Patient>.Update.AddToSet("referrals", referral);
        await patients.UpdateOneAsync(p => p.Id == id, updatePatients);
    }

}